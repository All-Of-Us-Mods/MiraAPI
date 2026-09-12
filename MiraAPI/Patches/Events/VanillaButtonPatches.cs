using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

/// <summary>
/// Patches to invoke <see cref="ActionButton"/> related <see cref="MiraEvent"/>s.
/// </summary>
[HarmonyPatch]
public static class VanillaButtonPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    public static bool DoClickPrefix(AbilityButton __instance)
    {
        // Invoke the generic button click event.
        PlayerControl? playerTarget = null;
        Vent? ventTarget = null;
        var role = PlayerControl.LocalPlayer.Data.Role;
        if (role.Role is RoleTypes.Tracker)
        {
            playerTarget = role.Cast<TrackerRole>().currentTarget;
        }
        else if (role.Role is RoleTypes.GuardianAngel)
        {
            playerTarget = role.Cast<GuardianAngelRole>().currentTarget;
        }
        else if (role.Role is RoleTypes.Engineer)
        {
            ventTarget = role.Cast<EngineerRole>().currentTarget;
        }
        var genericEvent = new VanillaButtonClickEvent(__instance, playerTarget, ventTarget);
        MiraEventManager.InvokeEvent(genericEvent);
        if (genericEvent.IsCancelled)
        {
            MiraEventManager.InvokeEvent(new VanillaButtonCancelledEvent(__instance));
        }

        return !genericEvent.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SecondaryAbilityButton), nameof(SecondaryAbilityButton.DoClick))]
    public static bool SecondaryDoClickPrefix(AbilityButton __instance)
    {
        // Invoke the generic button click event.
        PlayerControl? playerTarget = null;
        var role = PlayerControl.LocalPlayer.Data.Role;
        if (role.Role is RoleTypes.Detective)
        {
            playerTarget = role.Cast<DetectiveRole>().currentTarget;
        }
        var genericEvent = new VanillaButtonClickEvent(__instance, playerTarget);
        MiraEventManager.InvokeEvent(genericEvent);
        if (genericEvent.IsCancelled)
        {
            MiraEventManager.InvokeEvent(new VanillaButtonCancelledEvent(__instance));
        }

        return !genericEvent.IsCancelled;
    }
}
