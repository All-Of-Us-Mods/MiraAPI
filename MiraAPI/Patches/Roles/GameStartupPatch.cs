using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(MainMenuManager))]
public static class GameStartupPatch
{
    private static bool _runOnce;

    /// <summary>
    /// This is used for registering roles when the game opens, might be a janky solution, but it works.
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
        foreach (var mode in CustomGameModeManager.IdToModeMap.Values)
        {
            if (mode.Icon != null)
            {
                mode.TmpIcon = TmpSpriteUtils.CreateSpriteAsset(mode.Icon.LoadAsset(), $"MiraApi.Gamemode.{mode.Name.Replace(" ", string.Empty)}.png", 1.35f);
            }
        }

        if (MiraPluginManager.Instance.QueuedRoleRegistrations.Count <= 0)
        {
            return;
        }

        foreach (var queue in MiraPluginManager.Instance.QueuedRoleRegistrations)
        {
            CustomRoleManager.RegisterRoleTypes(queue.Value, queue.Key);
        }

        MiraPluginManager.Instance.QueuedRoleRegistrations.Clear();
        CustomRoleManager.RegisterInRoleManager();
    }
}
