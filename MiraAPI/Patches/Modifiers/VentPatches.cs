using HarmonyLib;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Modifiers;

[HarmonyPatch(typeof(Vent))]
public static class VentPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Vent.CanUse))]
    public static void CanUseVentPatch(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        var @object = pc.Object;
        var role = @object.Data.Role;

        var modifiers = @object.GetModifierComponent()?.ActiveModifiers;
        if (modifiers is null || modifiers.Count <= 0)
        {
            return;
        }

        switch (role.CanVent)
        {
            case true when modifiers.Exists(x => !x.CanVent()):
                couldUse = canUse = false;
                return;
            case false when modifiers.Exists(x => x.CanVent()):
                couldUse = true;
                break;
        }

        var num = float.MaxValue;

        canUse = couldUse;
        if (canUse)
        {
            var center = @object.Collider.bounds.center;
            var position = __instance.transform.position;
            num = Vector2.Distance(center, position);
            canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        }
        __result = num;
    }
}