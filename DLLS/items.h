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
#ifndef ITEMS_H
#define ITEMS_H


class CItem: public CBaseEntity
	{
	public:
		void	Spawn (void);
		CBaseEntity* Respawn (void);
		void	EXPORT ItemTouch (CBaseEntity* pOther);
		void	EXPORT Materialize (void);
		virtual int MyTouch (CBasePlayer* pPlayer) { return -1; };

		// ESHQ: поддержка собираемых объектов
		virtual int Save (CSave& save);
		virtual int Restore (CRestore& restore);
		static TYPEDESCRIPTION m_SaveData[];
		void KeyValue (KeyValueData* pkvd);

		int minimumToTrigger;
	};

#endif // ITEMS_H
