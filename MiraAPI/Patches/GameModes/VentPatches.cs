

/*
[HarmonyPatch(typeof(Vent))]
public static class VentPatches
{
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Vent.CanUse))]
    public static bool CanUseVentPatch(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        if (CustomGameModeManager.ActiveMode?.CanVent(__instance, pc) == false)
        {
            return couldUse = canUse = false;
        }

        if (pc.Role is not ICustomRole role)
        {
            return couldUse = canUse = true;
        }

        var num = float.MaxValue;
        var @object = pc.Object;
        var customRoleUsable = role.CanUseVent;

        canUse = couldUse = customRoleUsable && !pc.IsDead && (@object.CanMove || @object.inVent);
        if (canUse)
        {
            var center = @object.Collider.bounds.center;
            var position = __instance.transform.position;
            num = Vector2.Distance(center, position);
            canUse &= num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        }
        __result = num;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Vent.SetOutline))]
    public static bool SetOutlinePatch(Vent __instance, [HarmonyArgument(0)] bool on, [HarmonyArgument(1)] bool mainTarget)
    {
        var color = PlayerControl.LocalPlayer.Data.Role is ICustomRole role
            ? role.RoleColor : PlayerControl.LocalPlayer.Data.Role.IsImpostor ? Palette.ImpostorRed : Palette.CrewmateBlue;
        __instance.myRend.material.SetFloat(ShaderID.Outline, on ? 1 : 0);
        __instance.myRend.material.SetColor(ShaderID.OutlineColor, color);
        __instance.myRend.material.SetColor(ShaderID.AddColor, mainTarget ? color : Color.clear);

        return false;
    }
    
}*/