## MapOptions
###### Version: 0.55

### Description
This mod adds options to configure the map.  
- Creature Symbols: These symbols display what creature types are present in each room.
- Layer Focus: Only the active layer is displayed on the map.
- Skip Fade In/Out: Pressing the map button shows the map with no delay.
- Slugcat Symbols: Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op Mod is enabled, draws a sprite for each player.
- Uncover Region: Once loaded into the game the whole region map gets uncovered.
- Uncover Room: When the player enters a room the whole room gets uncovered instead of just the area around slugcat.
- Reveal Speed Multiplier: For a given value X the map is revealed X-times as fast. If the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.

### Installation
1. (ModLoader) `BepInEx` and `BOI` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.  
  **NOTE:** Rain World's BepInEx is a modified version. Don't download it from GitHub.  
2. (Dependency) The mod `ConfigMachine` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.
3. Download the file  `MapOptions.dll` from [Releases](https://github.com/SchuhBaum/MapOptions/releases) and place it in the folder `[Steam]\SteamApps\common\Rain World\Mods`.  
4. Start `[Steam]\SteamApps\common\Rain World\BOI\BlepOutIn.exe`.  
5. Click `Select path` and enter the game's path `[Steam]\SteamApps\common\Rain World`. Enable the mod `MapOptions.dll` and its dependencies. Then launch the game as normal. 

### Contact
If you have feedback, you can message me on Discord `@SchuhBaum#7246` or write an email to SchuhBaum71@gmail.com.  

### License  
There are two licenses available - MIT and Unlicense. You can choose which one you want to use.

### Changelog
v0.20:
- Added the option to skip the fade in/out animation when opening/closing the map.

v0.30:
- Added the option to display a slugcat sprite at the players position instead of a red circle (enabled by default). When JollyCoop Mod is used, you can see a slugcat sprite for each player.

v0.40:
- Added support for AutoUpdate.
- Added slider to customize the size of creature and slugcat sprites.
- Hides creature sprites when they are in a den.
- Fixed a bug where slugcat sprites were not deleted properly.

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

v0.55:
- Fixed two bugs where variables were not deleted properly.
- Restructered code.
- Uncover room option should be more reliable now.
- Switched to BepInEx plugin.
- Restructured code. Fixed a bug where creature symbols were not cleared.