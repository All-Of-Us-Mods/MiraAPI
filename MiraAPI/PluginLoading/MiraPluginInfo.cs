using BepInEx;
using BepInEx.Configuration;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using System.Collections.Generic;

namespace MiraAPI.PluginLoading;

public class MiraPluginInfo(string id, IMiraPlugin miraPlugin, PluginInfo info)
{
    public string PluginId { get; } = id;
    public IMiraPlugin MiraPlugin { get; } = miraPlugin;
    public PluginInfo PluginInfo { get; } = info;
    public ConfigFile PluginConfig { get; } = miraPlugin.GetConfigFile();

    public readonly List<AbstractOptionGroup> OptionGroups = [];
    public readonly List<IModdedOption> Options = [];

    public readonly Dictionary<ushort, RoleBehaviour> CustomRoles = [];
    public readonly Dictionary<int, CustomGameMode> GameModes = [];
}