using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using System.Reflection;

namespace MiraAPI;

[BepInAutoPlugin("dev.xtracube.launchpad", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraAPIPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MiraAPIPlugin Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();

        RegisterGameModeAttribute.Register(Assembly.GetExecutingAssembly());
        RegisterCustomRoleAttribute.Register(Assembly.GetExecutingAssembly());
        RegisterButtonAttribute.Register(Assembly.GetExecutingAssembly());
    }
}
