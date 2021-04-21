/***
*
*	Copyright (c) 1996-2002, Valve LLC. All rights reserved.
*
*	This product contains software technology licensed from Id
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc.
*	All Rights Reserved.
*
*   Use, distribution, and modification of this source code and/or resulting
*   object code is restricted to non-commercial enhancements to products from
*   Valve LLC.  All other use, distribution, or modification is prohibited
*   without written permission from Valve LLC.
*
****/
/*
===== h_cycler.cpp ========================================================
  The Halflife Cycler Monsters
*/

#include "extdll.h"
#include "util.h"
#include "cbase.h"
#include "monsters.h"
#include "animation.h"
#include "weapons.h"
#include "player.h"
// ESHQ
#include "func_break.h"

#define HC_CYCLER_PASSABLE	0x8

#define TEMP_FOR_SCREEN_SHOTS
#ifdef TEMP_FOR_SCREEN_SHOTS

class CCycler: public CBaseMonster
	{
	public:
		void GenericCyclerSpawn (char* szModel, Vector vecMin, Vector vecMax);
		virtual int	ObjectCaps (void) { return (CBaseEntity::ObjectCaps () | FCAP_IMPULSE_USE); }
		int TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType);
		void Spawn (void);
		void Think (void);

		// ESHQ: поддержка звуков ударов
		void CCycler::DamageSound (void);

		void Use (CBaseEntity* pActivator, CBaseEntity* pCaller, USE_TYPE useType, float value);

		// Don't treat as a live target
		virtual BOOL IsAlive (void) { return FALSE; }

		virtual int		Save (CSave& save);
		virtual int		Restore (CRestore& restore);
		static	TYPEDESCRIPTION m_SaveData[];

		// ESHQ: поддержка звуков ударов
		void KeyValue (KeyValueData* pkvd);

		int			m_animate;
		Materials	m_Material;
	};

// ESHQ: контроль корректности задания параметров
#define CHECK_CYCLER_SIZE(coord)	\
	if (abs (pev->startpos.coord - pev->endpos.coord) < 2.0f)	\
		{	\
		pev->startpos.coord -= 1.0f;	\
		pev->endpos.coord += 1.0f;	\
		}	\
	else if (pev->startpos.coord > pev->endpos.coord)	\
		{	\
		vec_t coord = pev->startpos.coord;	\
		pev->startpos.coord = pev->endpos.coord;	\
		pev->endpos.coord = coord;	\
		}

TYPEDESCRIPTION	CCycler::m_SaveData[] =
	{
	DEFINE_FIELD (CCycler, m_animate, FIELD_INTEGER),
	DEFINE_FIELD (CCycler, m_Material, FIELD_INTEGER),
	};

IMPLEMENT_SAVERESTORE (CCycler, CBaseMonster);

// ESHQ: поддержка возможности задания физического размера модели
void CCycler::KeyValue (KeyValueData* pkvd)
	{
	if (FStrEq (pkvd->szKeyName, "MinPoint"))
		{
		Vector tmp;
		UTIL_StringToVector ((float*)tmp, pkvd->szValue);
		pev->startpos = tmp;
		}
	else if (FStrEq (pkvd->szKeyName, "MaxPoint"))
		{
		Vector tmp;
		UTIL_StringToVector ((float*)tmp, pkvd->szValue);
		pev->endpos = tmp;
		}
	else if (FStrEq (pkvd->szKeyName, "material"))
		{
		int i = atoi (pkvd->szValue);

		if ((i < 0) || (i >= matLastMaterial))
			m_Material = matMetal;
		else
			m_Material = (Materials)i;

		pkvd->fHandled = TRUE;
		}
	else
		{
		CBaseMonster::KeyValue (pkvd);
		}
	}

// we should get rid of all the other cyclers and replace them with this.
class CGenericCycler: public CCycler
	{
	public:
		void Spawn (void) { GenericCyclerSpawn ((char*)STRING (pev->model), Vector (-16, -16, 0), Vector (16, 16, 72)); }
	};
LINK_ENTITY_TO_CLASS (cycler, CGenericCycler);
LINK_ENTITY_TO_CLASS (env_model, CGenericCycler);	// ESHQ: совместимость с AOMDC

// Cycler member functions
void CCycler::GenericCyclerSpawn (char* szModel, Vector vecMin, Vector vecMax)
	{
	if (!szModel || !*szModel)
		{
		ALERT (at_error, "cycler at %.0f %.0f %0.f missing modelname", pev->origin.x, pev->origin.y, pev->origin.z);
		REMOVE_ENTITY (ENT (pev));
		return;
		}

	pev->classname = MAKE_STRING ("cycler");
	PRECACHE_MODEL (szModel);
	SET_MODEL (ENT (pev), szModel);

	CCycler::Spawn ();

	// Контроль размеров
	if (!FBitSet (pev->spawnflags, HC_CYCLER_PASSABLE))
		{
		CHECK_CYCLER_SIZE (x);
		CHECK_CYCLER_SIZE (y);
		CHECK_CYCLER_SIZE (z);

		UTIL_SetSize (pev, pev->startpos, pev->endpos);
		}
	}

void CCycler::Spawn ()
	{
	InitBoneControllers ();
	pev->solid = SOLID_SLIDEBOX;
	pev->movetype = MOVETYPE_NONE;
	pev->takedamage = DAMAGE_YES;
	pev->effects = 0;
	pev->health = 80000;
	pev->yaw_speed = 5;
	pev->ideal_yaw = pev->angles.y;
	ChangeYaw (360);
	m_flFrameRate = 75;
	m_flGroundSpeed = 0;
	m_bloodColor = DONT_BLEED;	// ESHQ: странно, когда, например, деревья брызгают жёлтой кровью

	pev->nextthink += 1.0;

	ResetSequenceInfo ();

	if (/*pev->sequence != 0 ||*/ pev->frame != 0)	// ESHQ: непонятно, зачем было отключать анимацию на ненулевых секъюэнсах
		{
		m_animate = 0;
		pev->framerate = 0;
		}
	else
		{
		m_animate = 1;
		}
	}

// cycler think
void CCycler::Think (void)
	{
	pev->nextthink = gpGlobals->time + 0.1;

	if (m_animate)
		StudioFrameAdvance ();

	if (m_fSequenceFinished && !m_fSequenceLoops)
		{
		// hack to avoid reloading model every frame
		pev->animtime = gpGlobals->time;
		pev->framerate = 1.0;
		m_fSequenceFinished = FALSE;
		m_flLastEventCheck = gpGlobals->time;
		pev->frame = 0;

		if (!m_animate)
			pev->framerate = 0.0;	// FIX: don't reset framerate
		}
	}

// CyclerUse - starts a rotation trend
void CCycler::Use (CBaseEntity* pActivator, CBaseEntity* pCaller, USE_TYPE useType, float value)
	{
	m_animate = !m_animate;
	if (m_animate)
		pev->framerate = 1.0;
	else
		pev->framerate = 0.0;

	// ESHQ: активация привязанной цели
	SUB_UseTargets (NULL, USE_TOGGLE, 0);
	}

// ESHQ: обработка звукового сопровождения
// Почти полная копия из CBreakable
void CCycler::DamageSound (void)		
	{
	int pitch;
	float fvol;
	char* rgpsz[6];
	int i;
	int material = m_Material;

	// Отмена звука, если cycler нематериальный
	if (FBitSet (pev->spawnflags, HC_CYCLER_PASSABLE))
		return;

	// Настройка звука
	if (RANDOM_LONG (0, 2))
		pitch = PITCH_NORM;
	else
		pitch = 95 + RANDOM_LONG (0, 34);

	fvol = RANDOM_FLOAT (0.85, 1.0);

	if (material == matComputer && RANDOM_LONG (0, 1))
		material = matMetal;

	switch (material)
		{
		case matComputer:
		case matGlass:
		case matUnbreakableGlass:
			rgpsz[0] = "debris/glass1.wav";
			rgpsz[1] = "debris/glass2.wav";
			rgpsz[2] = "debris/glass3.wav";
			i = 3;
			break;

		case matWood:
			rgpsz[0] = "debris/wood5.wav";
			rgpsz[1] = "debris/wood6.wav";
			rgpsz[2] = "debris/wood7.wav";
			i = 3;
			break;

		case matMetal:
			rgpsz[0] = "player/pl_metal5.wav";
			rgpsz[1] = "player/pl_metal6.wav";
			rgpsz[2] = "player/pl_metal7.wav";
			rgpsz[3] = "player/pl_metal8.wav";
			i = 4;
			break;

		case matFlesh:
			rgpsz[0] = "debris/flesh2.wav";
			rgpsz[1] = "debris/flesh3.wav";
			rgpsz[2] = "debris/flesh4.wav";
			rgpsz[3] = "debris/flesh5.wav";
			rgpsz[4] = "debris/flesh6.wav";
			rgpsz[5] = "debris/flesh7.wav";
			i = 6;
			break;

		case matRocks:
		case matCinderBlock:
			rgpsz[0] = "debris/concrete1.wav";
			rgpsz[1] = "debris/concrete2.wav";
			rgpsz[2] = "debris/concrete3.wav";
			i = 3;
			break;

		case matCeilingTile:
			i = 0;
			break;
		}

	if (i)
		{
		EMIT_SOUND_DYN (ENT (pev), CHAN_VOICE, rgpsz[RANDOM_LONG (0, i - 1)], fvol, ATTN_MEDIUM, 0, pitch);
		}
	}

// Обработка получения урона
int CCycler::TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType)
	{
	if (m_animate)
		{
		ResetSequenceInfo ();
		pev->frame = 0;
		}
	else
		{
		pev->framerate = 1.0;
		StudioFrameAdvance (0.1);
		pev->framerate = 0;
		ALERT (at_console, "sequence: %d, frame %.0f\n", pev->sequence, pev->frame);
		}

	// Звук материала
	DamageSound ();

	return 0;
	}

#endif

// ESHQ: устаревший объект
class CCyclerSprite: public CBaseEntity
	{
	public:
		void Spawn (void);
		void Think (void);
		void Use (CBaseEntity* pActivator, CBaseEntity* pCaller, USE_TYPE useType, float value);
		virtual int	ObjectCaps (void) { return (CBaseEntity::ObjectCaps () | FCAP_IMPULSE_USE); }
		virtual int	TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType);
		void	Animate (float frames);

		virtual int		Save (CSave& save);
		virtual int		Restore (CRestore& restore);
		static	TYPEDESCRIPTION m_SaveData[];

		inline int		ShouldAnimate (void) { return (m_animate && (m_maxFrame > 1.0)); }
		int			m_animate;
		float		m_lastTime;
		float		m_maxFrame;
	};

LINK_ENTITY_TO_CLASS (cycler_sprite, CCyclerSprite);

TYPEDESCRIPTION	CCyclerSprite::m_SaveData[] =
	{
		DEFINE_FIELD (CCyclerSprite, m_animate, FIELD_INTEGER),
		DEFINE_FIELD (CCyclerSprite, m_lastTime, FIELD_TIME),
		DEFINE_FIELD (CCyclerSprite, m_maxFrame, FIELD_FLOAT),
	};

IMPLEMENT_SAVERESTORE (CCyclerSprite, CBaseEntity);

void CCyclerSprite::Spawn (void)
	{
	pev->solid = SOLID_SLIDEBOX;
	pev->movetype = MOVETYPE_NONE;
	pev->takedamage = DAMAGE_YES;
	pev->effects = 0;

	pev->frame = 0;
	pev->nextthink = gpGlobals->time + 0.1;
	m_animate = 1;
	m_lastTime = gpGlobals->time;

	PRECACHE_MODEL ((char*)STRING (pev->model));
	SET_MODEL (ENT (pev), STRING (pev->model));

	m_maxFrame = (float)MODEL_FRAMES (pev->modelindex) - 1;
	}

void CCyclerSprite::Think (void)
	{
	if (ShouldAnimate ())
		Animate (pev->framerate * (gpGlobals->time - m_lastTime));

	pev->nextthink = gpGlobals->time + 0.1;
	m_lastTime = gpGlobals->time;
	}

void CCyclerSprite::Use (CBaseEntity* pActivator, CBaseEntity* pCaller, USE_TYPE useType, float value)
	{
	m_animate = !m_animate;
	ALERT (at_console, "Sprite: %s\n", STRING (pev->model));
	}

int	CCyclerSprite::TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType)
	{
	if (m_maxFrame > 1.0)
		Animate (1.0);

	return 1;
	}

void CCyclerSprite::Animate (float frames)
	{
	pev->frame += frames;
	if (m_maxFrame > 0)
		pev->frame = fmod (pev->frame, m_maxFrame);
	}

class CWeaponCycler: public CBasePlayerWeapon
	{
	public:
		void Spawn (void);
		int iItemSlot (void) { return 1; }
		int GetItemInfo (ItemInfo* p) { return 0; }

		void PrimaryAttack (void);
		void SecondaryAttack (void);
		BOOL Deploy (void);
		void Holster (int skiplocal = 0);
		int m_iszModel;
		int m_iModel;
	};

LINK_ENTITY_TO_CLASS (cycler_weapon, CWeaponCycler);

void CWeaponCycler::Spawn ()
	{
	pev->solid = SOLID_SLIDEBOX;
	pev->movetype = MOVETYPE_NONE;

	PRECACHE_MODEL ((char*)STRING (pev->model));
	SET_MODEL (ENT (pev), STRING (pev->model));
	m_iszModel = pev->model;
	m_iModel = pev->modelindex;

	UTIL_SetOrigin (pev, pev->origin);
	UTIL_SetSize (pev, Vector (-16, -16, 0), Vector (16, 16, 16));
	SetTouch (&CBasePlayerItem::DefaultTouch);
	}

BOOL CWeaponCycler::Deploy ()
	{
	m_pPlayer->pev->viewmodel = m_iszModel;
	m_pPlayer->m_flNextAttack = UTIL_WeaponTimeBase () + 1.0;
	SendWeaponAnim (0);
	m_iClip = 0;
	return TRUE;
	}

void CWeaponCycler::Holster (int skiplocal)
	{
	m_pPlayer->m_flNextAttack = UTIL_WeaponTimeBase () + 0.5;
	}

void CWeaponCycler::PrimaryAttack ()
	{
	SendWeaponAnim (pev->sequence);

	m_flNextPrimaryAttack = gpGlobals->time + 0.3;
	}

void CWeaponCycler::SecondaryAttack (void)
	{
	float flFrameRate, flGroundSpeed;

	pev->sequence = (pev->sequence + 1) % 8;

	pev->modelindex = m_iModel;
	void* pmodel = GET_MODEL_PTR (ENT (pev));
	GetSequenceInfo (pmodel, pev, &flFrameRate, &flGroundSpeed);
	pev->modelindex = 0;

	if (flFrameRate == 0.0)
		pev->sequence = 0;

	SendWeaponAnim (pev->sequence);

	m_flNextSecondaryAttack = gpGlobals->time + 0.3;
	}

// Flaming Wreckage
class CWreckage: public CBaseMonster
	{
	int		Save (CSave& save);
	int		Restore (CRestore& restore);
	static	TYPEDESCRIPTION m_SaveData[];

	void Spawn (void);
	void Precache (void);
	void Think (void);

	int m_flStartTime;
	};

TYPEDESCRIPTION	CWreckage::m_SaveData[] =
	{
		DEFINE_FIELD (CWreckage, m_flStartTime, FIELD_TIME),
	};

IMPLEMENT_SAVERESTORE (CWreckage, CBaseMonster);

LINK_ENTITY_TO_CLASS (cycler_wreckage, CWreckage);

void CWreckage::Spawn (void)
	{
	pev->solid = SOLID_NOT;
	pev->movetype = MOVETYPE_NONE;
	pev->takedamage = 0;
	pev->effects = 0;

	pev->frame = 0;
	pev->nextthink = gpGlobals->time + 0.1;

	if (pev->model)
		{
		PRECACHE_MODEL ((char*)STRING (pev->model));
		SET_MODEL (ENT (pev), STRING (pev->model));
		}
	
	m_flStartTime = gpGlobals->time;
	}

void CWreckage::Precache ()
	{
	if (pev->model)
		PRECACHE_MODEL ((char*)STRING (pev->model));
	}

void CWreckage::Think (void)
	{
	StudioFrameAdvance ();
	pev->nextthink = gpGlobals->time + 0.2;

	if (pev->dmgtime)
		{
		if (pev->dmgtime < gpGlobals->time)
			{
			UTIL_Remove (this);
			return;
			}
		else if (RANDOM_FLOAT (0, pev->dmgtime - m_flStartTime) > pev->dmgtime - gpGlobals->time)
			{
			return;
			}
		}

	Vector VecSrc;

	VecSrc.x = RANDOM_FLOAT (pev->absmin.x, pev->absmax.x);
	VecSrc.y = RANDOM_FLOAT (pev->absmin.y, pev->absmax.y);
	VecSrc.z = RANDOM_FLOAT (pev->absmin.z, pev->absmax.z);

	MESSAGE_BEGIN (MSG_PVS, SVC_TEMPENTITY, VecSrc);
	WRITE_BYTE (TE_SMOKE);
	WRITE_COORD (VecSrc.x);
	WRITE_COORD (VecSrc.y);
	WRITE_COORD (VecSrc.z);
	WRITE_SHORT (g_sModelIndexSmoke);
	WRITE_BYTE (RANDOM_LONG (0, 49) + 50); // scale * 10
	WRITE_BYTE (RANDOM_LONG (0, 3) + 8); // framerate
	MESSAGE_END ();
	}
