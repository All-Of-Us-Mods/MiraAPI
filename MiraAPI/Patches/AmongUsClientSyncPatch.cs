using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;

namespace MiraAPI.Patches;

/// <summary>
/// Sync all settings to the player when they join the game.
/// </summary>
[HarmonyPatch]
internal static class AmongUsClientSyncPatch
{
    public static MethodBase TargetMethod()
    {
        return Helpers.GetStateMachineMoveNext<AmongUsClient>(nameof(AmongUsClient.CreatePlayer))!;
    }

    [HarmonyPostfix]
    public static void CreatePlayerPatch(Il2CppObjectBase __instance, bool __result)
    {
        if (__result)
            return;

        var wrapper = new StateMachineWrapper<AmongUsClient>(__instance);
        var clientData = wrapper.GetParameter<ClientData>("clientData");
        var joinEvent = new PlayerJoinEvent(clientData);
        joinEvent.InvokeEvent();

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (clientData.Id == AmongUsClient.Instance.HostId)
        {
            return;
        }

        ModdedOptionsManager.SyncAllOptions(clientData.Id);
        CustomRoleManager.SyncAllRoleSettings(clientData.Id);
        GameModeOption.RpcSyncGamemode(PlayerControl.LocalPlayer, GameModeOption.Value);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal static class AmongUsClientSyncPatch2
{
    [HarmonyPrefix]
    public static void PlayerLeftPatch(ClientData data, DisconnectReasons reason)
    {
        var leftEvent = new PlayerLeaveEvent(data, reason);
        leftEvent.InvokeEvent();
    }
}
