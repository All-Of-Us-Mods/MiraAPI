[![](https://dcbadge.limes.pink/api/server/all-of-us-launchpad-794950428756410429)](https://discord.gg/all-of-us-launchpad-794950428756410429)

# Mira API

A thorough, but simple, Among Us modding API that covers:
- Roles
- Options
- Gamemodes
- Assets
- HUD Elements
- Compatibility
  
Mira API strives to be simple and easy to use, while also using as many base game elements as possible. The result is a less intrusive, better modding API that covers general use cases.

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

## Buttons

Mira API provides a simple interface for adding ability buttons to the game. There is only 2 steps:
1. Create a class that inherits from the `CustomActionButton` class and implement the properties and methods.
2. Add the `[RegisterCustomButton]` attribute to the class.

The button API is simple, but provides a lot of flexibility. There are various methods you can override to customize the behaviour of your button. See [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI/Hud/CustomActionButton.cs) for a full list of methods you can override.

An example button can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExampleButton.cs).

## Assets

Mira API provides a simple, but expandable asset system. The core of the system is the `LoadableAsset<T>` class. This is a generic abstract class that provides a pattern for loading assets. 

Mira API comes with two asset loaders:
1. `LoadableBundleAsset<T>`: This is used for loading assets from AssetBundles.
2. `LoadableResourceAsset`: This is used for loading **only sprites** from the Embedded Resources within a mod.

The code below shows how to use these asset loaders:
```csharp
// Load a sprite from an AssetBundle
AssetBundle bundle = AssetBundleManager.Load("MyBundle"); // AssetBundleManager is a utility provided by Reactor
LoadableAsset<Sprite> mySpriteAsset = new LoadableBundleAsset<Sprite>("MySprite", bundle);
Sprite sprite = mySpriteAsset.LoadAsset();

// Load a sprite from an Embedded Resource
// Make sure to set the Build Action of your image to Embedded Resource!
LoadableAsset<Sprite> buttonAsset = new LoadableResourceAsset("ExampleMod.Resources.MyButton.png");
Sprite button = buttonSpriteAsset.LoadAsset();
```

You can create your own asset loaders by inheriting from `LoadableAsset<T>` and implementing the `LoadAsset` method.

# Disclaimer

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
