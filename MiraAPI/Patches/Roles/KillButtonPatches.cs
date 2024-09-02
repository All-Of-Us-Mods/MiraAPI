﻿using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Networking;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Fix kill button issues for neutral killers.
/// </summary>
[HarmonyPatch(typeof(KillButton))]
public static class KillButtonPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(KillButton.SetTarget))]
    public static bool SetTargetPrefix(KillButton __instance, PlayerControl target)
    {
        if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data == null || !PlayerControl.LocalPlayer.Data.Role)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return true;
        }

        if (__instance.currentTarget && __instance.currentTarget != target)
        {
            __instance.currentTarget.cosmetics.SetOutline(false, new Nullable<Color>(Color.clear));
        }
        __instance.currentTarget = target;
        if (__instance.currentTarget)
        {
            __instance.currentTarget.cosmetics.SetOutline(true, new Nullable<Color>(customRole.RoleColor));
            __instance.SetEnabled();
            return false;
        }
        __instance.SetDisabled();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(KillButton.DoClick))]
    public static bool DoClickPrefix(KillButton __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole)
        {
            return true;
        }

        if (!__instance.isActiveAndEnabled || !__instance.currentTarget || __instance.isCoolingDown ||
            PlayerControl.LocalPlayer.Data.IsDead || !PlayerControl.LocalPlayer.CanMove)
        {
            return false;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(__instance.currentTarget);
        __instance.SetTarget(null);

        return false;
    }
}