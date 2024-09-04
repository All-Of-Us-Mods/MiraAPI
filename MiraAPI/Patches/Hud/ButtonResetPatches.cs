using HarmonyLib;
using MiraAPI.Hud;

namespace MiraAPI.Patches.Hud;

/// <summary>
/// Reset button patches
/// </summary>
[HarmonyPatch]
public static class ButtonResetPatches
{
    /// <summary>
    /// Resets the cooldown and effect of all custom buttons when the meeting starts.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static void MeetingHudStartPostfix()
    {
        foreach (var customActionButton in CustomButtonManager.CustomButtons)
        {
            customActionButton.ResetCooldownAndOrEffect();
        }
    }

    /// <summary>
    /// Resets the cooldown and effect of all custom buttons after the exile screen is closed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    public static void ExileControllerReEnableGameplayPostfix()
    {
        foreach (var customActionButton in CustomButtonManager.CustomButtons)
        {
            customActionButton.ResetCooldownAndOrEffect();
        }
    }
}
