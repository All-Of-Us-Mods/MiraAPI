using BepInEx;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using System.Collections.Generic;

namespace MiraAPI.PluginLoading;

public class MiraPluginInfo
{
    public string PluginId { get; set; }
    public IMiraPlugin Config { get; set; }
    public PluginInfo PluginInfo { get; set; }

    public readonly List<IModdedOptionGroup> OptionGroups = [];
    public readonly List<IModdedOption> Options = [];

    public readonly Dictionary<ushort, RoleBehaviour> CustomRoles = [];
    public readonly Dictionary<int, CustomGameMode> GameModes = [];

    public MiraPluginInfo(string id, IMiraPlugin config, PluginInfo info)
    {
        PluginId = id;
        Config = config;
        PluginInfo = info;
    }

}