using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI;

[BepInAutoPlugin("mira.api", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraApiPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    private static MiraPluginManager PluginManager { get; set; }

    public static Color MiraColor = new Color32(238, 154, 112, 255);

    public override void Load()
    {
        Harmony.PatchAll();

        ReactorCredits.Register("Mira API", Version, true, ReactorCredits.AlwaysShow);

        PluginManager = new MiraPluginManager();
        PluginManager.Initialize();
    }
}
