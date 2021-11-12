/*
menu_strings.cpp - custom menu strings
Copyright (C) 2011 Uncle Mike

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

#include "extdll.h"
#include "basemenu.h"
#include "utils.h"
#include "menu_strings.h"

char *MenuStrings[HINT_MAXSTRINGS] =
	{
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 10
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 20
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 30
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 40
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 50
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 60
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 70
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 80
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 90
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 100
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 110
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 120
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 130
	"",
#ifdef RU
	"Режим отображения",
#else
	"Display mode",
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 140
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 150
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 160
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 170
#ifdef RU
	"Развернуть мышь",
#else
	"Reverse mouse",
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 180
	"",
	"",
	"",
#ifdef RU
	"Чувствительность мыши",
#else
	"Mouse sensitivity",
#endif
	"",
	"",
	"",
#ifdef RU
	"Вернуться к игре",
	"Начать новую игру",
#else
	"Return to game",
	"Start a new game",
#endif
	"",	// 190
#ifdef RU
	"Загрузить ранее сохранённую игру",
	"Загрузить сохранение, сохранить текущую игру",
	"Изменить настройки игры, настроить управление",
#else
	"Load a previously saved game",
	"Load a saved game, save the current game",
	"Change game settings, configure controls",
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 200
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 210
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 220
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 230
	"",
	"",
	"",
#ifdef RU
	"Запуск Тренировки завершит\nвсе текущие игры. Продолжить?",
#else
	"Starting a Hazard course will exit\nany current game. Continue?",
#endif
	"",	// filled in UI_LoadCustomStrings
#ifdef RU
	"Уверены, что хотите покинуть игру?",
#else
	"Are you sure you want to quit?",
#endif
	"",
	"",
	"",
#ifdef RU	// 240
	"Запуск новой игры завершит\nвсе текущие игры. Продолжить?",
#else
	"Starting a new game will exit\nany current game. Continue?",
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 250
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 260
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 270
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 280
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 290
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 300
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 310
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 320
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 330
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 340
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 350
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 360
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 370
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 380
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 390
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	// ESHQ: поддержка титров
#ifdef RU
	"Посмотреть титры мода", // 400
#else
	"View mod's credits", // 400
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 410
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 420
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 430
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 440
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 450
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 460
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 470
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 480
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 490
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 500
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 510
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 520
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
#ifdef RU
	"Выбрать свою игру",	// 530
#else
	"Select a custom game",	// 530
#endif
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 540
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",
	"",	// 550
	};

void UI_InitAliasStrings (void)
	{
	char token[1024];

	// some strings needs to be initialized here
	sprintf (token, 
#ifdef RU
		"Завершить %s без\nсохранения текущей игры?", 
#else
		"Quit %s without\nsaving current game?",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_QUIT_ACTIVE] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Узнайте, как играть в %s",
#else
		"Learn how to play %s",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_HAZARD_COURSE] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Играть в %s на 'лёгких настройках'",
#else
		"Play %s on the 'easy' skill setting",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_SKILL_EASY] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Играть в %s на 'средних настройках'",
#else
		"Play %s on the 'medium' skill setting",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_SKILL_NORMAL] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Играть в %s на 'сложных настройках'",
#else
		"Play %s on the 'difficult' skill setting",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_SKILL_HARD] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Завершить %s",
#else
		"Quit playing %s",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_QUIT_BUTTON] = StringCopy (token);

	sprintf (token, 
#ifdef RU
		"Найти серверы %s, настроить игрока",
#else
		"Search for %s servers, configure character",
#endif
		gMenu.m_gameinfo.title);
	MenuStrings[HINT_MULTIPLAYER] = StringCopy (token);
	}

void UI_LoadCustomStrings (void)
	{
	char *afile = (char *)LOAD_FILE ("gfx/shell/strings.lst", NULL);
	char *pfile = afile;
	char token[1024];
	int string_num;

	UI_InitAliasStrings ();

	if (!afile)
		return;

	while ((pfile = COM_ParseFile (pfile, token)) != NULL)
		{
		if (isdigit (token[0]))
			{
			string_num = atoi (token);

			// check for bad stiringnum
			if (string_num < 0) continue;
			if (string_num > (HINT_MAXSTRINGS - 1))
				continue;
			}
		else continue; // invalid declaration ?

		// parse new string 
		pfile = COM_ParseFile (pfile, token);
		MenuStrings[string_num] = StringCopy (token); // replace default string with custom
		}

	FREE_FILE (afile);
	}
