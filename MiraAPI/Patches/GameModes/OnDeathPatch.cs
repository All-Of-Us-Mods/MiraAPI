/*
[HarmonyPatch(typeof(KillAnimation))]
public static class OnDeathPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(KillAnimation.CoPerformKill))]
    public static void OnDeathPostfix([HarmonyArgument(0)] PlayerControl source, [HarmonyArgument(1)] PlayerControl target)
    {
        CustomGameModeManager.ActiveMode?.OnDeath(target);
    }
}*/
