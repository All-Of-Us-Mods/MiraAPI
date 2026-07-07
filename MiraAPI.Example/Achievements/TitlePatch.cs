using HarmonyLib;

namespace MiraAPI.Example.Achievements;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class TitlePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        __instance.gameObject.AddComponent<AchievementTitleComponent>();
    }
}
