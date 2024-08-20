using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(IntroCutscene))]
public static class IntroCutscenePatches
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    public static void BeginImpostorPatch(IntroCutscene __instance)
    {
        if (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.ActiveMode.ShowCustomRoleScreen())
        {
            var mode = CustomGameModeManager.ActiveMode;
            __instance.TeamTitle.text = $"<size=70%>{mode.Name}</size>\n<size=20%>{mode.Description}</size>";
        }
    }*/
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    public static bool BeginCrewmatePatch(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is ICustomRole customRole)
        {
            if (!customRole.IsNeutral)
            {
                return true;
            }

            var barTransform = __instance.BackgroundBar.transform;
            var position = barTransform.position;
            position.y -= 0.25f;
            barTransform.position = position;

            __instance.BackgroundBar.material.SetColor(ShaderID.Color, Color.gray);
            __instance.TeamTitle.text = "OUTCAST";
            __instance.impostorScale = 1f;
            __instance.ImpostorText.text = "You are an Outcast. You do not have a team.";
            __instance.TeamTitle.color = Color.gray;

            __instance.ourCrewmate = __instance.CreatePlayer(0, Mathf.CeilToInt(7.5f), PlayerControl.LocalPlayer.Data, false);
            return false;
        }
        return true;
    }

    /*
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.OnDestroy))]
    public static void GameBeginPatch()
    {
        CustomGameModeManager.ActiveMode?.Initialize();
    }*/
}