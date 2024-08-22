

/*
[HarmonyPatch]
public static class GameLogicPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static bool EndCritPatch(LogicGameFlowNormal __instance)
    {
        var runOriginal = true;
        CustomGameModeManager.ActiveMode?.CheckGameEnd(out runOriginal, __instance);
        return runOriginal;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
    public static bool AssignRolesPatch(LogicRoleSelectionNormal __instance)
    {
        var runOriginal = true;
        CustomGameModeManager.ActiveMode?.AssignRoles(out runOriginal, __instance);
        return runOriginal;
    }
}*/