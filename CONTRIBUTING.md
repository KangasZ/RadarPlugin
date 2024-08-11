# Contributing to `RadarPlugin`
Contributions are welcome, however, please read the following guidelines before contributing.

## Guidelines
- Ensure you do not rewrite the base of the plugin. The core systems should remain in place and if you have suggestions, make an issue describing what can be done to improve the system. I'm not here saying what exists is perfect, however it does exist in the current state due to lots of R&D and testing.
- Ensure your code follows the style of the rest of the plugin. This means naming, conventions, utility function locations, etc. At this point this project is a long-running project and has picked up better practices over time.
- Ensure your code will work with a prior version of the plugin. Any changes should have an automatic migration path. User's don't want to deal with that.

## What to contribute
- Bug fixes
- Additional constants - especially on release of new deep dungeon content
- Additional drawing features or fixes to existing features
- Additional modules to expand functionality of the deeper systems
- Fleshing out existing modules such as improving runtime performance or making a feature more robust

## System Overview
The plugin is rather basic but runs on a core system of modules to support the Radar. This assumes you know a bit about dalamud and c#. Is it the prettiest? Probably not, it's also a long running project that has evolved as I as a developer have improved.

### RadarPlugin/Configuration
- This holds the class that is saved to the disc when configuring the plugin.
- It also includes the functions that support various parts of the configuration feature

### RadarPlugin/RadarLogic
- This is the core of the plugin. It holds the logic for drawing the radar and the logic for the radar itself.
- This is split into three major parts, the driver, the 2d and 3d portions, and the modules.
#### RadarDriver.cs
- The driver is the main class that runs the radar. It is responsible for the main loop and the drawing of the radar.
- This is the hook to the UI tick of dalamud.
- This is where things get filtered out from the radar

#### Radar2D.cs and Radar3D.cs
- This is the 3d and 2d implementation of the radar
- It handles all drawing to the screen

#### RadarModules.cs
- This is the core of some of the additional functionality such as aggro radius, distance, and configuration information.
- The purpose of splitting the modules is to make the load on the system lower by caching things to only calculate once per tick
- Each module implements the `IModuleInterface` interface which is a simple interface that has a `StartTick` and `EndTick` function.
- All modules must implement these functions.
- All modules must be added to the `RadarModules` which will build and initialize the modules.
- The order of operations per tick is (`Dalamud.Framework.Update`) `RadarDriver.StartTick` -> (`Dalamud.UiBuilder.Draw`) `RadarDriver.Draw` -> (`Dalamud.UiBuilder.Draw`) `RadarModules.EndTick`
- Much of the Modules stuff is recent and were made due to being frustrated with how the system before was handling all these options. It is a bit of a test but I so far like the impact on the plugin.

### RadarPlugin/UI
- This is the UI, it only draws the user interface to configure the plugin. No radar code is written here
