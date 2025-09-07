using HarmonyLib;
using MiraAPI.Hud;
using Reactor.Utilities;

namespace MiraAPI.Patches.Hud;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
[HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
public static class ButtonResetPatches
{
    public static void Postfix()
    {
        foreach (var customActionButton in CustomButtonManager.CustomButtons)
        {
            try
            {
                customActionButton.ResetCooldownAndOrEffect();
            }
            catch (System.Exception ex)
            {
                Logger<MiraApiPlugin>.Error($"Error resetting cooldown and effect for button {customActionButton.GetType().Name}: {ex}");
            }

            if (customActionButton.UsesMode == ButtonUsesMode.PerRound)
            {
                customActionButton.SetUses(customActionButton.MaxUses);
            }
        }
    }
}
