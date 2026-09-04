using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

[HarmonyPatch]
public static class RoleGuidePatches
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateNormalModeSettings))]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateHnSModeSettings))]
    public static void CreateNormalModeSettings(MatchInfoGuide __instance)
    {
        __instance.MatchInfoRoleMaskArea.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);

        __instance.matchInfoPlayersMaskArea.transform.localPosition =
            __instance.matchInfoSettingsMaskArea.transform.localPosition = new Vector3(1.22f, -0.335f, -0.1f);

        __instance.matchInfoPlayersMaskArea.size =
            __instance.matchInfoSettingsMaskArea.size =
                __instance.MatchInfoRoleMaskArea.size = new Vector2(-6, 1.8f);

        __instance.matchInfoPlayersMaskArea.transform.parent.GetAllChildren().First(x => x.name.Contains("BG_Gradient"))
                .GetComponent<SpriteRenderer>()
                .maskInteraction =
            __instance.matchInfoSettingsMaskArea.transform.parent.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient")).GetComponent<SpriteRenderer>()
                    .maskInteraction =
                __instance.MatchInfoRoleMaskArea.transform.parent.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient")).GetComponent<SpriteRenderer>()
                    .maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Awake))]
    public static void Awake(MatchInfoGuide __instance)
    {
        __instance.transitionOpen.targetSize = 1.3f;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Open))]
    public static bool Open(MatchInfoGuide __instance)
    {
        if (HudManager.Instance.GameMenu.IsOpen || HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return false;
        }

        if (Minigame.Instance != null)
        {
            Minigame.Instance.Close();
        }
        if (MapBehaviour.Instance)
        {
            MapBehaviour.Instance.Close();
        }
        if (HudManager.InstanceExists)
        {
            ConsoleJoystick.SetMode_MenuAdditive();
        }
        ControllerManager.Instance.OpenOverlayMenu("MatchInfoGuide", __instance.closeButton);
        bool enabled = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
        __instance.glyphL.enabled = enabled;
        __instance.glyphR.enabled = enabled;
        if (GameManager.Instance.TryCast<NormalGameManager>() != null)
        {
            if (__instance.NormalModeSettings.Count == 0)
            {
                __instance.numOfTabs = 3;
                __instance.TabButtons[0].SelectButton(true);
                DisplayNormalRoleSettings(__instance, true);
                __instance.MatchInfoRoleMaskArea.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);

                __instance.matchInfoPlayersMaskArea.transform.localPosition =
                    __instance.matchInfoSettingsMaskArea.transform.localPosition = new Vector3(1.22f, -0.335f, -0.1f);

                __instance.matchInfoPlayersMaskArea.size =
                    __instance.matchInfoSettingsMaskArea.size =
                        __instance.MatchInfoRoleMaskArea.size = new Vector2(-6, 1.8f);

                __instance.matchInfoPlayersMaskArea.transform.parent.GetAllChildren().First(x => x.name.Contains("BG_Gradient"))
                        .GetComponent<SpriteRenderer>()
                        .maskInteraction =
                    __instance.matchInfoSettingsMaskArea.transform.parent.GetAllChildren()
                            .First(x => x.name.Contains("BG_Gradient")).GetComponent<SpriteRenderer>()
                            .maskInteraction =
                        __instance.MatchInfoRoleMaskArea.transform.parent.GetAllChildren()
                            .First(x => x.name.Contains("BG_Gradient")).GetComponent<SpriteRenderer>()
                            .maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
            else
            {
                DisplayNormalRoleSettings(__instance, false);
            }
        }
        else if (__instance.HnSModeSettings.Count == 0)
        {
            __instance.numOfTabs = 2;
            __instance.TabButtons[0].SelectButton(true);
            __instance.CreateHnSModeSettings();
        }
        PlayerControl.LocalPlayer.NetTransform.Halt();
        __instance.MatchInfoParent.SetActive(true);
        ControllerManager instance = ControllerManager.Instance;
        ControllerUiElementsState currentUiState = ControllerManager.Instance.CurrentUiState;
        Il2CppSystem.Collections.Generic.List<UiElement> controllerSelectable = __instance.ControllerSelectable;
        instance.SetUpSelectables(currentUiState, controllerSelectable[controllerSelectable.Count - 1], __instance.ControllerSelectable);
        ControllerManager instance2 = ControllerManager.Instance;
        Il2CppSystem.Collections.Generic.List<UiElement> controllerSelectable2 = __instance.ControllerSelectable;
        instance2.SetCurrentSelected(controllerSelectable2[controllerSelectable2.Count - 1]);
        __instance.SetActiveTab(0);
        return false;
    }

    private static Dictionary<RoleBehaviour, MatchInfoRolePanel> _rolePanels = [];

    public static void DisplayNormalRoleSettings(MatchInfoGuide instance, bool reset)
    {
        if (reset)
        {
            _rolePanels.Clear();
            instance.CreateSettingsEntry(
                StringNames.GameNumImpostors,
                GameManager.Instance.AllGameSettingData[StringNames.GameNumImpostors]
                    .GetValueString(GameManager.Instance.LogicOptions.NumImpostors));
            instance.CreateSettingsEntry(
                StringNames.GameKillCooldown,
                GameManager.Instance.AllGameSettingData[StringNames.GameKillCooldown]
                    .GetValueString(GameManager.Instance.LogicOptions.GetKillCooldown()));
            instance.CreateSettingsEntry(
                StringNames.GameEmergencyCooldown,
                GameManager.Instance.AllGameSettingData[StringNames.GameEmergencyCooldown]
                    .GetValueString((float)GameManager.Instance.LogicOptions.GetEmergencyCooldown()));
            instance.CreateSettingsEntry(
                StringNames.GameVisualTasks,
                instance.GetBoolString(GameManager.Instance.LogicOptions.GetVisualTasks()));
            instance.CreateSettingsEntry(
                StringNames.GameAnonymousVotes,
                instance.GetBoolString(GameManager.Instance.LogicOptions.GetAnonymousVotes()));
            instance.CreateSettingsEntry(
                StringNames.GameConfirmImpostor,
                instance.GetBoolString(GameManager.Instance.LogicOptions.GetConfirmImpostor()));
            instance.CreateSettingsEntry(
                StringNames.GameTaskBarMode,
                GameManager.Instance.LogicOptions.GetTaskBarMode().ToString());
            foreach (RoleBehaviour roleBehaviour in RoleManager.Instance.AllRoles)
            {
                if (roleBehaviour.Role != RoleTypes.Crewmate && roleBehaviour.Role != RoleTypes.Impostor &&
                    roleBehaviour.Role is not RoleTypes.CrewmateGhost &&
                    roleBehaviour.Role is not RoleTypes.ImpostorGhost)
                {
                    var panel = Object.Instantiate(
                        instance.MatchInfoRolePanelPrefab,
                        instance.settingsTabs[2].GetComponent<Scroller>().Inner);
                    _rolePanels.Add(roleBehaviour, panel);
                }
            }
        }

        int num = 0;
        foreach (var pair in _rolePanels)
        {
            var role = pair.Key;
            var panel = pair.Value;
            var amount = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role.Role);
            var chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(role.Role);
            var forciblyShow = role is ICustomRole custom ? custom.ForceShowRoleOnWiki : null;
            if (amount == 0 || chance == 0 || (Enum.IsDefined(role.Role) && role.IsRoleBlacklisted()) ||
                (role is ICustomRole custom2 && ((!custom2.CanSpawnOnCurrentMode() && forciblyShow == null) ||
                                                 (forciblyShow.HasValue && !forciblyShow.Value))))
            {
                panel.gameObject.SetActive(false);
                continue;
            }

            panel.gameObject.SetActive(true);
            panel.SetPanel(
                role,
                amount,
                chance);
            num++;
        }

        if (num == 0)
        {
            instance.rolesEnabledMessage.SetActive(true);
        }

        instance.MatchInfoRoleScroller.SetYBoundsMax(Mathf.Clamp(Mathf.Ceil((float)num / 2f) * 1.3f - 1.5f, 0f, 999f));
        instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
        instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
        if (reset)
        {
            instance.CreatePlayerEntries();
        }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreatePlayerEntries))]
    private static bool CreatePlayerEntries(MatchInfoGuide __instance)
    {
        __instance.PlayerPool.ReclaimAll();
        int num = 51;
        foreach (NetworkedPlayerInfo networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            PlayerIdentifierButton component =
                __instance.PlayerPool.Get<PoolableBehavior>().GetComponent<PlayerIdentifierButton>();
            component.transform.localPosition = new Vector3(0f, 0f, -1f);
            component.Populate(networkedPlayerInfo);
            __instance.ControllerSelectable.Add(component.Button);
            component.SetTextStencil(num++);
            component.PlatformIdentifier.transform.localPosition = new Vector3(0.314f, 0.088f, -2.78f);
            component.NameText.transform.localPosition = new Vector3(0.3563f, 0, -2.98f);
            component.NameText.text += $"\n<size=75%>{networkedPlayerInfo.GetPlayerColorString()}</size>";
            var namePlate = HatManager.Instance.GetNamePlateById(networkedPlayerInfo.DefaultOutfit.NamePlateId);
            var x = (NamePlateViewData viewdata) =>
            {
                component.buttonSprite.sprite = viewdata?.Image;
                component.buttonSprite.transform.localScale = new Vector3(0.7f, 1.075f, 1);
                component.buttonSprite.transform.localPosition = new Vector3(-0.395f, 0, 0.1f);
            };
            __instance.StartCoroutine(
                AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>(
                    __instance,
                    namePlate.GetAssetReference(),
                    x));
        }

        return false;
    }
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoRolePanel), nameof(MatchInfoRolePanel.SetPanel))]
    public static bool SetPanel(MatchInfoRolePanel __instance, RoleBehaviour role, int numPerGame, int chancePerGame)
    {
        __instance.roleCount.text = string.Format(CultureInfo.InvariantCulture, "{0} at {1}%", numPerGame.ToString(CultureInfo.InvariantCulture), chancePerGame);
        if (role is ICustomRole customRole)
        {
            __instance.roleName.text = customRole.RoleName;
            __instance.roleDescription.text = $"<size=60%>{customRole.RoleFactionTitle}</size>\n" + customRole.RoleMedDescription;
            __instance.roleIcon.sprite = customRole.Configuration.Icon?.LoadAsset();
            __instance.roleCount.text += $" ({customRole.ParentMod.MiraPlugin.GetAbbreviatedModName()})";
        }
        else
        {
            __instance.roleName.text = role.NiceName;
            __instance.roleDescription.text = $"<size=60%>{TranslationController.Instance.GetString(role.TeamType is RoleTeamTypes.Crewmate ? StringNames.Crewmate : StringNames.Impostor)}</size>\n" + role.BlurbMed;
            __instance.roleIcon.sprite = role.RoleIconColor;
            __instance.roleCount.text += " (AU)";
        }

        __instance.roleIcon.SetSizeLimit(0.13f);
        __instance.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
        __instance.roleName.fontMaterial.SetFloat(__instance.STENCIL_NAME, 50f);
        __instance.roleDescription.fontMaterial.SetFloat(__instance.STENCIL_NAME, 50f);
        __instance.roleCount.fontMaterial.SetFloat(__instance.STENCIL_NAME, 50f);
        __instance.roleIcon.transform.localScale = new Vector3(4f, 4f, 1f);
        return false;
    }
}
