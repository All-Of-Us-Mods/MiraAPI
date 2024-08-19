![https://discord.gg/all-of-us-launchpad-794950428756410429](https://img.shields.io/discord/794950428756410429?style=for-the-badge&logo=discord&logoColor=white)

# Mira API

A thorough, but simple, Among Us modding API that covers:
- Roles
- Options
- Gamemodes
- HUD Elements
- Compatibility
  
Mira API striv  es to be simple and easy to use, while also using as many base game elements as possible. The result is a less intrusive, better modding API that covers general use cases.

# Usage

To start using Mira API, you need to:
1. Add a reference to Mira API either through a DLL, project reference, or NuGet package.
2. Add a BepInDependency on your plugin class like this: `[BepInDependency(MiraApiPlugin.Id)]`
3. Implement the IMiraPlugin interface on your plugin class.

For a full example, see [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExamplePlugin.cs).

## Roles
Roles are very simple in Mira API. There are 3 things you need to do to create a custom role:
1. Create a class that inherits from a base game role (like `RoleBehaviour`, `CrewmateRole`, `ImpostorRole`, etc).
2. Implement the `ICustomRole` interface from Mira API.
3. Add the `[RegisterCustomRole]` attribute to the class.

See [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/CustomRole.cs) for a code example.

## Options
Options are also very simple in Mira API. Options are split up into Groups and Options. Every Option needs to be in a Group.

To create a group, you need to create a class that implements the `IModdedOptionGroup` interface. Groups contain two properties, the `GroupName` and `GroupColor`.

The easiest way to create an option within this group is to make a property within a class and assign one of the various Options Attributes listed below:
- `ModdedEnumOption`
- `ModdedNumberOption`
- `ModdedStringOption`
- `ModdedToggleOption`

They are used like this:
```csharp
// The first parameter is always the name of the option. The rest are dependent on the type of option.
[ModdedNumberOption("Sussy level", min: 0, max: 10)]
public float sussyLevel { get; set; } = 4f; // You can set a default value here.
```

To see a full example, see [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExampleOptions.cs).

### Role Options

Options can also be used within a Role class to show up in that Role's settings. To do this, simply add your option property to the Role class and specify the `roleType` parameter in the Option attribute.

An example can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/CustomRole2.cs).


## Disclaimer

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
