using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(MapBehaviour))]
public class MapBehaviourPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap))]
    public static bool ShowSabotagePatch(MapBehaviour __instance)
    {
        var shouldShow = CustomGameModeManager.ActiveMode?.ShouldShowSabotageMap(__instance);
        if (shouldShow.HasValue && !shouldShow.Value)
        {
            __instance.ShowNormalMap();
            return false;
        }

        return true;
    }
}