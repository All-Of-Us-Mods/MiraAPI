using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.Update))]
internal static class TaskPanelPatch
{
    public static bool Prefix(TaskPanelBehaviour __instance)
    {
        if (__instance != Roles.HudManagerPatches.RoleTab && CustomGameModeManager.ActiveMode != null)
        {
            CustomGameModeManager.ActiveMode.UpdateTaskPanel(__instance);
            return false;
        }
        return true;
    }
}
