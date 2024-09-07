[![](https://dcbadge.limes.pink/api/server/all-of-us-launchpad-794950428756410429)](https://discord.gg/all-of-us-launchpad-794950428756410429)

# Mira API

A thorough, but simple, Among Us modding API and utility library that covers:
- Roles
- Options
- Modifiers
- Buttons
- Custom Colors
- Assets
- Compatibility
- ~~Game Modes~~ (coming soon)

Mira API strives to be simple and easy to use, while also using as many base game elements as possible. The result is a less intrusive, better modding API that covers general use cases.

**Join the [Discord](https://discord.gg/all-of-us-launchpad-794950428756410429) for support and to stay updated on the latest releases**

# Usage

To start using Mira API, you need to:
1. Add a reference to Mira API either through a [DLL](https://github.com/All-Of-Us-Mods/MiraAPI/releases), project reference, or [NuGet package](https://www.nuget.org/packages/AllOfUs.MiraAPI).
2. Add a BepInDependency on your plugin class like this: `[BepInDependency(MiraApiPlugin.Id)]`
3. Implement the IMiraPlugin interface on your plugin class.

Mira API also depends on [Reactor](https://github.com/NuclearPowered/Reactor) in order to function properly! Do not forget to include it as a reference and `BepInDependency`!

For a full example, see [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExamplePlugin.cs).

## Recommended Project Structure
It is highly recommended to follow this project structure when using Mira API in order to keep your code clean and organized. You can also view the Example Mod in this repository for some guidance.
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

## Roles
Roles are very simple in Mira API. There are 3 things you need to do to create a custom role:
1. Create a class that inherits from a base game role (like `CrewmateRole`, `ImpostorRole`, etc) 
2. Implement the `ICustomRole` interface from Mira API.
3. Add the `[RegisterCustomRole]` attribute to the class.

**Disclaimer: Make sure your plugin class has the following attribute `[ReactorModFlags(ModFlags.RequireOnAllClients)]` or else your roles will not register correctly.**

Note: For step 1, if you are making neutral roles, choose either `CrewmateRole` or `ImpostorRole` as the base depending on if it can kill or not! 

Mira API handles everything else, from adding the proper options to the settings menu, to managing the role assignment at the start of the game. There are no extra steps on the developer's part.

See [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Roles/CustomRole.cs) for a code example.

## Modifiers
Mira API uses a different definition of 'modifiers' than other Among Us mods. For example, in Town Of Us, a modifier is an extra "ability" that is applied on top of the base role. However, in Mira API, modifiers are very flexible. A modifier is anything that "modifiers" a player's abilities or interactions.

Mira provides 3 classes for working with modifiers:
- `BaseModifier`: The base for every modifier. You MUST add and remove this modifier from a player manually!
- `TimedModifier`: A modifier that has a time limit. This modifier has to be added manually, but will automatically remove itself after the time limit
- `GameModifier`: This works like the typical TOU modifier, where it is automatically applied at the beginning of the game, then removed at the end.

Modifiers provide various overridable functions and properties for custom behaviour. They can also be used for "tagging" a player. You can check if a player has a modifier through the extension method `HasModifier` on a `PlayerControl` object.

To start using a modifier, pick one of the base classes above and create a class that inherits from it. Implement the properties and methods you would like, then add the `[RegisterModifier]` attribute to the class.

An example Game modifier can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Modifiers/GameModifierExample.cs).
An example Timer modifier can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Modifiers/ModifierTimerExample.cs).

## Options
Options are also very simple in Mira API. Mira API handles all the hard work behind the scenes, so developers only have to follow a few steps to create their custom options. The Options API is split up into Groups and Options. Every Option needs to be in a Group.

To create a group, you need to create a class that inherits from the `AbstractOptionGroup` abstract class. Groups contain 4 properties, `GroupName`, `GroupColor`, `GroupVisible`, and `AdvancedRole`. Only the `GroupName` is required.

Here is an example of a group class:
```csharp
public class MyOptionsGroup : AbstractOptionGroup
{
    public override string GroupName => "My Options"; // this is required
    
    [ModdedNumberOption("My Number Option", min: 0, max: 10)]
    public float MyNumberOption { get; set; } = 5f;
}
```

You can access any group class using the `OptionGroupSingleton` class like this:
```csharp
// MyOptionsGroup is a class that inherits from AbstractOptionGroup
var myGroup = OptionGroupSingleton<MyOptionsGroup>.Instance; // gets the instance of the group
Logger<MyPlugin>.Info(myGroup.MyNumberOption); // prints the value of the option to the console
```

Once you have an options group, there are two ways to make the actual options:
- Use an Option Attribute with a property.  
- Create a ModdedOption property.

### Option Attributes

This is an example of using an Option Attribute on a property:
```csharp
// The first parameter is always the name of the option. The rest are dependent on the type of option.
[ModdedNumberOption("Sussy level", min: 0, max: 10)]
public float SussyLevel { get; set; } = 4f; // You can set a default value here.
```

Here are the available Option Attributes and their signatures:
```csharp
ModdedEnumOption(string name, Type enumType, string[]? values = null, Type? roleType = null)
    
ModdedNumberOption(
    string name,
    float min,
    float max,
    float increment=1
    NumberSuffixes suffixType = NumberSuffixes.None,
    bool zeroInfinity = false,
    Type? roleType = null)

ModdedToggleOption(string name, Type? roleType = null)
```

### ModdedOption Properties

And this is an example of a ModdedOption property:
```csharp
public ModdedToggleOption YeezusAbility { get; } = new ModdedToggleOption("Yeezus Ability", false);
```

Here is a full list of ModdedOption classes you can use: 
- `ModdedEnumOption`
- `ModdedNumberOption`
- `ModdedToggleOption`

To see a full example of an options class, see [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Options/ExampleOptions.cs).

### Role Options

You can also specify a role type for an option or option group.

To set the role type for an entire group, set the `AdvancedRole` property on that group like this: 
```csharp
public class MyOptionsGroup : AbstractOptionGroup
{
    public override string GroupName => "My Options";
    public override Type AdvancedRole => typeof(MyRole); // this is the role that will have these options
    
    [ModdedNumberOption("Ability Uses", min: 0, max: 10)]
    public float AbilityUses { get; set; } = 5f;
}
```

To set the role type for individual options, specify the `roleType` parameter in the option like this:
```csharp
// this group doesnt specify a role, so it will show up in the global settings
public class MyOptionsGroup : AbstractOptionGroup
{
    public override string GroupName => "My Options";
    
    // this option will only show up in the settings for MyRole
    [ModdedNumberOption("Ability Uses", min: 0, max: 10, roleType: typeof(MyRole))]
    public float AbilityUses { get; set; } = 5f;
}
```

An example can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Options/Roles/CustomRoleSettings.cs).

## Custom Murders
Mira API provides it's own implementation for murders. Our implementation allows for more customization on kills, and helps bypass server checks.
You can use `PlayerControl.RpcCustomMurder` to perform a networked custom murder, or `PlayerControl.CustomMurder` to normally perform a custom murder.
For example: 
```cs
PlayerControl.LocalPlayer.RpcCustomMurder(Target, createDeadBody: false, teleportMurderer: false, playKillSound: false, resetKillTimer: false, showKillAnim: false);
```
This will kill a player without creating a dead body and without teleporting the murderer.

## Buttons

Mira API provides a simple interface for adding ability buttons to the game. There is only 2 steps:
1. Create a class that inherits from the `CustomActionButton` class and implement the properties and methods.
2. Add the `[RegisterCustomButton]` attribute to the class.

All other tasks and logic required to add the button to the game are handled by Mira API.

In case you need to access your `CustomActionButton` instance from another class, you can use the `CustomButtonSingleton` class like this:
```csharp
var myButton = CustomButtonSingleton<MyCoolButton>.Instance;
```

The button API is simple, but provides a lot of flexibility. There are various methods you can override to customize the behaviour of your button. See [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI/Hud/CustomActionButton.cs) for a full list of methods you can override.

An example button can be found [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/Buttons/ExampleButton.cs).

## Custom Colors

Mira provides a simple Custom Color API that allows you to add custom player colors to the game.

Creating custom colors isn't difficult, but there are some requirements for your colors to be registered by Mira.

1. Create a ***STATIC*** class to house all your `CustomColor` objects.
2. Inside this class, create a `CustomColor` property for each color you intend to add.
3. Add the `[RegisterCustomColors]` attribute to the class.

Here is an example of a custom color class:
```csharp
[RegisterCustomColors]
public static class MyCustomColors
{
    public static CustomColor Cerulean { get; } = new CustomColor("Cerulean", new Color(0.0f, 0.48f, 0.65f)); 

    public static CustomColor Rose { get; } = new CustomColor("Rose", new Color(0.98f, 0.26f, 0.62f));
    
    public static CustomColor Gold { get; } = new CustomColor("Gold", new Color(1.0f, 0.84f, 0.0f));
}
```

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

You can view an example file [here](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExampleAssets.cs).

You can create your own asset loaders by inheriting from `LoadableAsset<T>` and implementing the `LoadAsset` method.

# Disclaimer

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
