/*
pm_surface.c - surface tracing
Copyright (C) 2010 Uncle Mike

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

#include "common.h"
#include "mathlib.h"
#include "pm_local.h"
#include "gl_local.h"

// 4529
#define FRAC_EPSILON	(1.0f / 32.0f)

typedef struct
	{
	float		fraction;
	int			contents;
	// 4529
	msurface_t* surface;
	} linetrace_t;


/*
==============
4529: fix_coord
converts the reletive tex coords to absolute
==============
*/
static uint fix_coord (vec_t in, uint width)
	{
	if (in > 0) 
		return (uint)in % width;

	return width - ((uint)fabs ((double)in) % width);
	}

/*
=============
4529: SampleMiptex
fence texture testing
=============
*/
int PM_SampleMiptex (const msurface_t* surf, const vec3_t point)
	{
	mextrasurf_t* info = surf->info;
	mfacebevel_t* fb = info->bevel;
	int		contents;
	vec_t		ds, dt;
	byte* data;
	int		x, y;
	mtexinfo_t* tx;
	texture_t* mt;
	rgbdata_t* src;

	// fill the default contents
	if (fb) contents = fb->contents;
	else contents = CONTENTS_SOLID;

	if (!surf->texinfo || !surf->texinfo->texture)
		return contents;

	tx = surf->texinfo;
	mt = tx->texture;

	if (mt->name[0] != '{')
		return contents;

	src = R_GetTexture (mt->gl_texturenum)->original;
	if (!src) return contents; // original doesn't kept

	ds = DotProduct (point, tx->vecs[0]) + tx->vecs[0][3];
	dt = DotProduct (point, tx->vecs[1]) + tx->vecs[1][3];

	// convert ST to real pixels position
	x = fix_coord (ds, mt->width - 1);
	y = fix_coord (dt, mt->height - 1);

	ASSERT (x >= 0 && y >= 0);

	data = src->buffer;
	if (data[(mt->width * y) + x] == 255)
		return CONTENTS_EMPTY;
	return CONTENTS_SOLID;
	}

/*
==================
4529: PM_RecursiveSurfCheck
==================
*/
//msurface_t* PM_RecursiveSurfCheck (model_t* model, mnode_t* node, vec3_t p1, vec3_t p2)
msurface_t* PM_RecursiveSurfCheck (model_t* mod, mnode_t* node, vec3_t p1, vec3_t p2)
	{
	float		t1, t2, frac;
	/*int		side, ds, dt;
	mplane_t*	plane;*/
	int			i, side;
	msurface_t* surf;
	vec3_t		mid;
	//int		i;

loc0:
	if (node->contents < 0)
		return NULL;

	//plane = node->plane;
	t1 = PlaneDiff (p1, node->plane);
	t2 = PlaneDiff (p2, node->plane);

	//if (plane->type < 3)
	if (t1 >= -FRAC_EPSILON && t2 >= -FRAC_EPSILON)
		{
		/*t1 = p1[plane->type] - plane->dist;
		t2 = p2[plane->type] - plane->dist;*/
		node = node->children[0];
		goto loc0;
		}
	
	//else
	if ((t1 < FRAC_EPSILON) && (t2 < FRAC_EPSILON))
		{
		/*t1 = DotProduct (plane->normal, p1) - plane->dist;
		t2 = DotProduct (plane->normal, p2) - plane->dist;*/
		node = node->children[1];
		goto loc0;
		}

	/*if (t1 >= 0.0f && t2 >= 0.0f)
		return PM_RecursiveSurfCheck (model, node->children[0], p1, p2);
	if (t1 < 0.0f && t2 < 0.0f)
		return PM_RecursiveSurfCheck (model, node->children[1], p1, p2);*/
	side = (t1 < 0.0f);

	frac = t1 / (t1 - t2);

	/*if (frac < 0.0f) frac = 0.0f;
	if (frac > 1.0f) frac = 1.0f;*/
	frac = bound (0.0f, frac, 1.0f);

	VectorLerp (p1, frac, p2, mid);

	/*side = (t1 < 0.0f);

	// now this is weird.
	surf = PM_RecursiveSurfCheck (model, node->children[side], p1, mid);*/
	if ((surf = PM_RecursiveSurfCheck (mod, node->children[side], p1, mid)) != NULL)
		return surf;

	// 4529: walk through real faces
	//if (surf != NULL || (t1 >= 0.0f && t2 >= 0.0f) || (t1 < 0.0f && t2 < 0.0f))
	for (i = 0; i < node->numsurfaces; i++)
		{
		/*return surf;
		}*/
		msurface_t* surf = &mod->surfaces[node->firstsurface + i];
		mextrasurf_t* info = surf->info;
		mfacebevel_t* fb = info->bevel;
		int		j, contents;
		vec3_t		delta;

		//surf = model->surfaces + node->firstsurface;
		if (!fb) continue;	// ???

		/*for (i = 0; i < node->numsurfaces; i++, surf++)
		{
		ds = (int)((float)DotProduct (mid, surf->texinfo->vecs[0]) + surf->texinfo->vecs[0][3]);
		dt = (int)((float)DotProduct (mid, surf->texinfo->vecs[1]) + surf->texinfo->vecs[1][3]);*/
		VectorSubtract (mid, fb->origin, delta);
		if (DotProduct (delta, delta) >= fb->radius)
			continue;	// no intersection

		//if (ds >= surf->texturemins[0] && dt >= surf->texturemins[1])
		for (j = 0; j < fb->numedges; j++)
			{
			/*int s = ds - surf->texturemins[0];
			int t = dt - surf->texturemins[1];

			if (s <= surf->extents[0] && t <= surf->extents[1])
				return surf;*/
			if (PlaneDiff (mid, &fb->edges[j]) > FRAC_EPSILON)
				break; // outside the bounds
			}

		if (j != fb->numedges)
			continue; // we are outside the bounds of the facet

		// hit the surface
		contents = PM_SampleMiptex (surf, mid);

		if (contents != CONTENTS_EMPTY)
			return surf;

		return NULL; // through the fence
		}

	//return PM_RecursiveSurfCheck (model, node->children[side ^ 1], mid, p2);
	return PM_RecursiveSurfCheck (mod, node->children[side ^ 1], mid, p2);
	}

/*
==================
PM_TraceTexture

find the face where the traceline hit
assume physentity is valid
==================
*/
msurface_t* PM_TraceSurface (physent_t* pe, vec3_t start, vec3_t end)
	{
	matrix4x4		matrix;
	model_t* bmodel;
	hull_t* hull;
	vec3_t		start_l, end_l;
	vec3_t		offset;

	bmodel = pe->model;

	if (!bmodel || bmodel->type != mod_brush)
		return NULL;

	hull = &pe->model->hulls[0];
	VectorSubtract (hull->clip_mins, vec3_origin, offset);
	VectorAdd (offset, pe->origin, offset);

	VectorSubtract (start, offset, start_l);
	VectorSubtract (end, offset, end_l);

	// rotate start and end into the models frame of reference
	if (!VectorIsNull (pe->angles))
		{
		Matrix4x4_CreateFromEntity (matrix, pe->angles, offset, 1.0f);
		Matrix4x4_VectorITransform (matrix, start, start_l);
		Matrix4x4_VectorITransform (matrix, end, end_l);
		}

	return PM_RecursiveSurfCheck (bmodel, &bmodel->nodes[hull->firstclipnode], start_l, end_l);
	}

/*
==================
PM_TraceTexture

find the face where the traceline hit
assume physentity is valid
==================
*/
const char* PM_TraceTexture (physent_t* pe, vec3_t start, vec3_t end)
	{
	msurface_t* surf = PM_TraceSurface (pe, start, end);

	if (!surf || !surf->texinfo || !surf->texinfo->texture)
		return NULL;

	return surf->texinfo->texture->name;
	}

/*
==================
PM_TestLine_r
4529: optimized trace for light gathering
==================
*/
//int PM_TestLine_r (mnode_t* node, vec_t p1f, vec_t p2f, const vec3_t start, const vec3_t stop, linetrace_t* trace)
int PM_TestLine_r (model_t* mod, mnode_t* node, vec_t p1f, vec_t p2f, const vec3_t start, 
	const vec3_t stop, linetrace_t* trace)
	{
	float	front, back;
	float	frac, midf;
	//int	r, side;
	int		i, r, side;
	vec3_t	mid;

loc0:
	if (node->contents < 0)
	/*trace->contents = node->contents;
	if (node->contents == CONTENTS_SOLID)
		return CONTENTS_SOLID;
	if (node->contents == CONTENTS_SKY)
		return CONTENTS_SKY;
	if (node->contents < 0)*/
		{
		// water, slime or lava interpret as empty
		if (node->contents == CONTENTS_SOLID)
			return CONTENTS_SOLID;
		if (node->contents == CONTENTS_SKY)
			return CONTENTS_SKY;
		trace->fraction = 1.0f;

		return CONTENTS_EMPTY;
		}

	front = PlaneDiff (start, node->plane);
	back = PlaneDiff (stop, node->plane);

	//if (front >= -ON_EPSILON && back >= -ON_EPSILON)
	if ((front >= -FRAC_EPSILON) && (back >= -FRAC_EPSILON))
		{
		node = node->children[0];
		goto loc0;
		}

	//if (front < ON_EPSILON && back < ON_EPSILON)
	if ((front < FRAC_EPSILON) && (back < FRAC_EPSILON))
		{
		node = node->children[1];
		goto loc0;
		}

	side = (front < 0);
	frac = front / (front - back);
	frac = bound (0.0, frac, 1.0);

	VectorLerp (start, frac, stop, mid);
	midf = p1f + (p2f - p1f) * frac;

	//r = PM_TestLine_r (node->children[side], p1f, midf, start, mid, trace);
	r = PM_TestLine_r (mod, node->children[side], p1f, midf, start, mid, trace);

	if (r != CONTENTS_EMPTY)
		{
		//trace->fraction = midf;
		if (trace->surface == NULL)
			trace->fraction = midf;
		trace->contents = r;
		return r;
		}

	//return PM_TestLine_r (node->children[!side], midf, p2f, mid, stop, trace);
	
	// 4529: walk through real faces
	for (i = 0; i < node->numsurfaces; i++)
		{
		msurface_t* surf = &mod->surfaces[node->firstsurface + i];
		mextrasurf_t* info = surf->info;
		mfacebevel_t* fb = info->bevel;
		int		j, contents;
		vec3_t		delta;

		if (!fb) continue;

		VectorSubtract (mid, fb->origin, delta);
		if (DotProduct (delta, delta) >= fb->radius)
			continue;	// no intersection

		for (j = 0; j < fb->numedges; j++)
			{
			if (PlaneDiff (mid, &fb->edges[j]) > FRAC_EPSILON)
				break; // outside the bounds
			}

		if (j != fb->numedges)
			continue; // we are outside the bounds of the facet

		// hit the surface
		contents = PM_SampleMiptex (surf, mid);

		// fill the trace and out
		trace->contents = contents;
		trace->fraction = midf;

		if (contents != CONTENTS_EMPTY)
			trace->surface = surf;

		return contents;
		}

	return PM_TestLine_r (mod, node->children[!side], midf, p2f, mid, stop, trace);
	}

int PM_TestLineExt (playermove_t* pmove, physent_t* ents, int numents, const vec3_t start, const vec3_t end, int flags)
	{
	linetrace_t		trace, trace_bbox;
	matrix4x4		matrix;
	hull_t*			hull = NULL;
	vec3_t			offset, start_l, end_l;
	qboolean		rotated;
	physent_t*		pe;
	int				i;

	trace.contents = CONTENTS_EMPTY;
	trace.fraction = 1.0f;
	// 4529
	trace.surface = NULL;

	for (i = 0; i < numents; i++)
		{
		pe = &ents[i];

		// 4529
		//if (i != 0 && (flags & PM_WORLD_ONLY))
		if ((i != 0) && FBitSet (flags, PM_WORLD_ONLY))
			break;

		if (!pe->model || (pe->model->type != mod_brush) || (pe->solid != SOLID_BSP))
			continue;

		// 4529
		//if (pe->rendermode != kRenderNormal)
		if (FBitSet (flags, PM_GLASS_IGNORE) && pe->rendermode != kRenderNormal)
			continue;

		hull = &pe->model->hulls[0];

		hull = PM_HullForBsp (pe, pmove, offset);

		if (pe->solid == SOLID_BSP && !VectorIsNull (pe->angles))
			rotated = true;
		else rotated = false;

		if (rotated)
			{
			Matrix4x4_CreateFromEntity (matrix, pe->angles, offset, 1.0f);
			Matrix4x4_VectorITransform (matrix, start, start_l);
			Matrix4x4_VectorITransform (matrix, end, end_l);
			}
		else
			{
			VectorSubtract (start, pe->origin, start_l);
			VectorSubtract (end, pe->origin, end_l);
			}

		trace_bbox.contents = CONTENTS_EMPTY;
		trace_bbox.fraction = 1.0f;
		// 4529
		trace_bbox.surface = NULL;

		//PM_TestLine_r (&pe->model->nodes[hull->firstclipnode], 0.0f, 1.0f, start_l, end_l, &trace_bbox);
		PM_TestLine_r (pe->model, &pe->model->nodes[hull->firstclipnode], 0.0f, 1.0f, start_l, end_l, &trace_bbox);

		if (trace_bbox.contents != CONTENTS_EMPTY || trace_bbox.fraction < trace.fraction)
			{
			trace = trace_bbox;
			}
		}

	return trace.contents;
	}
