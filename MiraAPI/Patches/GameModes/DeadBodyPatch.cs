/*
[HarmonyPatch(typeof(DeadBody))]
public static class DeadBodyPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DeadBody.OnClick))]
    public static bool OnClickPatch(DeadBody __instance)
    {
        return CustomGameModeManager.ActiveMode == null || CustomGameModeManager.ActiveMode.CanReport(__instance);
    }
}*/
