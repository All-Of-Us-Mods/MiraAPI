using System.Collections.Generic;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace MiraAPI;

public class MiraPluginInfo
{
    public string PluginId { get; set; }
    public IMiraConfig Config { get; set; }
    
    public readonly List<IModdedOptionGroup> OptionGroups = [];
    public readonly List<IModdedOption> Options = [];
    
    public readonly Dictionary<ushort, ICustomRole> CustomRoles = [];
    public readonly Dictionary<int, CustomGameMode> GameModes = [];
    
    public MiraPluginInfo(string id, IMiraConfig config)
    {
        PluginId = id;
        Config = config;
    }

}