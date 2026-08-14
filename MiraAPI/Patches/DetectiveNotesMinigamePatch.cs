using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(DetectiveNotesMinigame), nameof(DetectiveNotesMinigame.SetImpostorPopup))]
public static class DetectiveNotesMinigamePatch
{
    public static void Prefix(DetectiveNotesMinigame __instance, bool active)
    {
        __instance.impostorTypePopup.gameObject.SetActive(active);
        if (!active)
        {
            ControllerManager.Instance.CloseOverlayMenu(__instance.ImpostorOverlay);
            ControllerManager.Instance.SetUpSelectables(ControllerManager.Instance.CurrentUiState, __instance.ControllerSelectable[0], __instance.ControllerSelectable);
            ControllerManager.Instance.SetCurrentSelected(__instance.ControllerSelectable[2]);
            return;
        }
        if (__instance.impostorButton != null)
        {
            ControllerManager.Instance.OpenOverlayMenu(__instance.ImpostorOverlay, __instance.impostorTypePopup, __instance.ImpostorIconControllerSelectables[0], __instance.ImpostorIconControllerSelectables, false);
            return;
        }
        __instance.impostorButton = new List<GameObject>();
        var allRoles = RoleManager.Instance.AllRoles;
        var customRoles = CustomRoleManager.CustomRoleBehaviours;
        foreach (var role in allRoles)
        {
            if (role.TeamType != RoleTeamTypes.Impostor || role.Role == RoleTypes.ImpostorGhost || customRoles.Contains(role))
                continue;
            var gameObject = Object.Instantiate(__instance.impostorTypePrefab, __instance.impostorTypeParent);
            __instance.impostorButton.Add(gameObject);
            gameObject.GetComponent<DetectiveImpostorType>().Initialize(__instance, role);
            __instance.ImpostorIconControllerSelectables.Add(gameObject.GetComponent<PassiveButton>());
        }
        ControllerManager.Instance.OpenOverlayMenu(__instance.ImpostorOverlay, __instance.impostorTypePopup, __instance.ImpostorIconControllerSelectables[0], __instance.ImpostorIconControllerSelectables, false);
    }
}
