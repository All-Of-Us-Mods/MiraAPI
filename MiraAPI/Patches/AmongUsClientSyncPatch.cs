using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
public static class AmongUsClientSyncPatch
{
    public static void Postfix(ClientData clientData)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }
        
        if (clientData.Id == AmongUsClient.Instance.HostId)
        {
            return;
        }

        ModdedOptionsManager.SyncAllOptions(clientData.Id);
        CustomRoleManager.SyncRoleSettings(clientData.Id);
    }
}