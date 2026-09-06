using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(HowToPlayScene), nameof(HowToPlayScene.OpenRolesSelectionMenu))]
internal static class HowToPlayPatch
{
    // yes i patched the entire method
    public static void Prefix(HowToPlayScene __instance)
    {
        if (RoleManager.Instance.AllRoles.ToArray().All(x => !x.IsCustomRole()))
        {
            return;
        }
        __instance.sceneIndex = 0;
        __instance.category = HowToPlayScene.HowToPlayCategory.RolesSelection;
        __instance.startPage.SetActive(false);
        if (__instance.roleButtonsParent.childCount == 0)
        {
            foreach (var role in RoleManager.Instance.AllRoles.ToArray().Where(x => !x.IsCustomRole()))
            {
                if (!role.IsSimpleRole && role.Role != RoleTypes.CrewmateGhost && role.Role != RoleTypes.ImpostorGhost)
                {
                    var component = Object.Instantiate(__instance.roleButtonPrefab, __instance.roleButtonsParent).GetComponent<HowToPlayRoleButton>();
                    var roleIcon = __instance.rolesScenes.ToArray().First(r => r.role == role.Role).roleIcon;
                    component.SetRoleInfo(role, roleIcon);
                    component.SetButtonAction((Il2CppSystem.Action)(() =>
                    {
                        OpenRolePage(__instance, role.Role);
                    }));
                    __instance.controllerSelectables.Add(component.GetComponent<PassiveButton>());
                }
            }
            foreach (var uiElement in __instance.controllerSelectables)
            {
                uiElement.ReceiveMouseOut();
            }
            ControllerManager.Instance.NewScene(__instance.name, __instance.closeButton, __instance.defaultButtonSelected, __instance.controllerSelectables);
        }
        __instance.DisableAllScenes();
        __instance.roleSelectionScene.SetActive(true);
        ControllerManager.Instance.SetDefaultSelection(__instance.defaultButtonSelected);
    }
    public static void OpenRolePage(HowToPlayScene instance, RoleTypes roleType)
    {
        instance.category = HowToPlayScene.HowToPlayCategory.Roles;
        var newList = instance.rolesScenes.ToArray().ToList();
        var buttonList = instance.roleButtons;
        instance.sceneIndex = newList.FindIndex(r => r.role == roleType);
        if (roleType != RoleTypes.Crewmate)
        {
            foreach (var button in buttonList)
            {
                if (button.GetRole().Role == roleType)
                {
                    instance.previouslySelectedRoleButton = button.GetComponent<PassiveButton>();
                }
            }
        }
        instance.SetupDots(instance.rolesScenes[instance.sceneIndex].rolePages.Count);
        instance.ChangeScene(0);
    }
}
