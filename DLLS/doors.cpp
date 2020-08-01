/***
*
*	Copyright (c) 1996-2002, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology"). Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
* Use, distribution, and modification of this source code and/or resulting
* object code is restricted to non-commercial enhancements to products from
* Valve LLC. All other use, distribution, or modification is prohibited
* without written permission from Valve LLC.
*
****/
/*

===== doors.cpp ========================================================

*/

#include "extdll.h"
#include "util.h"
#include "cbase.h"
#include "doors.h"


extern void SetMovedir(entvars_t* ev);

// Переопределение имён переменных, в которых будут храниться звуки
#define noiseMoving noise1
#define noiseArrived noise2
#define noiseReturned noise3

class CBaseDoor : public CBaseToggle
	{
	public:
		void Spawn (void);
		void Precache (void);
		virtual void KeyValue (KeyValueData *pkvd);
		virtual void Use (CBaseEntity *pActivator, CBaseEntity *pCaller, USE_TYPE useType, float value);
		virtual void Blocked (CBaseEntity *pOther);


		virtual int	ObjectCaps (void) 
			{ 
			if (pev->spawnflags & SF_ITEM_USE_ONLY)
				return (CBaseToggle::ObjectCaps() & ~FCAP_ACROSS_TRANSITION) | FCAP_IMPULSE_USE;
			else
				return (CBaseToggle::ObjectCaps() & ~FCAP_ACROSS_TRANSITION);
			};
		virtual int	Save (CSave &save);
		virtual int	Restore (CRestore &restore);

		static	TYPEDESCRIPTION m_SaveData[];

		virtual void SetToggleState (int state);

		// used to selectivly override defaults
		void EXPORT DoorTouch (CBaseEntity *pOther);

		// local functions
		int DoorActivate ();
		void EXPORT DoorGoUp (void);
		void EXPORT DoorGoDown (void);
		void EXPORT DoorHitTop (void);
		void EXPORT DoorHitBottom (void);

		BYTE	m_bHealthValue;// some doors are medi-kit doors, they give players health

		BYTE	m_bMoveSnd;			// sound a door makes while moving
		BYTE	m_bStopSnd;			// Основной звук остановки/Звук остановки в открытом состоянии
		BYTE	m_bStop2Snd;		// Звук остановки в закрытом состоянии

		locksound_t m_ls;			// door lock sounds

		BYTE	m_bLockedSound;		// ordinals from entity selection
		BYTE	m_bLockedSentence;	
		BYTE	m_bUnlockedSound;	
		BYTE	m_bUnlockedSentence;
	};


TYPEDESCRIPTION	CBaseDoor::m_SaveData[] = 
	{
	DEFINE_FIELD (CBaseDoor, m_bHealthValue, FIELD_CHARACTER),
	DEFINE_FIELD (CBaseDoor, m_bMoveSnd, FIELD_CHARACTER),
	DEFINE_FIELD (CBaseDoor, m_bStopSnd, FIELD_CHARACTER),
	DEFINE_FIELD (CBaseDoor, m_bStop2Snd, FIELD_CHARACTER),

	DEFINE_FIELD (CBaseDoor, m_bLockedSound, FIELD_CHARACTER),
	DEFINE_FIELD (CBaseDoor, m_bLockedSentence, FIELD_CHARACTER),
	DEFINE_FIELD (CBaseDoor, m_bUnlockedSound, FIELD_CHARACTER),	
	DEFINE_FIELD (CBaseDoor, m_bUnlockedSentence, FIELD_CHARACTER),	

	};

IMPLEMENT_SAVERESTORE (CBaseDoor, CBaseToggle);


#define DOOR_SENTENCEWAIT	6
#define DOOR_SOUNDWAIT		2
#define BUTTON_SOUNDWAIT	0.5

// play door or button locked or unlocked sounds. 
// pass in pointer to valid locksound struct. 
// if flocked is true, play 'door is locked' sound,
// otherwise play 'door is unlocked' sound
// NOTE: this routine is shared by doors and buttons

void PlayLockSounds(entvars_t *pev, locksound_t *pls, int flocked, int fbutton)
	{
	// LOCKED SOUND

	// CONSIDER: consolidate the locksound_t struct (all entries are duplicates for lock/unlock)
	// CONSIDER: and condense this code.
	float flsoundwait;

	if (fbutton)
		flsoundwait = BUTTON_SOUNDWAIT;
	else
		flsoundwait = DOOR_SOUNDWAIT;

	if (flocked)
		{
		int fplaysound = (pls->sLockedSound && gpGlobals->time > pls->flwaitSound);
		int fplaysentence = (pls->sLockedSentence && !pls->bEOFLocked && gpGlobals->time > pls->flwaitSentence);
		float fvol;

		if (fplaysound && fplaysentence)
			fvol = 0.25;
		else
			fvol = 1.0;

		// if there is a locked sound, and we've debounced, play sound
		if (fplaysound)
			{
			// play 'door locked' sound
			EMIT_SOUND(ENT(pev), CHAN_ITEM, (char*)STRING(pls->sLockedSound), fvol, ATTN_MEDIUM);
			pls->flwaitSound = gpGlobals->time + flsoundwait;
			}

		// if there is a sentence, we've not played all in list, and we've debounced, play sound
		if (fplaysentence)
			{
			// play next 'door locked' sentence in group
			int iprev = pls->iLockedSentence;

			pls->iLockedSentence = SENTENCEG_PlaySequentialSz(ENT(pev), STRING(pls->sLockedSentence), 
				0.85, ATTN_MEDIUM, 0, 100, pls->iLockedSentence, FALSE);
			pls->iUnlockedSentence = 0;

			// make sure we don't keep calling last sentence in list
			pls->bEOFLocked = (iprev == pls->iLockedSentence);

			pls->flwaitSentence = gpGlobals->time + DOOR_SENTENCEWAIT;
			}
		}
	else
		{
		// UNLOCKED SOUND

		int fplaysound = (pls->sUnlockedSound && gpGlobals->time > pls->flwaitSound);
		int fplaysentence = (pls->sUnlockedSentence && !pls->bEOFUnlocked && gpGlobals->time > pls->flwaitSentence);
		float fvol;

		// if playing both sentence and sound, lower sound volume so we hear sentence
		if (fplaysound && fplaysentence)
			fvol = 0.25;
		else
			fvol = 1.0;

		// play 'door unlocked' sound if set
		if (fplaysound)
			{
			EMIT_SOUND(ENT(pev), CHAN_ITEM, (char*)STRING(pls->sUnlockedSound), fvol, ATTN_MEDIUM);
			pls->flwaitSound = gpGlobals->time + flsoundwait;
			}

		// play next 'door unlocked' sentence in group
		if (fplaysentence)
			{
			int iprev = pls->iUnlockedSentence;

			pls->iUnlockedSentence = SENTENCEG_PlaySequentialSz(ENT(pev), STRING(pls->sUnlockedSentence), 
				0.85, ATTN_MEDIUM, 0, 100, pls->iUnlockedSentence, FALSE);
			pls->iLockedSentence = 0;

			// make sure we don't keep calling last sentence in list
			pls->bEOFUnlocked = (iprev == pls->iUnlockedSentence);
			pls->flwaitSentence = gpGlobals->time + DOOR_SENTENCEWAIT;
			}
		}
	}

//
// Cache user-entity-field values until spawn is called.
//

void CBaseDoor::KeyValue (KeyValueData *pkvd)
	{

	if (FStrEq(pkvd->szKeyName, "skin"))//skin is used for content type
		{
		pev->skin = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "movesnd"))
		{
		m_bMoveSnd = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "stopsnd"))
		{
		m_bStopSnd = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	// Для обработки звука закрытия
	else if (FStrEq(pkvd->szKeyName, "stop2snd"))
		{
		m_bStop2Snd = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	//
	else if (FStrEq(pkvd->szKeyName, "healthvalue"))
		{
		m_bHealthValue = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "locked_sound"))
		{
		m_bLockedSound = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "locked_sentence"))
		{
		m_bLockedSentence = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "unlocked_sound"))
		{
		m_bUnlockedSound = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "unlocked_sentence"))
		{
		m_bUnlockedSentence = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "WaveHeight"))
		{
		pev->scale = atof(pkvd->szValue) * (1.0/8.0);
		pkvd->fHandled = TRUE;
		}
	else
		CBaseToggle::KeyValue (pkvd);
	}

/*QUAKED func_door (0 .5 .8) ? START_OPEN x DOOR_DONT_LINK TOGGLE
if two doors touch, they are assumed to be connected and operate as a unit.

TOGGLE causes the door to wait in both the start and end states for a trigger event.

START_OPEN causes the door to move to its destination when spawned, and operate in reverse.
It is used to temporarily or permanently close off an area when triggered (not usefull for
touch or takedamage doors).

"angle" determines the opening direction
"targetname"	if set, no touch field will be spawned and a remote button or trigger
field activates the door.
"health" if set, door must be shot open
"speed" movement speed (100 default)
"wait" wait before returning (3 default, -1 = never return)
"lip" lip remaining at end of move (8 default)
"dmg" damage to inflict when blocked (2 default)
"sounds"
0) no sound
1) stone
2) base
3) stone chain
4) screechy metal
*/

LINK_ENTITY_TO_CLASS (func_door, CBaseDoor);
//
// func_water - same as a door. 
//
LINK_ENTITY_TO_CLASS (func_water, CBaseDoor);


void CBaseDoor::Spawn ()
	{
	Precache();
	SetMovedir (pev);

	if (pev->skin == 0)
		{//normal door
		if (FBitSet (pev->spawnflags, SF_DOOR_PASSABLE))
			pev->solid		= SOLID_NOT;
		else
			pev->solid		= SOLID_BSP;
		}
	else
		{// special contents
		pev->solid		= SOLID_NOT;
		SetBits (pev->spawnflags, SF_DOOR_SILENT);	// water is silent for now
		}

	pev->movetype	= MOVETYPE_PUSH;
	UTIL_SetOrigin(pev, pev->origin);
	SET_MODEL (ENT(pev), STRING(pev->model));

	if (pev->speed == 0)
		pev->speed = 100;

	m_vecPosition1	= pev->origin;		// По умолчанию, позиция "закрыто"

	// Subtract 2 from size because the engine expands bboxes by 1 in all directions making the size too big
	m_vecPosition2	= m_vecPosition1 + (pev->movedir * (fabs (pev->movedir.x * (pev->size.x-2)) + fabs (pev->movedir.y * (pev->size.y-2)) + fabs (pev->movedir.z * (pev->size.z-2)) - m_flLip));

	ASSERTSZ(m_vecPosition1 != m_vecPosition2, "door start/end positions are equal");

	if (FBitSet (pev->spawnflags, SF_DOOR_START_OPEN))
		{	// swap pos1 and pos2, put door at pos2
		UTIL_SetOrigin(pev, m_vecPosition2);
		m_vecPosition2 = m_vecPosition1;
		m_vecPosition1 = pev->origin;
		}

	m_toggle_state = TS_AT_BOTTOM;

	// if the door is flagged for USE button activation only, use NULL touch function
	if (FBitSet (pev->spawnflags, SF_DOOR_USE_ONLY))
		{
		SetTouch (NULL);
		}
	else // touchable button
		SetTouch (&CBaseDoor::DoorTouch);
	}


void CBaseDoor :: SetToggleState (int state)
	{
	if (state == TS_AT_TOP)
		UTIL_SetOrigin (pev, m_vecPosition2);
	else
		UTIL_SetOrigin (pev, m_vecPosition1);
	}


void CBaseDoor::Precache (void)
	{
	char *pszSound;
	char precacheBuf[32];
	precacheBuf[0] = '\0';

	// set the door's "in-motion" sound
	switch (m_bMoveSnd)
		{
		case	0:
		default:
			pev->noiseMoving = ALLOC_STRING("common/null.wav");
			break;

		case	1:
		case	2:
		case	3:
		case	4:
		case	5:
		case	6:
		case	7:
		case	8:
		case	11:
		case	12:
		case	13:
			sprintf (precacheBuf, "doors/doormove%i.wav", m_bMoveSnd);
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;

		case	9:
			sprintf (precacheBuf, "doors/doormove9%c.wav", 'a' + RANDOM_LONG (0, 2));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;

		case	10:
			sprintf (precacheBuf, "doors/doormove10%c.wav", 'a' + RANDOM_LONG (0, 1));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;
		}

	// Установка звука остановки в открытом состоянии/Общего звука остановки
	switch (m_bStopSnd)
		{
		case	0:
		default:
			pev->noiseArrived = ALLOC_STRING("common/null.wav");
			break;

		case	1:
		case	2:
		case	3:
		case	4:
		case	5:
		case	6:
		case	7:
		case	8:
		case	9:
		case	11:
		case	12:
		//case	13:
			sprintf (precacheBuf, "doors/doorstop%i.wav", m_bStopSnd);
			PRECACHE_SOUND (precacheBuf);
			pev->noiseArrived = ALLOC_STRING(precacheBuf);
			break;

		case	10:
			sprintf (precacheBuf, "doors/doorstop10%c.wav", 'a' + RANDOM_LONG (0, 1));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseArrived = ALLOC_STRING(precacheBuf);
			break;
		}

	// Установка звука остановки в закрытом состоянии (если есть)
	switch (m_bStop2Snd)
		{
		// Для обеспечения совместимости состояние "нет звука" передвинуто
		case	-1:
		default:
			pev->noiseReturned = ALLOC_STRING("common/null.wav");
			break;

			// Эмуляция отсутствия этой настройки
		case	0:
			// Звук уже был кэширован
			pev->noiseReturned = ALLOC_STRING ((char*)STRING(pev->noiseArrived));
			break;

			// Далее - стандартная процедура
		case	1:
		case	2:
		case	3:
		case	4:
		case	5:
		case	6:
		case	7:
		case	8:
		case	9:
		case	11:
		case	12:
		//case	13:
			sprintf (precacheBuf, "doors/doorstop%i.wav", m_bStop2Snd);
			PRECACHE_SOUND (precacheBuf);
			pev->noiseReturned = ALLOC_STRING(precacheBuf);
			break;

		case	10:
			sprintf (precacheBuf, "doors/doorstop10%c.wav", 'a' + RANDOM_LONG (0, 1));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseReturned = ALLOC_STRING(precacheBuf);
			break;
		}

	// Контроль обратного состояния
	if (FBitSet (pev->spawnflags, SF_DOOR_START_OPEN))
		{
		string_t ArrRet = pev->noiseReturned;
		pev->noiseReturned = pev->noiseArrived;
		pev->noiseArrived = ArrRet;
		}

	// get door button sounds, for doors which are directly 'touched' to open
	if (m_bLockedSound)
		{
		pszSound = ButtonSound ((int)m_bLockedSound);
		PRECACHE_SOUND(pszSound);
		m_ls.sLockedSound = ALLOC_STRING(pszSound);
		}

	if (m_bUnlockedSound)
		{
		pszSound = ButtonSound ((int)m_bUnlockedSound);
		PRECACHE_SOUND(pszSound);
		m_ls.sUnlockedSound = ALLOC_STRING(pszSound);
		}

	// get sentence group names, for doors which are directly 'touched' to open

	switch (m_bLockedSentence)
		{
		case 1: m_ls.sLockedSentence = ALLOC_STRING("NA"); break; // access denied
		case 2: m_ls.sLockedSentence = ALLOC_STRING("ND"); break; // security lockout
		case 3: m_ls.sLockedSentence = ALLOC_STRING("NF"); break; // blast door
		case 4: m_ls.sLockedSentence = ALLOC_STRING("NFIRE"); break; // fire door
		case 5: m_ls.sLockedSentence = ALLOC_STRING("NCHEM"); break; // chemical door
		case 6: m_ls.sLockedSentence = ALLOC_STRING("NRAD"); break; // radiation door
		case 7: m_ls.sLockedSentence = ALLOC_STRING("NCON"); break; // gen containment
		case 8: m_ls.sLockedSentence = ALLOC_STRING("NH"); break; // maintenance door
		case 9: m_ls.sLockedSentence = ALLOC_STRING("NG"); break; // broken door

		default: m_ls.sLockedSentence = 0; break;
		}

	switch (m_bUnlockedSentence)
		{
		case 1: m_ls.sUnlockedSentence = ALLOC_STRING("EA"); break; // access granted
		case 2: m_ls.sUnlockedSentence = ALLOC_STRING("ED"); break; // security door
		case 3: m_ls.sUnlockedSentence = ALLOC_STRING("EF"); break; // blast door
		case 4: m_ls.sUnlockedSentence = ALLOC_STRING("EFIRE"); break; // fire door
		case 5: m_ls.sUnlockedSentence = ALLOC_STRING("ECHEM"); break; // chemical door
		case 6: m_ls.sUnlockedSentence = ALLOC_STRING("ERAD"); break; // radiation door
		case 7: m_ls.sUnlockedSentence = ALLOC_STRING("ECON"); break; // gen containment
		case 8: m_ls.sUnlockedSentence = ALLOC_STRING("EH"); break; // maintenance door

		default: m_ls.sUnlockedSentence = 0; break;
		}
	}

//
// Doors not tied to anything (e.g. button, another door) can be touched, to make them activate.
//
void CBaseDoor::DoorTouch (CBaseEntity *pOther)
	{
	entvars_t*	pevToucher = pOther->pev;

	// Ignore touches by anything but players
	if (!FClassnameIs(pevToucher, "player"))
		return;

	// If door has master, and it's not ready to trigger, 
	// play 'locked' sound

	if (m_sMaster && !UTIL_IsMasterTriggered(m_sMaster, pOther))
		PlayLockSounds(pev, &m_ls, TRUE, FALSE);

	// If door is somebody's target, then touching does nothing.
	// You have to activate the owner (e.g. button).

	if (!FStringNull(pev->targetname))
		{
		// Контроль на открытие
		if ((m_toggle_state == TS_AT_BOTTOM) && !FBitSet (pev->spawnflags, SF_DOOR_START_OPEN) ||
			(m_toggle_state == TS_AT_TOP) && FBitSet (pev->spawnflags, SF_DOOR_START_OPEN))
			{
			// play locked sound
			PlayLockSounds(pev, &m_ls, TRUE, FALSE);
			}
		return; 
		}

	m_hActivator = pOther;// remember who activated the door

	if (DoorActivate ())
		SetTouch (NULL); // Temporarily disable the touch function, until movement is finished.
	}


//
// Used by SUB_UseTargets, when a door is the target of a button.
//
void CBaseDoor::Use (CBaseEntity *pActivator, CBaseEntity *pCaller, USE_TYPE useType, float value)
	{
	m_hActivator = pActivator;
	// if not ready to be used, ignore "use" command.
	if (m_toggle_state == TS_AT_BOTTOM || FBitSet(pev->spawnflags, SF_DOOR_NO_AUTO_RETURN) && m_toggle_state == TS_AT_TOP)
		DoorActivate();
	}

//
// Causes the door to "do its thing", i.e. start moving, and cascade activation.
//
int CBaseDoor::DoorActivate ()
	{
	if (!UTIL_IsMasterTriggered(m_sMaster, m_hActivator))
		return 0;

	if (FBitSet(pev->spawnflags, SF_DOOR_NO_AUTO_RETURN) && m_toggle_state == TS_AT_TOP)
		// door should close
		{

		// play door unlock sounds (исправление на случай DOOR_START_OPEN) 
		if (FBitSet(pev->spawnflags, SF_DOOR_START_OPEN))
			{
			PlayLockSounds(pev, &m_ls, FALSE, FALSE);
			}

		DoorGoDown();
		}
	else
		// door should open
		{

		if (m_hActivator != NULL && m_hActivator->IsPlayer())
			{// give health if player opened the door (medikit)
			// VARS (m_eoActivator)->health += m_bHealthValue;
			m_hActivator->TakeHealth (m_bHealthValue, DMG_GENERIC);
			}

		// play door unlock sounds (исправление на случай DOOR_START_OPEN) 
		if (!FBitSet(pev->spawnflags, SF_DOOR_START_OPEN))
			{
			PlayLockSounds(pev, &m_ls, FALSE, FALSE);
			}

		DoorGoUp();
		}

	return 1;
	}

extern Vector VecBModelOrigin (entvars_t* pevBModel);

//
// Starts the door going to its "up" position (simply ToggleData->vecPosition2).
//
void CBaseDoor::DoorGoUp (void)
	{
	entvars_t	*pevActivator;

	// It could be going-down, if blocked.
	ASSERT(m_toggle_state == TS_AT_BOTTOM || m_toggle_state == TS_GOING_DOWN);

	// emit door moving and stop sounds on CHAN_STATIC so that the multicast doesn't
	// filter them out and leave a client stuck with looping door sounds!
	if (!FBitSet (pev->spawnflags, SF_DOOR_SILENT))
		EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving), 1, ATTN_MEDIUM);

	m_toggle_state = TS_GOING_UP;

	SetMoveDone (&CBaseDoor::DoorHitTop);
	if (FClassnameIs(pev, "func_door_rotating"))		// !!! BUGBUG Triggered doors don't work with this yet
		{
		float	sign = 1.0;

		if (m_hActivator != NULL)
			{
			pevActivator = m_hActivator->pev;

			if (!FBitSet (pev->spawnflags, SF_DOOR_ONEWAY) && pev->movedir.y) 		// Y axis rotation, move away from the player
				{
				Vector vec = pevActivator->origin - pev->origin;
				Vector angles = pevActivator->angles;
				angles.x = 0;
				angles.z = 0;
				UTIL_MakeVectors (angles);
				//			Vector vnext = (pevToucher->origin + (pevToucher->velocity * 10)) - pev->origin;
				UTIL_MakeVectors (pevActivator->angles);
				Vector vnext = (pevActivator->origin + (gpGlobals->v_forward * 10)) - pev->origin;
				if ((vec.x*vnext.y - vec.y*vnext.x) < 0)
					sign = -1.0;
				}
			}
		AngularMove(m_vecAngle2*sign, pev->speed);
		}
	else
		LinearMove(m_vecPosition2, pev->speed);
	}


//
// The door has reached the "up" position. Either go back down, or wait for another activation.
//
void CBaseDoor::DoorHitTop (void)
	{
	if (!FBitSet (pev->spawnflags, SF_DOOR_SILENT))
		{
		STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
		EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseArrived), 1, ATTN_MEDIUM);
		}

	ASSERT(m_toggle_state == TS_GOING_UP);
	m_toggle_state = TS_AT_TOP;

	// toggle-doors don't come down automatically, they wait for refire.
	if (FBitSet(pev->spawnflags, SF_DOOR_NO_AUTO_RETURN))
		{
		// Re-instate touch method, movement is complete
		if (!FBitSet (pev->spawnflags, SF_DOOR_USE_ONLY))
			SetTouch (&CBaseDoor::DoorTouch);
		}
	else
		{
		// In flWait seconds, DoorGoDown will fire, unless wait is -1, then door stays open
		pev->nextthink = pev->ltime + m_flWait;
		SetThink (&CBaseDoor::DoorGoDown);

		if (m_flWait == -1)
			{
			pev->nextthink = -1;
			}
		}

	// Fire the close target (if startopen is set, then "top" is closed) - netname is the close target
	if (pev->netname && (pev->spawnflags & SF_DOOR_START_OPEN))
		FireTargets (STRING(pev->netname), m_hActivator, this, USE_TOGGLE, 0);

	SUB_UseTargets (m_hActivator, USE_TOGGLE, 0); // this isn't finished
	}


//
// Starts the door going to its "down" position (simply ToggleData->vecPosition1).
//
void CBaseDoor::DoorGoDown (void)
	{
	if (!FBitSet (pev->spawnflags, SF_DOOR_SILENT))
		EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving), 1, ATTN_MEDIUM);

#ifdef DOOR_ASSERT
	ASSERT(m_toggle_state == TS_AT_TOP);
#endif // DOOR_ASSERT
	m_toggle_state = TS_GOING_DOWN;

	SetMoveDone (&CBaseDoor::DoorHitBottom);
	if (FClassnameIs(pev, "func_door_rotating"))//rotating door
		AngularMove (m_vecAngle1, pev->speed);
	else
		LinearMove (m_vecPosition1, pev->speed);
	}

//
// The door has reached the "down" position. Back to quiescence.
//
void CBaseDoor::DoorHitBottom (void)
	{
	if (!FBitSet (pev->spawnflags, SF_DOOR_SILENT))
		{
		STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
		EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseReturned), 1, ATTN_MEDIUM);
		}

	ASSERT(m_toggle_state == TS_GOING_DOWN);
	m_toggle_state = TS_AT_BOTTOM;

	// Re-instate touch method, cycle is complete
	if (FBitSet (pev->spawnflags, SF_DOOR_USE_ONLY))
		{// use only door
		SetTouch (NULL);
		}
	else // touchable door
		SetTouch (&CBaseDoor::DoorTouch);

	SUB_UseTargets (m_hActivator, USE_TOGGLE, 0); // this isn't finished

	// Fire the close target (if startopen is set, then "top" is closed) - netname is the close target
	if (pev->netname && !(pev->spawnflags & SF_DOOR_START_OPEN))
		FireTargets (STRING(pev->netname), m_hActivator, this, USE_TOGGLE, 0);
	}

void CBaseDoor::Blocked (CBaseEntity *pOther)
	{
	edict_t	*pentTarget = NULL;
	CBaseDoor	*pDoor		= NULL;

	// Остановка предыдущего звука (без этого циклические звуки "застревают" в дверях)
	if (!FBitSet (pev->spawnflags, SF_DOOR_SILENT))
		{
		STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
		}

	// Hurt the blocker a little.
	if (pev->dmg)
		pOther->TakeDamage (pev, pev, pev->dmg, DMG_CRUSH);

	// if a door has a negative wait, it would never come back if blocked,
	// so let it just squash the object to death real fast

	if (m_flWait >= 0)
		{
		if (m_toggle_state == TS_GOING_DOWN)
			{
			DoorGoUp();
			}
		else
			{
			DoorGoDown();
			}
		}
	}


/*QUAKED FuncRotDoorSpawn (0 .5 .8) ? START_OPEN REVERSE 
DOOR_DONT_LINK TOGGLE X_AXIS Y_AXIS
if two doors touch, they are assumed to be connected and operate as 
a unit.

TOGGLE causes the door to wait in both the start and end states for 
a trigger event.

START_OPEN causes the door to move to its destination when spawned, 
and operate in reverse. It is used to temporarily or permanently 
close off an area when triggered (not usefull for touch or 
takedamage doors).

You need to have an origin brush as part of this entity. The 
center of that brush will be
the point around which it is rotated. It will rotate around the Z 
axis by default. You can
check either the X_AXIS or Y_AXIS box to change that.

"distance" is how many degrees the door will be rotated.
"speed" determines how fast the door moves; default value is 100.

REVERSE will cause the door to rotate in the opposite direction.

"angle"		determines the opening direction
"targetname" if set, no touch field will be spawned and a remote 
button or trigger field activates the door.
"health"	if set, door must be shot open
"speed"		movement speed (100 default)
"wait"		wait before returning (3 default, -1 = never return)
"dmg"		damage to inflict when blocked (2 default)
"sounds"
0)	no sound
1)	stone
2)	base
3)	stone chain
4)	screechy metal
*/
class CRotDoor : public CBaseDoor
	{
	public:
		void Spawn (void);
		virtual void SetToggleState (int state);
	};

LINK_ENTITY_TO_CLASS (func_door_rotating, CRotDoor);


void CRotDoor::Spawn (void)
	{
	Precache();
	// set the axis of rotation
	CBaseToggle::AxisDir (pev);

	// check for clockwise rotation
	if (FBitSet (pev->spawnflags, SF_DOOR_ROTATE_BACKWARDS))
		pev->movedir = pev->movedir * -1;

	//m_flWait			= 2; who the hell did this? (sjb)
	m_vecAngle1	= pev->angles;
	m_vecAngle2	= pev->angles + pev->movedir * m_flMoveDistance;

	ASSERTSZ(m_vecAngle1 != m_vecAngle2, "rotating door start/end positions are equal");

	if (FBitSet (pev->spawnflags, SF_DOOR_PASSABLE))
		pev->solid		= SOLID_NOT;
	else
		pev->solid		= SOLID_BSP;

	pev->movetype	= MOVETYPE_PUSH;
	UTIL_SetOrigin(pev, pev->origin);
	SET_MODEL(ENT(pev), STRING(pev->model));

	if (pev->speed == 0)
		pev->speed = 100;

	// DOOR_START_OPEN is to allow an entity to be lighted in the closed position
	// but spawn in the open position
	if (FBitSet (pev->spawnflags, SF_DOOR_START_OPEN))
		{	// swap pos1 and pos2, put door at pos2, invert movement direction
		pev->angles = m_vecAngle2;
		m_vecAngle2 = m_vecAngle1;	// Исправляем ошибку с обменом переменных (исходный вариант устанавливает им одинаковые значения)
		m_vecAngle1 = pev->angles;
		pev->movedir = pev->movedir * -1;
		}

	m_toggle_state = TS_AT_BOTTOM;

	if (FBitSet (pev->spawnflags, SF_DOOR_USE_ONLY))
		{
		SetTouch (NULL);
		}
	else // touchable button
		SetTouch (&CBaseDoor::DoorTouch);
	}


void CRotDoor :: SetToggleState (int state)
	{
	if (state == TS_AT_TOP)
		pev->angles = m_vecAngle2;
	else
		pev->angles = m_vecAngle1;

	UTIL_SetOrigin (pev, pev->origin);
	}


class CMomentaryDoor : public CBaseToggle
	{
	public:
		void	Spawn (void);
		void Precache (void);
		void EXPORT MomentaryMoveDone (void);

		void	KeyValue (KeyValueData *pkvd);
		void	Use (CBaseEntity *pActivator, CBaseEntity *pCaller, USE_TYPE useType, float value);
		virtual int	ObjectCaps (void) { return CBaseToggle :: ObjectCaps() & ~FCAP_ACROSS_TRANSITION; }

		virtual int	Save (CSave &save);
		virtual int	Restore (CRestore &restore);
		static	TYPEDESCRIPTION m_SaveData[];

		BYTE	m_bMoveSnd;			// sound a door makes while moving	
		BYTE	m_bStopSnd;			// sound a door makes when it stops
		float	oldSoundValue;		// Значение, используемое для контроля состояния звука двери
	};

LINK_ENTITY_TO_CLASS (momentary_door, CMomentaryDoor);

TYPEDESCRIPTION	CMomentaryDoor::m_SaveData[] = 
	{
	DEFINE_FIELD (CMomentaryDoor, m_bMoveSnd, FIELD_CHARACTER),
	DEFINE_FIELD (CMomentaryDoor, m_bStopSnd, FIELD_CHARACTER),
	};

IMPLEMENT_SAVERESTORE (CMomentaryDoor, CBaseToggle);

void CMomentaryDoor::Spawn (void)
	{
	SetMovedir (pev);

	pev->solid		= SOLID_BSP;
	pev->movetype	= MOVETYPE_PUSH;

	UTIL_SetOrigin(pev, pev->origin);
	SET_MODEL (ENT(pev), STRING(pev->model));

	if (pev->speed == 0)
		pev->speed = 100;
	if (pev->dmg == 0)
		pev->dmg = 2;

	m_vecPosition1	= pev->origin;
	// Subtract 2 from size because the engine expands bboxes by 1 in all directions making the size too big
	m_vecPosition2	= m_vecPosition1 + (pev->movedir * (fabs (pev->movedir.x * (pev->size.x-2)) + fabs (pev->movedir.y * (pev->size.y-2)) + fabs (pev->movedir.z * (pev->size.z-2)) - m_flLip));
	ASSERTSZ(m_vecPosition1 != m_vecPosition2, "door start/end positions are equal");

	if (FBitSet (pev->spawnflags, SF_DOOR_START_OPEN))
		{	// swap pos1 and pos2, put door at pos2
		UTIL_SetOrigin(pev, m_vecPosition2);
		m_vecPosition2 = m_vecPosition1;
		m_vecPosition1 = pev->origin;
		}
	SetTouch (NULL);

	Precache();
	}

void CMomentaryDoor::Precache (void)
	{
	char precacheBuf[32];

	// set the door's "in-motion" sound
	switch (m_bMoveSnd)
		{
		case	0:
		default:
			pev->noiseMoving = ALLOC_STRING("common/null.wav");
			break;

		case	1:
		case	2:
		case	3:
		case	4:
		case	5:
		case	6:
		case	7:
		case	8:
		case	11:
		case	12:
		case	13:
			sprintf (precacheBuf, "doors/doormove%i.wav", m_bMoveSnd);
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;

		case	9:
			sprintf (precacheBuf, "doors/doormove9%c.wav", 'a' + RANDOM_LONG (0, 2));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;

		case	10:
			sprintf (precacheBuf, "doors/doormove10%c.wav", 'a' + RANDOM_LONG (0, 1));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseMoving = ALLOC_STRING(precacheBuf);
			break;
		}

	// set the door's 'reached destination' stop sound
	switch (m_bStopSnd)
		{
		case	0:
		default:
			pev->noiseArrived = ALLOC_STRING("common/null.wav");
			break;

		case	1:
		case	2:
		case	3:
		case	4:
		case	5:
		case	6:
		case	7:
		case	8:
		case	9:
		case	11:
		case	12:
		//case	13:
			sprintf (precacheBuf, "doors/doorstop%i.wav", m_bStopSnd);
			PRECACHE_SOUND (precacheBuf);
			pev->noiseArrived = ALLOC_STRING(precacheBuf);
			break;

		case	10:
			sprintf (precacheBuf, "doors/doorstop10%c.wav", 'a' + RANDOM_LONG (0, 1));
			PRECACHE_SOUND (precacheBuf);
			pev->noiseArrived = ALLOC_STRING(precacheBuf);
			break;
		}
	}

void CMomentaryDoor::KeyValue (KeyValueData *pkvd)
	{

	if (FStrEq(pkvd->szKeyName, "movesnd"))
		{
		m_bMoveSnd = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "stopsnd"))
		{
		m_bStopSnd = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else if (FStrEq(pkvd->szKeyName, "healthvalue"))
		{
		//		m_bHealthValue = atof(pkvd->szValue);
		pkvd->fHandled = TRUE;
		}
	else
		CBaseToggle::KeyValue (pkvd);
	}

void CMomentaryDoor::Use (CBaseEntity *pActivator, CBaseEntity *pCaller, USE_TYPE useType, float value)
	{
	if (useType != USE_SET)		// Momentary buttons will pass down a float in here
		return;

	if (value > 1.0)
		value = 1.0;
	if (value < -1.0)
		value = -1.0;

	Vector move = m_vecPosition1 + (abs (value) * (m_vecPosition2 - m_vecPosition1));
	Vector delta = move - pev->origin;
	float speed = delta.Length() * 10;

	if ((value != 0) && (abs (value) < 1.0))
		{
		//if ((value > 0) && ((pev->nextthink < pev->ltime) || (pev->nextthink == 0)))	// Не работает

		// oldSoundValue, равное нулю, означает, что движение начато только что
		// разные знаки означают, что изменено направление движения
		// в обоих случаях звук нужно перезапустить
		if ((oldSoundValue == 0.0f) || (abs (oldSoundValue) == 1.0f) || (oldSoundValue * value < 0))
			{
			STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
			EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving), 1, ATTN_MEDIUM);
			}

		LinearMove (move, speed);
		}
	else if ((oldSoundValue != 0) && (abs (oldSoundValue) < 1.0))	// Звук остановки невозможен в начале движения
		{
		STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
		EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseArrived), 1, ATTN_MEDIUM);
		}

	oldSoundValue = value;			// Обновление значения
	}

void CMomentaryDoor::MomentaryMoveDone (void)
	{
	STOP_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseMoving));
	EMIT_SOUND(ENT(pev), CHAN_STATIC, (char*)STRING(pev->noiseArrived), 1, ATTN_MEDIUM);
	}
