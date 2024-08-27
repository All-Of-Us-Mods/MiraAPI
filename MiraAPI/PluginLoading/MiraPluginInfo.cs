using BepInEx;
using BepInEx.Configuration;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using System.Collections.Generic;

namespace MiraAPI.PluginLoading;

public class MiraPluginInfo(IMiraPlugin miraPlugin, PluginInfo info)
{
    public string PluginId { get; set; } = info.Metadata.GUID;
    public IMiraPlugin MiraPlugin { get; set; } = miraPlugin;
    public PluginInfo PluginInfo { get; set; } = info;
    public ConfigFile PluginConfig { get; set; } = miraPlugin.GetConfigFile();

    public readonly List<AbstractOptionGroup> OptionGroups = [];
    public readonly List<IModdedOption> Options = [];

    public readonly Dictionary<ushort, RoleBehaviour> CustomRoles = [];
    internal readonly Dictionary<int, CustomGameMode> GameModes = [];
}