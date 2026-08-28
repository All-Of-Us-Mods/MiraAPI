using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
internal static class IntroCutscenePatch
{
    public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        if (CustomGameModeManager.ActiveMode?.ShowGameModeIntroCutscene == true)
        {
            __result = CustomGameModeManager.ActiveMode.IntroCutscene(__instance).WrapToIl2Cpp();
            return false;
        }

        return true;
    }
}
