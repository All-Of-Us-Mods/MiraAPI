using System.Collections.Generic;
using System.Collections.ObjectModel;
using BepInEx;
using BepInEx.Configuration;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Presets;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace MiraAPI.PluginLoading;

/// <summary>
/// Represents an <see cref="IMiraPlugin"/>.
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
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="BaseModifier"/>s. This is not safe for <see cref="BaseModifier"/>s with constructors.
    /// </summary>
    public IReadOnlyCollection<BaseModifier> Modifiers { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="AbstractOptionGroup"/>s.
    /// </summary>
    public IReadOnlyCollection<AbstractOptionGroup> OptionGroups { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="IModdedOption"/>s.
    /// </summary>
    public IReadOnlyCollection<IModdedOption> Options { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of Role IDs and the <see cref="RoleBehaviour"/> object they are associated with.
    /// </summary>
    public ReadOnlyDictionary<ushort, RoleBehaviour> Roles { get; private set; } = null!;

    /// <summary>
    /// Gets a read only dictionary of Game Mode IDs and the AbstractGameMode object they are associated with.
    /// </summary>
    /// <returns>Read only dictionary of IDs and Game Modes.</returns>
    public ReadOnlyDictionary<uint, AbstractGameMode> GetModes()
    {
        return new ReadOnlyDictionary<uint, AbstractGameMode>(GameModes);
    }

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="CustomActionButton"/>.
    /// </summary>
    public IReadOnlyCollection<CustomActionButton> Buttons { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="TargetedMeetingButton"/>.
    /// </summary>
    public IReadOnlyCollection<TargetedMeetingButton> TargetedMeetingButtons { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="MeetingActionButton"/>.
    /// </summary>
    public IReadOnlyCollection<MeetingActionButton> MeetingButtons { get; private set; } = null!;

    /// <summary>
    /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of this <see cref="IMiraPlugin"/>'s <see cref="OptionPreset"/>s.
    /// </summary>
    public IReadOnlyCollection<OptionPreset> Presets { get; internal set; } = null!;

    internal void SavePublicCollections()
    {
        Presets = [.. InternalPresets];
        Modifiers = [.. InternalModifiers];
        OptionGroups = [.. InternalOptionGroups];
        Options = [.. InternalOptions];
        Roles = new ReadOnlyDictionary<ushort, RoleBehaviour>(InternalRoles);
        Buttons = [.. InternalButtons];
        MeetingButtons = [.. InternalMeetingButtons];
        TargetedMeetingButtons = [.. InternalTargetedMeetingButtons];
    }

    internal List<OptionPreset> InternalPresets { get; } = [];

    internal List<AbstractOptionGroup> InternalOptionGroups { get; } = [];

    internal List<IModdedOption> InternalOptions { get; } = [];

    internal List<BaseModifier> InternalModifiers { get; } = [];

    internal Dictionary<uint, AbstractGameMode> GameModes { get; } = [];
    internal Dictionary<ushort, RoleBehaviour> InternalRoles { get; } = [];

    internal List<CustomActionButton> InternalButtons { get; } = [];

    internal List<TargetedMeetingButton> InternalTargetedMeetingButtons { get; } = [];

    internal List<MeetingActionButton> InternalMeetingButtons { get; } = [];

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
    /// Gets the plugin's <see cref="ConfigFile"/>.
    /// </summary>
    public ConfigFile PluginConfig { get; }
}
