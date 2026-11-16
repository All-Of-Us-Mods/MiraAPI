using HarmonyLib;

namespace MiraAPI.Patches.Hud;

/// <summary>
/// Preloads the Mira wiki/role guide panels when a game starts so opening the guide
/// in-game does not cause a noticeable hitch.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class MatchInfoGuidePreloadPatch
{
    public static void Postfix()
    {
        if (!MatchInfoGuide.Instance)
        {
            return;
        }

        Warning("Preloading Match Info Guide...");
        RoleGuidePatches.TriggerPreload(MatchInfoGuide.Instance);
    }
}
