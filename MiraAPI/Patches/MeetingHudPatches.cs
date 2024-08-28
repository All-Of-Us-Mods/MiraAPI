using HarmonyLib;
using MiraAPI.Hud;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    public static void VotingCompletePostfix()
    {
        foreach (var customActionButton in CustomButtonManager.CustomButtons)
        {
            customActionButton.ResetCooldownAndOrEffect();
        }
    }
}