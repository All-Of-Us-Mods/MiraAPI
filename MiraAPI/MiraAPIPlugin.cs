using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using System.Reflection;

namespace MiraAPI;

[BepInAutoPlugin("mira.api", "MiraAPI")]
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

        RegisterCustomRoleAttribute.Initialize();
        RegisterModdedOptionAttribute.Initialize();

        RegisterGameModeAttribute.Register(Assembly.GetCallingAssembly());
        RegisterButtonAttribute.Register(Assembly.GetExecutingAssembly());
    }
}
