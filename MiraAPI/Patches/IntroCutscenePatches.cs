using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System;
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

    /// <summary>
    /// Show the custom team intro.
    /// </summary>
    /// <param name="__instance">The intro cutscene instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.ShowTeam))]
    public static void ShowTeamPatch(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return;
        }

        CustomTeamIntroScene introScene = customRole.Configuration.IntroScene;
        __instance.BackgroundBar.material.SetColor(ShaderID.Color, introScene.TeamColor);
        __instance.ImpostorText.text = introScene.TeamDescription;

        if (introScene.BaseTeam == ModdedRoleTeams.Crewmate && introScene.TeamDescription == string.Empty)
        {
            int adjustedNumImpostors = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
            if (adjustedNumImpostors == 1)
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS, Array.Empty<Il2CppSystem.Object>());
            }
            else
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, adjustedNumImpostors);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    public static bool BeginNeutralPatch(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return true;
        }

        CustomTeamIntroScene introScene = customRole.Configuration.IntroScene;
        __instance.TeamTitle.text = introScene.TeamName;
        __instance.TeamTitle.color = introScene.TeamColor;

        if (customRole.Team is not ModdedRoleTeams.Neutral)
        {
            return true;
        }

        __instance.ourCrewmate = __instance.CreatePlayer(
            0,
            Mathf.CeilToInt(7.5f),
            PlayerControl.LocalPlayer.Data,
            false);

        return false;
    }

    /*
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.OnDestroy))]
    public static void GameBeginPatch()
    {
        CustomGameModeManager.ActiveMode?.Initialize();
    }*/
}
