using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Patches;
using System;
using System.Text;
using UnityEngine;

namespace MiraAPI;

[BepInAutoPlugin("mira.api", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraAPIPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MiraAPIPlugin Instance { get; private set; }
    public static MiraPluginManager PluginManager { get; private set; }
    public static Color MiraColor = new Color32(238, 154, 112, 255);

    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();

        PluginManager = new MiraPluginManager();
        PluginManager.Initialize();

        ReactorVersionShower.TextUpdated += (text) =>
        {
            text.text = new StringBuilder($"{MiraColor.ToTextColor()}Mira API</color> ")
            .Append(GetShortHashVersion(Version))
                        .Append("\n")
            .Append(text.text)
            .ToString();
        };
    }

    private static string GetShortHashVersion(string version)
    {
        var index = version.IndexOf("+", StringComparison.Ordinal);

        return index < 0 ? version : version[..(index + 3)];
    }
}
