/***
*
*	Copyright (c) 1996-2002, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology"). Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
* This source code contains proprietary and confidential information of
* Valve LLC and its suppliers. Access to this code is restricted to
* persons who have executed a written SDK license with Valve. Any access,
* use or distribution of this code by or to any unlicensed person is illegal.
*
****/
//=========================================================
// GMan - misunderstood servant of the people
//=========================================================
#include	"extdll.h"
#include	"util.h"
#include	"cbase.h"
#include	"monsters.h"
#include	"schedule.h"
#include	"weapons.h"
#include	"talkmonster.h"
#include	"scripted.h"
#include	"soundent.h"

//=========================================================
// Monster's Anim Events Go Here
//=========================================================

class CGMan : public CTalkMonster
	{
	public:
		void Spawn (void);
		void KeyValue (KeyValueData *pkvd);
		void Precache (void);
		void SetYawSpeed (void);
		int Classify (void);
		void HandleAnimEvent (MonsterEvent_t *pEvent);
		int ISoundMask (void);

		/*int	Save (CSave &save); 
		int Restore (CRestore &restore);
		static TYPEDESCRIPTION m_SaveData[];*/

		/*void TalkInit (void);
		void StartTask (Task_t *pTask);
		void RunTask (Task_t *pTask);*/
		int TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType);
		void TraceAttack (entvars_t *pevAttacker, float flDamage, Vector vecDir, TraceResult *ptr, int bitsDamageType);

		/*void PlayScriptedSentence (const char *pszSentence, float duration, float volume, float attenuation, BOOL bConcurrent, CBaseEntity *pListener);

		EHANDLE m_hTalkTarget;
		float m_flTalkTime;*/
		EHANDLE m_hPlayer;
	};

LINK_ENTITY_TO_CLASS (monster_gman, CGMan);

/*TYPEDESCRIPTION	CGMan::m_SaveData[] = 
	{
	DEFINE_FIELD (CGMan, m_hTalkTarget, FIELD_EHANDLE),
	DEFINE_FIELD (CGMan, m_flTalkTime, FIELD_TIME),
	};
IMPLEMENT_SAVERESTORE (CGMan, CTalkMonster);*/


//=========================================================
// Classify - indicates this monster's place in the 
// relationship table.
//=========================================================
int	CGMan :: Classify (void)
	{
	return	CLASS_NONE;
	}

//=========================================================
// SetYawSpeed - allows each sequence to have a different
// turn rate associated with it.
//=========================================================
void CGMan :: SetYawSpeed (void)
	{
	int ys;

	switch (m_Activity)
		{
		case ACT_IDLE:
		default:
			ys = 90;
		}

	pev->yaw_speed = ys;
	}

//=========================================================
// HandleAnimEvent - catches the monster-specific messages
// that occur when tagged animation frames are played.
//=========================================================
void CGMan :: HandleAnimEvent (MonsterEvent_t *pEvent)
	{
	switch (pEvent->event)
		{
		case 0:
		default:
			CBaseMonster::HandleAnimEvent (pEvent);
			break;
		}
	}

//=========================================================
// ISoundMask - generic monster can't hear.
//=========================================================
int CGMan :: ISoundMask (void)
	{
	return NULL;
	}

//=========================================================
// Spawn
//=========================================================
void CGMan :: Spawn()
	{
	Precache ();	// Обязательно до переопределния скина, т.к. опирается на него

	switch (pev->skin)
		{
		default:
			pev->skin = 0;	// Принудительное переназначение в случае несоответствия диапазону

		// Чёрный и белый gman
		case 0:
		case 1:
			SET_MODEL (ENT(pev), "models/gman.mdl");
			UTIL_SetSize(pev, VEC_HUMAN_HULL_MIN, VEC_HUMAN_HULL_MAX);
			break;

		// Псевдо-гордон
		case 2:
			pev->skin = 0;
			SET_MODEL (ENT(pev), "models/player.mdl");
			UTIL_SetSize(pev, Vector (-16, -16, -36), Vector (16, 16, 36));
			break;
		}

	pev->solid			= SOLID_SLIDEBOX;
	pev->movetype		= MOVETYPE_STEP;
	m_bloodColor		= DONT_BLEED;
	pev->health			= 100;
	m_flFieldOfView		= 0.5;// indicates the width of this monster's forward view cone (as a dotproduct result)
	m_MonsterState		= MONSTERSTATE_NONE;

	CTalkMonster::MonsterInit();
	}

void CGMan::KeyValue (KeyValueData *pkvd)
	{
	if (FStrEq(pkvd->szKeyName, "skin"))
		{
		pev->skin = atoi(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else 
		CTalkMonster::KeyValue (pkvd);
	}
//=========================================================
// Precache - precaches all resources this monster needs
//=========================================================
void CGMan :: Precache()
	{
	if (pev->skin == 2)
		PRECACHE_MODEL ("models/player.mdl");
	else
		PRECACHE_MODEL ("models/gman.mdl");

	CTalkMonster::TalkInit();
	CTalkMonster::Precache();
	}	


// Функционал поддержки завершающей сцены HL1
/*void CGMan :: StartTask (Task_t *pTask)
	{
	switch (pTask->iTask)
		{
		case TASK_WAIT:
			if (m_hPlayer == NULL)
				{
				m_hPlayer = UTIL_FindEntityByClassname (NULL, "player");
				}
			break;
		}

	CTalkMonster::StartTask (pTask);
	}

void CGMan :: RunTask (Task_t *pTask)
	{
	switch (pTask->iTask)
		{
		case TASK_WAIT:
			// look at who I'm talking to
			if (m_flTalkTime > gpGlobals->time && m_hTalkTarget != NULL)
				{
				float yaw = VecToYaw(m_hTalkTarget->pev->origin - pev->origin) - pev->angles.y;

				if (yaw > 180) yaw -= 360;
				if (yaw < -180) yaw += 360;

				// turn towards vector
				SetBoneController (0, yaw);
				}
			// look at player, but only if playing a "safe" idle animation
			else if (m_hPlayer != NULL && pev->sequence == 0)
				{
				float yaw = VecToYaw(m_hPlayer->pev->origin - pev->origin) - pev->angles.y;

				if (yaw > 180) yaw -= 360;
				if (yaw < -180) yaw += 360;

				// turn towards vector
				SetBoneController (0, yaw);
				}
			else 
				{
				SetBoneController (0, 0);
				}
			CTalkMonster::RunTask (pTask);
			break;

		default:
			SetBoneController (0, 0);
			CTalkMonster::RunTask (pTask);
			break;
		}
	}*/


// GMan теперь может получать урон, но только если это разрешено spawn-флагом
int CGMan :: TakeDamage (entvars_t* pevInflictor, entvars_t* pevAttacker, float flDamage, int bitsDamageType)
	{
	if ((pev->spawnflags & 32) == 0)
		{
		pev->health = pev->max_health / 2; // always trigger the 50% damage aitrigger
		}

	if (flDamage > 0)
		{
		SetConditions(bits_COND_LIGHT_DAMAGE);
		}

	if (flDamage >= 20)
		{
		SetConditions(bits_COND_HEAVY_DAMAGE);
		}

	if (pev->spawnflags & 32)
		{
		return CBaseMonster::TakeDamage (pevInflictor, pevAttacker, flDamage, bitsDamageType);
		}
	else
		{
		return TRUE;
		}
	}


void CGMan::TraceAttack (entvars_t *pevAttacker, float flDamage, Vector vecDir, TraceResult *ptr, int bitsDamageType)
	{
	UTIL_Ricochet (ptr->vecEndPos, 1.0);
	AddMultiDamage (pevAttacker, this, flDamage, bitsDamageType);
	}

// Функционал поддержки завершающей сцены HL1
/*void CGMan::PlayScriptedSentence (const char *pszSentence, float duration, float volume, float attenuation, BOOL bConcurrent, CBaseEntity *pListener)
	{
	CTalkMonster::PlayScriptedSentence (pszSentence, duration, volume, attenuation, bConcurrent, pListener);

	m_flTalkTime = gpGlobals->time + duration;
	m_hTalkTarget = pListener;
	}*/
