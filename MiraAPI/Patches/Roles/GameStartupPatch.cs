using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(MainMenuManager))]
public static class GameStartupPatch
{
    private static bool _runOnce;

    /// <summary>
    /// This is used for registering roles when the game opens, might be a janky solution, but it works
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    public static void StartPostfix()
    {
        if (_runOnce)
        {
            return;
        }

        _runOnce = true;
        CustomRoleManager.RegisterInRoleManager();
    }
}