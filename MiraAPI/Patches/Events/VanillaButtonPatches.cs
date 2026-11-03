using System;
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
        else if (role.Role is RoleTypes.Detective)
        {
            playerTarget = role.Cast<DetectiveRole>().currentTarget;
        }
        else if (role.Role is RoleTypes.GuardianAngel)
        {
            playerTarget = role.Cast<GuardianAngelRole>().currentTarget;
        }
        else if (role.Role is RoleTypes.SpiritGuide)
        {
            playerTarget = role.Cast<SpiritGuideRole>().currentTarget;
        }
        else if (role.Role is RoleTypes.Engineer)
        {
            ventTarget = role.Cast<EngineerRole>().currentTarget;
        }

        return InvokeEvents(__instance, playerTarget, ventTarget);
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
        else if (role.Role is RoleTypes.SpiritGuide)
        {
            playerTarget = role.Cast<SpiritGuideRole>().currentTarget;
        }

        return InvokeEvents(__instance, playerTarget);
    }

    private static bool InvokeEvents(AbilityButton __instance, PlayerControl? playerTarget, Vent? ventTarget = null)
    {
        var genericEvent = new VanillaButtonClickEvent(__instance, playerTarget, ventTarget);
        genericEvent.InvokeEvent();
        if (genericEvent.IsCancelled)
        {
            new VanillaButtonCancelledEvent(__instance).InvokeEvent();
        }

        var eventType = typeof(VanillaButtonClickEvent<>).MakeGenericType(__instance.GetType());
        var @event = (MiraCancelableEvent)Activator.CreateInstance(eventType, __instance)!;
        var specificInvoked = @event.InvokeEvent(eventType);
        if (@event.IsCancelled)
        {
            var cancelEventType = typeof(VanillaButtonCancelledEvent<>).MakeGenericType(__instance.GetType());
            var cancelEvent = (MiraEvent)Activator.CreateInstance(cancelEventType, __instance)!;
            cancelEvent.InvokeEvent(cancelEventType);
        }

        MiraCancelableEvent eventCheck = specificInvoked ? @event : genericEvent;
        return !eventCheck.IsCancelled;
    }
}
