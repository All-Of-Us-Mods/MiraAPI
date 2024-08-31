using BepInEx;
using BepInEx.Configuration;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using System.Collections.Generic;

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

    internal List<AbstractOptionGroup> OptionGroups { get; } = [];

    internal List<IModdedOption> Options { get; } = [];

    internal Dictionary<ushort, RoleBehaviour> CustomRoles { get; } = [];

    internal Dictionary<int, CustomGameMode> GameModes { get; } = [];

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
