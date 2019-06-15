# Xash3d for ESHQ / ES:FA v 7.4
### Xash3d engine v43/0.91 (rev 3737); adaptation for ESHQ / ES:FA mod v 7.4
#
This engine modification created specially for [ESHQ mod](http://www.moddb.com/mods/eshq/) for Half-Life part 1.

Modification based on old (from 2015) version of engine and may contain old bugs. But some new features
may be useful for developers with the same mods' plots.
#
## Main changes:

1. We have fix some known bugs (like non-rotating func_rotating; crashes at killed scientists sentences, etc) for which we had enough mind and time.

2. We have added entity trigger_sound that replaces env_sound. It works as well as trigger_multiple (with hardcoded 'wait' value - 1 s). We cannot understand, why there was no brush entities to set sound effect. Spherical env_sound has very weird and unpredictable behaviour, and it is difficult to apply it in cases like long narrow building entrances (f.e., partially opened gates). This situation really needs two brushes - before and after the gate - to trigger sound effects.

3. We have returned green blood to some monsters. This feature was implemented but not activated in client library. Also accepted by 'env_blood' and 'monster_generic'.

4. Also we have returned human-like gibs and red blood to zombie. We think that zombie is more like a scientist than a bullsquid.

5. We have removed from our maps the 'cycler_sprite' and added 'Non-solid' flag to 'cycler'. Also our 'cycler' now have 'Material' field (crowbar hit sound depends on it) and two fields that defines collision box endpoints (looks like 'Color' setup). 'cycler' and 'env_sprite' entities can also accept 'body', 'skin' and 'sequence' settings. 'cycler' now can trigger its target.

6. Our doors (momentary, rotating and simple) have different fields for 'Just opened' and 'Just closed' sounds. We are planning to split 'Opening' sound to 'Opening' and 'Closing'. But now it is not necessary. Also we have fixed some bugs (basically, around 'Starts open' flag) and expanded list of sounds (not only replaced exist ones). Finally, our doors are not play 'locked' sounds when opened.

7. Our turrets and apaches can trigger something on death.

8. Breakables can spawn crowbars (why it was not so?) and gauss gun clips.

9. Our ambient_generic (and all entities that can sound) has more accurate sound radius (minimal as default).

10. We have added entity 'item_key' in addition to 'item_security' (can look like a card or like a bunch of keys) to simplify doors triggering.

11. We have added entity 'game_player_set_health' that sets absolute value of health and armour. Applicable when you need to create effect of immediate but controllable damage.

12. Our 'func_illusionary' triggers its textures when get call (as well as 'func_wall' or 'func_button').

13. Our grunts, barneys, scientists and zombies got 'burned' state: we can add burned corpses to the map. Also zombie got 'dead' animation.

14. Our gman has 'Killable' flag and two skins.

15. Our monster_rat can run and can be smashed (as well as monster_cockroach, but with red blood, of course).

16. Our .357 and crossbow got correct reload sounds.

17. We really want to create fog entity. We think that 'func_water' without oxygen loss and swimming-like movement (and some other sound effect) can be used for it.

18. We have added 'Don't reset view angle/speed' flag to trigger_teleport entity. It is useful in case of [map space expanding](http://www.moddb.com/mods/eshq/news/engine-specifications-for-teleports).

19. Our weapon_fastswitch is really fast (as it is in HL2). Just press slot button again for next weapon selection.

20. Our game_end entity works correctly (ends the game), and player_loadsaved can 'kill' player (so you don't need trigger_hurt or some other 'frozing' method).

21. Wood, glass and snow textures got own sounds for player steps.

22. We have added a 'trigger_ramdom' entity that can randomly trigger targets from a specified list with specified probabilities. No more lasers needed!

23. Our 'item_security' and 'item_antidote' are collectable now. Their counts can be used to trigger events on maps and activate extra abilities.

24. Our lasers can be turn off correctly (old version of an engine turns off a sprite, but doesn't turn off a damage).

25. Added achievement script’s support. Now our modification generates script with extended player's abilities according to count of collected 'item_antidote' items.

26. Added support of 'origin' brush for breakables and pushables when they drop items on break. Now item will be dropped at the center of 'origin' brush or at the center of entity if brush not presented.

27. Range of pushables' sounds has been expanded and now they depend of materials of pushables. Sound script for pushables has been improved (better behavior corresponding).

28. 'scripted_sentence' entity now can play single sound (it must be prefixed with '!!'). Also you can add text message from titles.txt in addition to the sound sentence.

29. Some speed improvements applied to shotgun and mp5.

30. Fixed mouse wheel's behavior and keyboard settings interface's inconvenience.

#
## Known bugs

Unfortunately, we cannot fix some bugs yet:

1. 'momentary_rot_button' bug: infinite cycle for loopable door sounds.

2. 'func_pushable' bug: totally incorrect collision box.

3. 'func_door_rotating' bug: incorrect collision if door has 'Ox' and/or 'Oy' flags.



But we believe that it is not serious problem for now.
#
This assembly completely adapted for building with MS VS 10.0 and newer (some fixes for type declarations and headers syntax added).

This assembly is enough to launch Half-Life (WON) and some compatible mods.

This assembly is a fork of original Xash3D engine with the same license.
