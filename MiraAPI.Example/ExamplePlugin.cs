using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Utilities.Assets;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraAPIExample")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin, IMiraConfig
{
    public Harmony Harmony { get; } = new(Id);
    public static ExamplePlugin Instance { get; private set; }
    public ModdedOptionTabSettings TabSettings => new ModdedOptionTabSettings()
    {
        Title = "MiraAPI Example Mod",
        TabIcon = MiraAssets.Empty
    };

    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();
    }
}