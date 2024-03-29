## MapOptions
###### Version: 2.1.7
This is a mod for Rain World v1.9.

### Description
Adds options to configure the map:  
- `(Map Zoom)` The zoom can be adjusted (50%-150%).
- `(Aerial Map)` When disabled, the default map shader is used in Chimney Canopy and Sky Islands.
- `(Clear Expedition Maps)` When enabled, clears the map progress for each new expedition run. Warning: Map progress is saved even without completing a full cycle.
- `(Creature Symbols)` These symbols display what creature types are present in each room.
- `(Discover Multiplier)` Can be used to decrease or increase the map discover range around slugcat. Warning: This deletes your map progress first. The game tries to recover your map progress based on visited rooms.
- `(Item Tracker)` Tracked key items are shown on the map even when the option 'Slug Senses' is disabled. The option 'Key item tracking' needs to be enabled in Rain World Remix.
- `(Layer Focus)` Only the active layer is displayed on the map.
- `(Shadow Sprites)` Draws shadows for creature and slugcat symbols.
- `(Skip Fade In/Out)` Pressing the map button shows the map with no delay.
- `(Slugcat Symbols)` Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op Mod is enabled, draws a sprite for each player.
- `(Uncover Region)` Once loaded into the game the whole region map gets uncovered. Warning: Map progress is saved even without completing a full cycle.
- `(Uncover Room)` When the player enters a room the whole room gets uncovered instead of just the area around slugcat.
- `(Reveal Speed Multiplier)` For a given value X the map is revealed X-times as fast. If the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.

### Installation
0. Update Rain World to version 1.9 if needed.
1. Download the file  `MapOptions.zip` from [Releases](https://github.com/SchuhBaum/MapOptions/releases/tag/v2.1.7).
2. Extract its content in the folder `[Steam]\SteamApps\common\Rain World\RainWorld_Data\StreamingAssets\mods`.
3. Start the game as normal. In the main menu select `Remix` and enable the mod. 

### Bug reports
Please post bugs on the Rain World Discord server in the channel #modding-support:  
https://discord.gg/rainworld

### Contact
If you have feedback, you can message me on Discord `@schuhbaum` or write an email to SchuhBaum71@gmail.com.  

### License  
There are two licenses available - MIT and Unlicense. You can choose which one you want to use.

### Changelog
#### (Rain World v1.9)
v2.1.7:
- Fixed a bug that could lag the game when you would try to open the map while an in-game text message is displayed.
- (creature symbols) Fixed a bug where slugcat npc symbols would have the same color as the player.
- (creature symbols) Fixed a bug where a NullReference exeption was thrown when the room of the creature could not be found.
- (creature symbols) Removed that symbols are shown in shelters. Vanilla already shows symbols for items and creatures in shelters.
- Changed the hook initialization logic. This should reduce the log spam from IL hooks. Instead of doing it every cycle while in-game they are initialized when starting the game or when changing the options.
- (discover multiplier) Added a slider to change the map discover radius around slugcat.
- (uncover room) Potentially fixed an issue where adjacent non-overlapping rooms were partly getting uncovered as well.
- (clear expedition maps) Added this options (disabled by default). Clears the map progress for each new expedition run.
- Fixed a vanilla bug, where the reveal routine can completely stop. This happened when too many pixels needed to be revealed.

v2.1.0:
- Added support for Rain World 1.9.
- Removed AutoUpdate.
- (uncover region) Should work again.
- Activated map for sleep screen again. There were some visual issues in the past (mostly for the fast travel screen). I hadn't any so far.
- Activated map for death screen again.
- (aerial map) Added this option (enabled by default). When disabled, the default map shader is used in Chimney Canopy and Sky Islands.
- Restructured code.
- (creature symbols) Skipping rooms that are disabled for the map. Otherwise symbols for spawned creatures in these rooms can overlap with enabled rooms.
- Enabled this mod in the Safari mode.
- (shadow sprites) Added this option (disabled by default). When enabled, draws shadows for creature and slugcat symbols.
- (item tracker) Added this option (enabled by default). Tracked key items are shown on the map even when the option 'Slug Senses' is disabled. The option 'Key item tracking' needs to be enabled in Rain World Remix.
- Restructured code.
- (layer focus) Fixed a bug where this option would not activate correctly.
- Added pdb file for debugging.

#### (Rain World v1.5)
v0.59:
- Fixed two bugs where variables were not deleted properly.
- Restructered code.
- Uncover room option should be more reliable now.
- Switched to BepInEx plugin.
- Fixed a bug where creature symbols were not cleared.
- Restructured code.
- Fixed a bug where JollyCoop became a mandatory dependency.
- Fixed a bug where the dependency checks would fail when using the modloader Realm.
- (uncover room and uncover region options) Slightly increased the (reveal) area of rooms. Otherwise, some connections to other rooms might not get immediately uncovered.

v0.50:
- Restructured code. Logging more stuff.
- Creature symbols of swallowed creatures should not be visible anymore. Context: The position of swallowed creatures doesn't get updated.
- Fixed some bugs with accessing information.
- StandardGroundCreature symbols are not shown anymore. Should only affect custom creatures without custom symbols.
- Creatures in offscreen dens are not shown anymore.
- Updated option descriptions based on feedback.
- Added an option that uncovers the whole room when entered. Enabled by default.
- Simplified implementation. This should fix some bugs.
- Various changes to support multiple maps / cameras.
- Added an option to increase the reveal speed. Default value is one. The option instant reveal is removed or rather included in this one.

v0.40:
- Added support for AutoUpdate.
- Added slider to customize the size of creature and slugcat sprites.
- Hides creature sprites when they are in a den.
- Fixed a bug where slugcat sprites were not deleted properly.

v0.30:
- Added the option to display a slugcat sprite at the players position instead of a red circle (enabled by default). When JollyCoop Mod is used, you can see a slugcat sprite for each player.

v0.20:
- Added the option to skip the fade in/out animation when opening/closing the map.