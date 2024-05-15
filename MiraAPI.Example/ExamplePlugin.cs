using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraAPIExample")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static ExamplePlugin Instance { get; private set; }
    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();

    }
}
