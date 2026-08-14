using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraExampleMod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText { get; } = "Mira API\nExample Mod";

    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public override void Load()
    {
        ExampleEventHandlers.Initialize();
        Harmony.PatchAll();
    }
}
