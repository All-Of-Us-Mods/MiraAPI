using HarmonyLib;
using MiraAPI.Hud;

namespace MiraAPI.Patches;

/// <summary>
/// Harmony patches for the MeetingHud class.
/// </summary>
[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudPatches
{
    /// <summary>
    /// Resets the cooldown and effect of all custom buttons when the meeting starts.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void StartPostfix()
    {
        foreach (var customActionButton in CustomButtonManager.CustomButtons)
        {
            customActionButton.ResetCooldownAndOrEffect();
        }
    }

    /// <summary>
    /// Resets the cooldown and effect of all custom buttons when the voting is complete.
    /// </summary>
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
