using System.Collections.Generic;
using System.Collections.ObjectModel;
using BepInEx;
using BepInEx.Configuration;
using MiraAPI.CustomChats;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers;
using MiraAPI.Presets;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace MiraAPI.PluginLoading;

/// <summary>
/// Represents a Mira plugin.
/// </summary>
public class MiraPluginInfo
{
    internal MiraPluginInfo(IMiraPlugin miraPlugin, PluginInfo info)
    {
        MiraPlugin = miraPlugin;
        PluginConfig = miraPlugin.GetConfigFile();
        PluginInfo = info;
        PluginId = info.Metadata.GUID;
    }

    /// <summary>
    /// Gets a read only collection of this plugin's modifiers. This is not safe for modifiers with constructors.
    /// </summary>
    public IReadOnlyCollection<BaseModifier> Modifiers { get; private set; } = null!;

    /// <summary>
    /// Gets a read only collection of this plugin's Option Groups.
    /// </summary>
    public IReadOnlyCollection<AbstractOptionGroup> OptionGroups { get; private set; } = null!;

    /// <summary>
    /// Gets a read only collection of this plugin's options.
    /// </summary>
    public IReadOnlyCollection<IModdedOption> Options { get; private set; } = null!;

    /// <summary>
    /// Gets a read only collection of this plugin's custom chats.
    /// </summary>
    public IReadOnlyCollection<AbstractCustomChat> Chats { get; private set; } = null!;

    /// <summary>
    /// Gets a read only dictionary of Role IDs and the RoleBehaviour object they are associated with.
    /// </summary>
    public ReadOnlyDictionary<ushort, RoleBehaviour> Roles { get; private set; } = null!;

    /// <summary>
    /// Gets a read only collection of this plugin's custom buttons.
    /// </summary>
    public IReadOnlyCollection<CustomActionButton> Buttons { get; private set; } = null!;

    /// <summary>
    /// Gets a read only dictionary of this plugin's custom presets.
    /// </summary>
    public IReadOnlyCollection<OptionPreset> Presets { get; internal set; } = null!;

    internal void SavePublicCollections()
    {
        Presets = [..InternalPresets];
        Modifiers = [..InternalModifiers];
        OptionGroups = [..InternalOptionGroups];
        Options = [..InternalOptions];
        Roles = new ReadOnlyDictionary<ushort, RoleBehaviour>(InternalRoles);
        Buttons = [..InternalButtons];
        Chats = [..InternalChats];
    }

    internal List<OptionPreset> InternalPresets { get; } = [];

    internal List<AbstractOptionGroup> InternalOptionGroups { get; } = [];

    internal List<IModdedOption> InternalOptions { get; } = [];

    internal List<BaseModifier> InternalModifiers { get; } = [];

    internal Dictionary<ushort, RoleBehaviour> InternalRoles { get; } = [];

    internal Dictionary<int, CustomGameMode> InternalGameModes { get; } = [];

    internal List<CustomActionButton> InternalButtons { get; } = [];

    internal List<AbstractCustomChat> InternalChats { get; } = [];

    /// <summary>
    /// Gets the plugin's ID, as defined in the plugin's BepInEx metadata.
    /// </summary>
    public string PluginId { get; }

    /// <summary>
    /// Gets the plugin's instance as an <see cref="IMiraPlugin"/>.
    /// </summary>
    public IMiraPlugin MiraPlugin { get; }

    /// <summary>
    /// Gets the plugin's BepInEx metadata.
    /// </summary>
    public PluginInfo PluginInfo { get; }

    /// <summary>
    /// Gets the plugin's configuration file.
    /// </summary>
    public ConfigFile PluginConfig { get; }
}
