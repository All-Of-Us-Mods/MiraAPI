using System.Collections.Generic;
using System.Collections.ObjectModel;
using BepInEx;
using BepInEx.Configuration;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;

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
    /// Get a read only collection of this plugin's Option Groups.
    /// </summary>
    /// <returns>Readonly collection of option groups.</returns>
    public IReadOnlyCollection<AbstractOptionGroup> GetOptionGroups()
    {
        return OptionGroups.AsReadOnly();
    }

    /// <summary>
    /// Gets a read only collection of this plugin's options.
    /// </summary>
    /// <returns>Read only collection of options.</returns>
    public IReadOnlyCollection<IModdedOption> GetOptions()
    {
        return Options.AsReadOnly();
    }

    /// <summary>
    /// Gets a read only dictionary of Role IDs and the RoleBehaviour object they are associated with.
    /// </summary>
    /// <returns>Read only dictionary of IDs and Roles.</returns>
    public ReadOnlyDictionary<ushort, RoleBehaviour> GetRoles()
    {
        return new ReadOnlyDictionary<ushort, RoleBehaviour>(CustomRoles);
    }

    /// <summary>
    /// Gets a read only collection of this plugin's custom buttons.
    /// </summary>
    /// <returns>Read only collection of buttons.</returns>
    public IReadOnlyCollection<CustomActionButton> GetButtons()
    {
        return Buttons.AsReadOnly();
    }

    /// <summary>
    /// Gets a read only collection of this plugin's custom buttons.
    /// </summary>
    /// <returns>Read only collection of buttons.</returns>
    public IReadOnlyCollection<Cosmetics.AbstractCosmeticsGroup> GetCosmetics()
    {
        return CosmeticGroups.AsReadOnly();
    }

    internal List<AbstractOptionGroup> OptionGroups { get; } = [];

    internal List<IModdedOption> Options { get; } = [];

    internal List<Cosmetics.AbstractCosmeticsGroup> CosmeticGroups { get; } = [];

    internal Dictionary<ushort, RoleBehaviour> CustomRoles { get; } = [];

    internal Dictionary<int, CustomGameMode> GameModes { get; } = [];

    internal List<CustomActionButton> Buttons { get; } = [];

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
