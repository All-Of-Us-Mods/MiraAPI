[![](https://dcbadge.limes.pink/api/server/AEfHJGwggC)](https://discord.gg/AEfHJGwggC)

[English] | [简体中文](Translated/README_zh.md)

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

# Mira API

A thorough, but simple, Among Us modding API and utility library that covers:

- Roles
- Options
- Modifiers
- Buttons
- Custom Colors
- Events
- Voting
- Assets
- Keybinds
- Local Settings
- Custom Game Over Conditions
- Compatibility
- ~~Game Modes~~ (coming soon)

Mira API strives to be comprehensive, yet straightforward, while also using as many base game elements as possible.
The result is a less intrusive, better modding API that covers general use cases.

**Join the [Discord](https://discord.gg/FYYqJU2bvp) for support and to stay updated on the latest releases**

# Usage

To start using Mira API, you need to:

1. Add a reference to Mira API either through a [DLL](https://github.com/All-Of-Us-Mods/MiraAPI/releases), project reference, or [NuGet package](https://www.nuget.org/packages/AllOfUs.MiraAPI).
2. Add a BepInDependency on your plugin class like this: `[BepInDependency(MiraApiPlugin.Id)]`
3. Implement the `IMiraPlugin` interface on your plugin class.

Mira API also depends on [Reactor](https://github.com/NuclearPowered/Reactor) to function properly!
Remember to include it as a reference and `BepInDependency`!

For a full example, see [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExamplePlugin.cs).

## Recommended Project Structure

It is highly recommended to follow this project structure when using Mira API to keep your code clean and organized.
You can also view the Example Mod in this repository for some guidance.

```
MyMiraMod/
├── Buttons/
│   └── MyCoolButton.cs
├── Options/
│   ├── Roles/
│   │   └── CoolCustomRoleOptions.cs
│   └── MainOptionGroup.cs
├── Patches/
│   ├── Roles/
│   │   └── CoolCustomRole/
│   │       ├── PlayerControlPatches.cs
│   │       └── ExileControllerPatches.cs
│   └── General/
│       └── HudManagerPatches.cs
├── Resources/
│   ├── CoolButton.png
│   └── myAssets-win-x86.bundle
├── Roles/
│   └── CoolCustomRole.cs
├── MyMiraModPlugin.cs
└── MyModAssets.cs
```

# Documentation

Full documentation for every Mira API feature is available on the **[wiki](https://github.com/All-Of-Us-Mods/MiraAPI/wiki)**:

- [Get Started / IMiraPlugin](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Home)
- [Custom Roles](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Custom-Roles)
- [Options](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Options)
- [Modifiers](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Modifiers)
- [Custom Buttons](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Custom-Buttons)
- [Custom Colors](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Colors)
- [Events](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Events)
- [Meetings and Voting](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Meetings-and-Voting)
- [Assets](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Assets)
- [Keybinds](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Keybinds)
- [Local Settings](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Local-Settings)
- [Game Modes](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Game-Modes)
- [Custom Game Over](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Game-Over)
- [Utilities](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Utilities)
