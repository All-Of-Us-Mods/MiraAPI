/*
/// <summary>
/// Allow Impostors to kill each other if can kill is enabled in gamemode or friendly fire is toggled on
/// </summary>
[HarmonyPatch(typeof(ImpostorRole), "IsValidTarget")]
public static class ImpostorTargetPatch
{
    public static bool Prefix(ImpostorRole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target, ref bool __result)
    {
        var runOriginal = true;
        var result = false;
        CustomGameModeManager.ActiveMode?.CanKill(out runOriginal, out result, target.Object);
        if (runOriginal || !result)
        {
            return true;
        }

        __result = target is { Disconnected: false, IsDead: false } &&
                   target.PlayerId != __instance.Player.PlayerId && !(target.Role == null) &&
                   !(target.Object == null) && !target.Object.inVent && !target.Object.inMovingPlat;
        return false;

    }
}*/
