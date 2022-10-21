# Xash3d Engine adaptation
### for mods **ESHQ** v 12.7 (and older) and **ES: Randomaze** v 3.7 (and older)

#

:warning: ***This engine has been replaced with [Xash3D FWGS Engine adaptation](https://github.com/adslbarxatov/Xash3d-FWGS-for-ESHQ).
This repository is no longer updated***

#

This engine modification created especially for [ESHQ mod](https://moddb.com/mods/eshq) for Half-Life part 1.

Modification based on one of old (from 2019) versions of [Xash3d engine (v0.99, rev 4529)](https://github.com/FWGS/xash3d) and may contain some bugs.

#

## Main changes:

1. We have fix some known bugs (like non-rotating `func_rotating`; crashes at killed scientists sentences, etc) for which we had enough mind and time.

2. We have added the `trigger_sound` entity that replaces the `env_sound`. It works as well as `trigger_multiple` (with hardcoded “wait” value – 1 s). We cannot understand why there were no brush entities to set sound effect. Spherical `env_sound` has very weird and unpredictable behavior, and it is difficult to apply it in cases like long narrow building entrances (f. e., partially opened gates). This situation really needs two brushes – before and after the gate – to trigger sound effects.

3. We have returned green blood to some monsters. This feature was implemented but wasn’t enabled in client library. Also it has been added to `env_blood` and `monster_generic` entities (with an ability to set it up).

4. Also we have returned human-like gibs and red blood to zombie. We think that zombie is more like a scientist than a bullsquid.

5. We have removed the `cycler_sprite` entity from our maps and added “Non-solid” flag to the `cycler` entity. Also our `cycler` now have the “Material” field (crowbar sound depends on it) and two fields that defines collision box endpoints (like the “Color” setup). `cycler` and `env_sprite` entities can also accept “body”, “skin” and “sequence” fields. Also `cycler` now can trigger its target.

6. Our doors (momentary, rotating and simple) have different fields for “Just opened” and “Just closed” sounds. We’re planning to split “Opening” sound to “Opening” and “Closing”. But it’s not necessary for now. Also we’ve fixed some bugs (basically, around the “Starts open” flag) and expanded the list of sounds (not only replaced exist ones). Finally, our doors will not play “locked” sounds when they’re opened.

7. Our turrets and apaches can trigger something on their deathes.

8. Breakables can spawn crowbars (why it was not so?) and gauss gun clips.

9. Our `ambient_generic` (and all entities that can sound) has more accurate sound radius (minimal as default).

10. We’ve added the `item_key` entity in addition to `item_security` to simplify doors triggering. It can look like a card or like a bunch of keys. 

11. We’ve added the `game_player_set_health` entity that sets absolute value of health and armor. It is applicable when you need to create an effect of immediate but controllable damage.

12. Our `func_illusionary` triggers its textures when get call (as well as `func_wall` or `func_button`).

13. Our grunts, barneys, scientists and zombies got “burned” state. Now it is possible to add burned corpses to the map. Also zombie got “dead” animation.

14. Our gman has “Killable” flag and two skins.

15. Our `monster_rat` can run and can be smashed (as well as `monster_cockroach`, but with red blood, of course).

16. Our .357 and crossbow got correct reload sounds.

17. We really want to create fog entity. We think that `func_water` without oxygen loss and swimming-like movement (and some other sound effect) can be used for it.

18. We’ve added “Don’t reset view angle / speed” flag to the  `trigger_teleport` entity. It’s useful in case of [map space expanding](http://moddb.com/mods/eshq/news/engine-specifications-for-teleports).

19. Our `weapon_fastswitch` mode is now really fast (as it is in HL2). You just need to press slot button again to select the next weapon.

20. Our `game_end` entity now works correctly (ends the game), and `player_loadsaved` can “kill” player (so you don’t need the `trigger_hurt` or some other “freezing” method).

21. Wood, glass and snow textures got their own sounds for player steps.

22. We have added the `trigger_ramdom` entity that can randomly trigger targets from a specified list with specified probabilities. No more lasers needed!

23. Our `item_security` and `item_antidote` are collectable now. Their counts can be used to trigger events on maps and activate extra abilities.

24. Our lasers can be turn off correctly. The older version of an engine turned off a sprite, but didn’t turn off the damage field.

25. Added support for an achievement script. Our modification generates script with extended player’s abilities according to count of collected `item_antidote` items.

26. Added support of “origin” brush for breakables and pushables when they drop items on break. Now item will be dropped at the center of the “origin” brush or at the center of entity if the brush is not presented.

27. Range of sounds for pushables has been expanded and now they depend of materials. Sound script for pushables has been improved (better behavior fitting).

28. The `scripted_sentence` entity now can play single sound (it must be prefixed with “!!”). Also you can add text message from `titles.txt` in addition to the sound sentence.

29. Some speed improvements applied to shotgun, mp5, hornetgun, 357 and other.

30. Fixed mouse wheel’s behavior and some inconveniences in the keyboard settings interface.

31. Fixed “gag” flag’s behavior: now all scientists with “gag” will be silent.

32. “Credits” section added to main menu; map for it can be specified in the game configuration.

33. Added “Locked sequence” flag for all monsters. When set, engine will loop the sequence that is specified in monster’s settings before its first damage, death or before the call from the `scripted_sequence` entity. This feature works without additional `scripted_sequence` entities.

34. Behavior of sounds of momentary doors reviewed: sounds of moving and stop work’s properly now.

35. Fixed bug with stuck weapons that can be dropped by dead human grunts.

36. “Use only” flag for doors will now work as a lock. Triggering these doors by their names will unlock them without opening. If no name specified, the door will be initially unlocked.

37. Added replacements for entities from *HL: blue shift* and *Afraid of monsters*.

38. Barnacles can shoot nothing on death.

39. Fixed too long time stuck on jumping from the water.

40. Switching between walk and run modes now works as well as in HL2 (run requires “/” key holding by default). “Always run” flag has been added to Advanced controls menu (disables this behavior).

41. Fixed mapping for additive textures: now they are able to be transparent and semitransparent conveyors.

42. Breakables now have sounds that depend on their sizes.

43. HUD now can display extra abilities (superflashlight, invisibility for enemies, damageproof).

44. Pushables are now react on explosions, shooting and hitting by a crowbar.

45. Implemented the “meat mode”: corpses can now be destroyed by bullets and crowbar in ome hit. Requires line `meat_mode "1"` in file `config.cfg`.



&nbsp;



## Known bugs

Unfortunately, we cannot fix some bugs yet:
- `func_pushable` bug: incorrect collision box.
- `func_door_rotating` bug: incorrect collision if door has “Ox” and/or “Oy” flags.

But we believe that it is not serious problem for now.

&nbsp;



## Other notes

- This assembly completely adapted for building with Visual studio 19.0 and newer (some fixes for type declarations and headers syntax added).
- This assembly is enough to launch Half-Life (WON) and some compatible mods.
- This assembly is a fork of original Xash3D engine with the same license.

&nbsp;



## [Development policy and EULA](https://adslbarxatov.github.io/ADP)

This Policy (ADP), its positions, conclusion, EULA and application methods
describes general rules that we follow in all of our development processes, released applications and implemented ideas.
***It must be acquainted by participants and users before using any of laboratory’s products.
By downloading them, you agree and accept this Policy!***
