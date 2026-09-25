using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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
        var wikiTab = Object.Instantiate(__instance.settingsTabs[2], __instance.settingsTabs[2].transform.parent);
        advancedWikiTab = wikiTab.GetComponent<Scroller>();
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
        var enabled = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
        __instance.glyphL.enabled = enabled;
        __instance.glyphR.enabled = enabled;
        if (!titleText)
        {
            titleText = __instance.transitionOpen.transform.FindChild("Text_Title")?.GetComponent<TextMeshPro>()!;
            if (titleText)
            {
                titleText.transform.GetComponent<TextTranslatorTMP>().Destroy();
            }
        }

        if (titleText)
        {
            titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
        }

        if (GameManager.Instance.TryCast<NormalGameManager>() != null)
        {
            if (__instance.NormalModeSettings.Count == 0)
            {
                __instance.numOfTabs = 3;
                __instance.TabButtons[0].SelectButton(true);
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
                __instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
                __instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
                var wikiTab = Object.Instantiate(__instance.settingsTabs[2], __instance.settingsTabs[2].transform.parent);
                wikiTab.transform.FindChild("MaskArea")?.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);
                advancedWikiTab = wikiTab.GetComponent<Scroller>();
                DisplayNormalRoleSettings(__instance, true);
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
        var instance = ControllerManager.Instance;
        var currentUiState = ControllerManager.Instance.CurrentUiState;
        var controllerSelectable = __instance.ControllerSelectable;
        instance.SetUpSelectables(currentUiState, controllerSelectable[^1], __instance.ControllerSelectable);
        var instance2 = ControllerManager.Instance;
        var controllerSelectable2 = __instance.ControllerSelectable;
        instance2.SetCurrentSelected(controllerSelectable2[^1]);
        __instance.SetActiveTab(0);
        return false;
    }

    private static readonly Dictionary<RoleBehaviour, MatchInfoRolePanel> RolePanels = [];
    private static Scroller advancedWikiTab;
    private static GameObject currentAdvancedTabObject;
    private static TextMeshPro titleText;

    public static void DisplayNormalRoleSettings(MatchInfoGuide instance, bool reset)
    {
        if (reset)
        {
            RolePanels.Clear();
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
                    .GetValueString(GameManager.Instance.LogicOptions.GetEmergencyCooldown()));
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

            var hoverColor = new Color32(255, 255, 255, 150);
            foreach (var roleBehaviour in RoleManager.Instance.AllRoles.ToArray().OrderBy(x => x.GetRoleName()))
            {
                if (roleBehaviour.Role is not RoleTypes.Crewmate and not RoleTypes.Impostor and
                    not RoleTypes.CrewmateGhost and
                    not RoleTypes.ImpostorGhost)
                {
                    var panel = Object.Instantiate(
                        instance.MatchInfoRolePanelPrefab,
                        instance.settingsTabs[2].GetComponent<Scroller>().Inner);
                    var collider = panel.roleIcon.gameObject.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(0.13f, 0.13f);
                    collider.offset = new Vector2(0, 0);
                    var passiveButton = panel.roleIcon.gameObject.AddComponent<PassiveButton>();
                    passiveButton.ClickSound = HudManager.Instance.MapButton.ClickSound;
                    passiveButton.OnMouseOver = new UnityEvent();
                    passiveButton.OnMouseOver.AddListener(
                        (UnityAction)(() =>
                        {
                            panel.roleIcon.color = hoverColor;
                        }));
                    passiveButton.OnMouseOut = new UnityEvent();
                    passiveButton.OnMouseOut.AddListener(
                        (UnityAction)(() =>
                        {
                            panel.roleIcon.color = Color.white;
                        }));

                    passiveButton.OnClick = new Button.ButtonClickedEvent();
                    passiveButton.OnClick.AddListener(
                        (Action)(() => { DisplayAdvancedWiki(instance, roleBehaviour); }));
                    RolePanels.Add(roleBehaviour, panel);
                }
            }
        }

        advancedWikiTab?.gameObject.SetActive(false);

        var num = 0;
        foreach (var (roleData, panel) in RolePanels)
        {
            var amount = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleData.Role);
            var chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleData.Role);
            var forciblyShow = roleData is ICustomRole custom ? custom.ForceShowRoleOnWiki : null;
            if (amount == 0 || chance == 0 || (Enum.IsDefined(roleData.Role) && roleData.IsRoleBlacklisted()) ||
                (roleData is ICustomRole custom2 && ((!custom2.CanSpawnOnCurrentMode() && forciblyShow == null) ||
                                                                    (forciblyShow.HasValue && !forciblyShow.Value))))
            {
                panel.gameObject.SetActive(false);
                continue;
            }

            panel.gameObject.SetActive(true);
            panel.SetPanel(
                roleData,
                amount,
                chance);
            num++;
        }

        if (num == 0)
        {
            instance.rolesEnabledMessage.SetActive(true);
        }

        instance.MatchInfoRoleScroller.SetYBoundsMax(Mathf.Clamp(Mathf.Ceil(num / 2f) * 1.3f - 1.5f, 0f, 999f));
        instance.MatchInfoRoleScroller.SetYBoundsMax(Mathf.Clamp(Mathf.Ceil(num / 2f) * 1.3f - 1.5f, 0f, 999f));
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
        var num = 51;
        foreach (var networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            var component =
                __instance.PlayerPool.Get<PoolableBehavior>().GetComponent<PlayerIdentifierButton>();
            component.transform.localPosition = new Vector3(0f, 0f, -1f);
            component.Populate(networkedPlayerInfo);
            __instance.ControllerSelectable.Add(component.Button);
            component.SetTextStencil(num++);
            component.PlatformIdentifier.transform.localPosition = new Vector3(0.314f, 0.088f, -2.78f);
            component.NameText.transform.localPosition = new Vector3(0.3563f, 0, -2.98f);
            component.NameText.text += $"\n<size=75%>{networkedPlayerInfo.GetPlayerColorString()}</size>";
            var namePlate = HatManager.Instance.GetNamePlateById(networkedPlayerInfo.DefaultOutfit.NamePlateId);

            __instance.StartCoroutine(
                __instance.CoLoadAssetAsync<NamePlateViewData>(
                    namePlate.GetAssetReference(),
                    (Action<NamePlateViewData>?)LoadNameplate));

            void LoadNameplate(NamePlateViewData viewData)
            {
                component.buttonSprite.sprite = viewData.Image;
                component.buttonSprite.transform.localScale = new Vector3(0.7f, 1.075f, 1);
                component.buttonSprite.transform.localPosition = new Vector3(-0.395f, 0, 0.1f);
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.SetActiveTab))]
    private static void SetActiveTab()
    {
        if (titleText)
        {
            titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
        }

        advancedWikiTab?.gameObject.SetActive(false);
    }

    public static void DisplayAdvancedWiki(MatchInfoGuide instance, RoleBehaviour role)
    {
        Warning($"Opening advanced tab for {role.GetRoleName()}.");
        instance.settingsTabs[2].SetActive(false);
        advancedWikiTab?.gameObject.SetActive(true);
        if (currentAdvancedTabObject)
        {
            currentAdvancedTabObject.SetActive(false);
            currentAdvancedTabObject.Destroy();
        }

        if (advancedWikiTab == null)
        {
            Warning($"Wiki tab is null.");
            return;
        }

        if (role is ICustomRole custom)
        {
            currentAdvancedTabObject = custom.GetAdvancedWiki(instance, titleText, advancedWikiTab);
        }
        else
        {
            advancedWikiTab.ScrollToTop();
            var titleTxt = role.GetRoleName() + $" ({TranslationController.Instance.GetString(role.TeamType is RoleTeamTypes.Crewmate ? StringNames.Crewmate : StringNames.Impostor)})";
            var description = TranslationController.Instance.GetString(role.BlurbNameLong);
            if (description.Contains("STRMISS"))
            {
                var baseName = $"{role.StringName}".Replace("Role", string.Empty);
                if (Enum.TryParse<StringNames>($"RolesHelp_{baseName}_01", out var helpName))
                {
                    description = TranslationController.Instance.GetString(helpName);
                }
            }

            currentAdvancedTabObject = Helpers.CreateAdvancedWikiTab(
                instance,
                role.Role.ToString(),
                titleTxt,
                description,
                titleText,
                out var desc);
            currentAdvancedTabObject.transform.SetParent(advancedWikiTab.Inner.transform);
            currentAdvancedTabObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            advancedWikiTab.SetYBoundsMax(Mathf.Clamp(desc.textBounds.size.y - 2, 0f, 999f));
        }
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
