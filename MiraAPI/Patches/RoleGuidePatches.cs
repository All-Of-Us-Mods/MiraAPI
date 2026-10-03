using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
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
    public static PassiveButton ViewButton;
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

        var playerMenuGradient = __instance.matchInfoPlayersMaskArea.transform.parent.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient"));
        var settingsenuGradient = __instance.matchInfoSettingsMaskArea.transform.parent.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient"));
        var roleMenuGradient = __instance.MatchInfoRoleMaskArea.transform.parent.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient"));
        playerMenuGradient
                .GetComponent<SpriteRenderer>()
                .maskInteraction =
            settingsenuGradient.GetComponent<SpriteRenderer>()
                    .maskInteraction =
                roleMenuGradient.GetComponent<SpriteRenderer>()
                    .maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        roleMenuGradient.transform.localScale = new Vector3(0.5297f, 0.1565f, 1);
        roleMenuGradient.transform.localPosition = new Vector3(0, -0.62f, -5);
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
                var settingsButton = Object.Instantiate(
                    HudManager.Instance.SettingsButton,
                    __instance.TabButtons[2].transform.parent);
                ViewButton = settingsButton.GetComponent<PassiveButton>();
                ViewButton.OnClick = new Button.ButtonClickedEvent();
                ViewButton.OnClick.AddListener(new Action(SwitchOrder));
                ViewButton.name = "ViewButton";
                settingsButton.transform.localPosition = new Vector3(2.4f, 0.765f, -0.1f);
                settingsButton.transform.localScale = Vector3.one;
                settingsButton.transform.GetChild(2).gameObject.SetActive(false);
                __instance.numOfTabs = 3;
                __instance.TabButtons[0].SelectButton(true);
                __instance.MatchInfoRoleMaskArea.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);

                __instance.matchInfoPlayersMaskArea.transform.localPosition =
                    __instance.matchInfoSettingsMaskArea.transform.localPosition = new Vector3(1.22f, -0.335f, -0.1f);

                __instance.matchInfoPlayersMaskArea.size =
                    __instance.matchInfoSettingsMaskArea.size =
                        __instance.MatchInfoRoleMaskArea.size = new Vector2(-6, 1.8f);

                var playerMenuGradient = __instance.matchInfoPlayersMaskArea.transform.parent.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient"));
                var settingsenuGradient = __instance.matchInfoSettingsMaskArea.transform.parent.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient"));
                var roleMenuGradient = __instance.MatchInfoRoleMaskArea.transform.parent.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient"));
                playerMenuGradient
                        .GetComponent<SpriteRenderer>()
                        .maskInteraction =
                    settingsenuGradient.GetComponent<SpriteRenderer>()
                            .maskInteraction =
                        roleMenuGradient.GetComponent<SpriteRenderer>()
                            .maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                roleMenuGradient.transform.localScale = new Vector3(0.5297f, 0.1565f, 1);
                roleMenuGradient.transform.localPosition = new Vector3(0, -0.62f, -5);
                __instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
                __instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);

                var wikiTab = Object.Instantiate(__instance.settingsTabs[2], __instance.settingsTabs[2].transform.parent);
                wikiTab.transform.FindChild("MaskArea")?.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);
                wikiTab.transform.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
                advancedWikiTab = wikiTab.GetComponent<Scroller>();

                DisplayNormalRoleSettings(__instance, true);

                var searchBox = Object.Instantiate(__instance.TabButtons[2].gameObject, __instance.TabButtons[2].transform.parent);
                var searchButton = searchBox.GetComponent<MatchInfoGuideTabButton>();
                var tmpText = searchButton.transform.GetChild(0).GetComponent<TextMeshPro>();
                tmpText.GetComponent<TextTranslatorTMP>().Destroy();
                tmpText.color = new Color(0.75f, 0.75f, 0.75f);
                tmpText.text = string.Empty;
                tmpText.fontSizeMax = 4;
                tmpText.overflowMode = TextOverflowModes.Ellipsis;
                tmpText.alignment = TextAlignmentOptions.Left;
                tmpText.horizontalAlignment = HorizontalAlignmentOptions.Left;
                tmpText.rectTransform.offsetMax = new Vector2(-0.1531f, 0.2972f);
                tmpText.rectTransform.sizeDelta = new Vector2(3, 1);
                tmpText.transform.localPosition = new Vector3(0, 0.0343f, -0.2f);
                var inactive = searchButton.inactiveSprites;
                var selected = searchButton.selectedSprites;
                var highlight = searchButton.activeSprites;
                var disabledSprite = searchButton.disabledSprites;
                disabledSprite.GetComponent<SpriteRenderer>().size = new Vector2(3, 0.6f);
                searchButton.Destroy();
                inactive.gameObject.SetActive(false);
                selected.gameObject.SetActive(false);
                highlight.gameObject.SetActive(false);
                disabledSprite.gameObject.SetActive(true);
                var button = searchBox.AddComponent<PassiveButton>();
                button.OnUp = true;
                searchBoxTmp = searchBox.AddComponent<TextBoxTMP>();
                searchBoxTmp.outputText = tmpText;
                button.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        searchBoxTmp.GiveFocus();
                    }));
                button.OnMouseOver = new UnityEvent();
                button.OnMouseOut = new UnityEvent();
                var mainScroller = __instance.settingsTabs[2].GetComponent<Scroller>();
                searchBoxTmp.OnChange = new Button.ButtonClickedEvent();
                searchBoxTmp.OnChange.AddListener(
                    (UnityAction)(() =>
                    {
                        var text = searchBoxTmp.outputText.text;
                        var newSorted = RolePanels
                            .OrderByDescending(child =>
                                child.Value.GetTitle().Equals(text, StringComparison.OrdinalIgnoreCase))
                            .ThenByDescending(child => child.Value.GetTitle().Contains(
                                text,
                                StringComparison.InvariantCultureIgnoreCase))
                            .ThenBy(GetSortingOrder());

                        foreach (var pair in newSorted)
                        {
                            pair.Value.Panel.transform.SetAsLastSibling();
                        }

                        mainScroller.ScrollToTop();
                    }));
                searchBoxTmp.transform.localPosition = new Vector3(-1.438f, 0.756f, -0.2f);

                var playerButton = __instance.TabButtons[0];
                var playerCollider = playerButton.GetComponent<BoxCollider2D>();
                playerButton.transform.GetChild(0).gameObject.SetActive(false);
                var playerBtnInactive = playerButton.inactiveSprites.GetComponent<SpriteRenderer>();
                playerBtnInactive.sprite = MiraAssets.WikiPlayersButtonIdleSprite;
                playerBtnInactive.size = Vector2.one;
                playerBtnInactive.transform.GetChild(0).gameObject.SetActive(false);
                var playerBtnSelected = playerButton.selectedSprites.GetComponent<SpriteRenderer>();
                playerBtnSelected.sprite = MiraAssets.WikiPlayersButtonOpenSprite;
                playerBtnSelected.size = Vector2.one;
                playerBtnSelected.transform.GetChild(0).gameObject.SetActive(false);
                var playerBtnHighlight = playerButton.activeSprites.GetComponent<SpriteRenderer>();
                playerBtnHighlight.sprite = MiraAssets.WikiPlayersButtonHoverSprite;
                playerBtnHighlight.size = Vector2.one;
                playerBtnHighlight.transform.GetChild(0).gameObject.SetActive(false);

                var settingButton = __instance.TabButtons[1];
                var settingsCollider = settingButton.GetComponent<BoxCollider2D>();
                settingButton.transform.GetChild(0).gameObject.SetActive(false);
                var settingBtnInactive = settingButton.inactiveSprites.GetComponent<SpriteRenderer>();
                settingBtnInactive.sprite = MiraAssets.WikiSettingsButtonIdleSprite;
                settingBtnInactive.size = Vector2.one;
                settingBtnInactive.transform.GetChild(0).gameObject.SetActive(false);
                var settingBtnSelected = settingButton.selectedSprites.GetComponent<SpriteRenderer>();
                settingBtnSelected.sprite = MiraAssets.WikiSettingsButtonOpenSprite;
                settingBtnSelected.size = Vector2.one;
                settingBtnSelected.transform.GetChild(0).gameObject.SetActive(false);
                var settingBtnHighlight = settingButton.activeSprites.GetComponent<SpriteRenderer>();
                settingBtnHighlight.sprite = MiraAssets.WikiSettingsButtonHoverSprite;
                settingBtnHighlight.size = Vector2.one;
                settingBtnHighlight.transform.GetChild(0).gameObject.SetActive(false);

                var rolesButton = __instance.TabButtons[2];
                var rolesCollider = rolesButton.GetComponent<BoxCollider2D>();
                rolesButton.transform.GetChild(0).gameObject.SetActive(false);
                var rolesBtnInactive = rolesButton.inactiveSprites.GetComponent<SpriteRenderer>();
                rolesBtnInactive.sprite = MiraAssets.WikiRolesButtonIdleSprite;
                rolesBtnInactive.size = Vector2.one;
                rolesBtnInactive.transform.GetChild(0).gameObject.SetActive(false);
                var rolesBtnSelected = rolesButton.selectedSprites.GetComponent<SpriteRenderer>();
                rolesBtnSelected.sprite = MiraAssets.WikiRolesButtonOpenSprite;
                rolesBtnSelected.size = Vector2.one;
                rolesBtnSelected.transform.GetChild(0).gameObject.SetActive(false);
                var rolesBtnHighlight = rolesButton.activeSprites.GetComponent<SpriteRenderer>();
                rolesBtnHighlight.sprite = MiraAssets.WikiRolesButtonHoverSprite;
                rolesBtnHighlight.size = Vector2.one;
                rolesBtnHighlight.transform.GetChild(0).gameObject.SetActive(false);

                playerCollider.size = settingsCollider.size = rolesCollider.size = new Vector2(0.8f, 0.74f);
                playerCollider.offset = settingsCollider.offset = rolesCollider.offset = Vector2.zero;
                playerButton.transform.localScale = settingButton.transform.localScale = rolesButton.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                playerButton.transform.localPosition = new Vector3(-3.6f, 0.656f, -0.2f);
                settingButton.transform.localPosition = new Vector3(-3.6f, 0.056f, -0.2f);
                rolesButton.transform.localPosition = new Vector3(-3.6f, -0.544f, -0.2f);
            }
            else
            {
                DisplayNormalRoleSettings(__instance, false);
                searchBoxTmp.SetText(string.Empty);
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

    private static readonly Dictionary<RoleBehaviour, DetailedPanel> RolePanels = [];
    private static Scroller advancedWikiTab;
    private static TextBoxTMP searchBoxTmp;
    private static GameObject currentAdvancedTabObject;
    private static TextMeshPro titleText;

    public sealed class DetailedPanel(MatchInfoRolePanel panel, string title, string category, string modId)
    {
        public MatchInfoRolePanel Panel = panel;
        public string Title = title;
        public string Category = category;
        public string ModId = modId;
        public int Amount { get; set; }
        public int Chance { get; set; }
        public float LikelyhoodOfRole { get; set; }
    }

    private static SortingFilter sortFilter = SortingFilter.EnabledOnly;
    private static SortingMethod sortMethod = SortingMethod.Alphabetical;
    private static SortingOrder sortOrder = SortingOrder.RoleName;

    public static void SwitchFilter()
    {
        var stepUp = (SortingFilter)((int)sortFilter + 1);
        if (Enum.IsDefined(stepUp))
        {
            sortFilter = stepUp;
        }
        else
        {
            sortFilter = SortingFilter.EnabledOnly;
        }

        var newSorted = sortMethod is SortingMethod.Alphabetical ? RolePanels.OrderBy(GetSortingOrder()) : RolePanels.OrderByDescending(GetSortingOrder());

        foreach (var pair in newSorted)
        {
            pair.Value.Panel.transform.SetAsLastSibling();
        }
    }

    public static void SwitchMethod()
    {
        var stepUp = (SortingMethod)((int)sortMethod + 1);
        if (Enum.IsDefined(stepUp))
        {
            sortMethod = stepUp;
        }
        else
        {
            sortMethod = SortingMethod.Alphabetical;
        }

        var newSorted = sortMethod is SortingMethod.Alphabetical ? RolePanels.OrderBy(GetSortingOrder()) : RolePanels.OrderByDescending(GetSortingOrder());

        foreach (var pair in newSorted)
        {
            pair.Value.Panel.transform.SetAsLastSibling();
        }
    }

    public static void SwitchOrder()
    {
        var stepUp = (SortingOrder)((int)sortOrder + 1);
        if (Enum.IsDefined(stepUp))
        {
            sortOrder = stepUp;
        }
        else
        {
            sortOrder = SortingOrder.RoleName;
        }

        var newSorted = sortMethod is SortingMethod.Alphabetical ? RolePanels.OrderBy(GetSortingOrder()) : RolePanels.OrderByDescending(GetSortingOrder());

        foreach (var pair in newSorted)
        {
            pair.Value.Panel.transform.SetAsLastSibling();
        }
    }

    public static void DisplayNormalRoleSettings(MatchInfoGuide instance, bool reset)
    {
        var inner = instance.settingsTabs[2].GetComponent<Scroller>().Inner;
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
                        inner);
                    var amount = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);
                    var chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
                    panel.SetPanel(
                        roleBehaviour,
                        amount,
                        chance);
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
                    RolePanels.Add(
                        roleBehaviour,
                        new DetailedPanel(
                            panel,
                            roleBehaviour.GetRoleName(),
                            roleBehaviour.GetCategoryTitle(),
                            roleBehaviour is ICustomRole custom ? custom.ParentMod.MiraPlugin.GetAbbreviatedModName() : "AU"));
                }
            }
        }

        advancedWikiTab?.gameObject.SetActive(false);

        var num = 0;
        foreach (var (roleData, holder) in RolePanels)
        {
            var panel = holder.Panel;
            var amount = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleData.Role);
            var chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleData.Role);
            var forciblyShow = roleData is ICustomRole custom ? custom.ForceShowRoleOnWiki : null;
            panel.SetPanel(
                roleData,
                amount,
                chance);
            holder.Amount = amount;
            holder.Chance = chance;
            holder.LikelyhoodOfRole = amount * chance;
            if (amount == 0 || chance == 0 || (Enum.IsDefined(roleData.Role) && roleData.IsRoleBlacklisted()) ||
                (roleData is ICustomRole custom2 && ((!custom2.CanSpawnOnCurrentMode() && forciblyShow == null) ||
                                                                    (forciblyShow.HasValue && !forciblyShow.Value))))
            {
                panel.gameObject.SetActive(false);
                continue;
            }

            panel.gameObject.SetActive(true);
            num++;
        }

        var newSorted = sortMethod is SortingMethod.Alphabetical ? RolePanels.OrderBy(GetSortingOrder()) : RolePanels.OrderByDescending(GetSortingOrder());

        foreach (var pair in newSorted)
        {
            pair.Value.Panel.transform.SetAsLastSibling();
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

    public static Func<KeyValuePair<RoleBehaviour, DetailedPanel>, string> GetSortingOrder()
    {
        switch (sortOrder)
        {
            case SortingOrder.Faction:
                return x => $"{x.Value.Category} ({x.Value.Title}) {x.Value.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)}";
            case SortingOrder.AmountChance:
                return x => $"{x.Value.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)} {x.Value.Title} ({x.Value.Category})";
            default:
                return x => $"{x.Value.Title} ({x.Value.Category}) {x.Value.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)}";
        }
    }

    public static string GetTitle(this DetailedPanel panel)
    {
        switch (sortOrder)
        {
            case SortingOrder.Faction:
                return $"{panel.Category} ({panel.Title}) {panel.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)}";
            case SortingOrder.AmountChance:
                return $"{panel.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)} {panel.Title} ({panel.Category})";
            default:
                return $"{panel.Title} ({panel.Category}) {panel.LikelyhoodOfRole.ToString("0000.0", CultureInfo.InvariantCulture)}";
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
            __instance.roleDescription.text = $"<size=60%>{role.GetCategoryTitle()}</size>\n" + customRole.RoleMedDescription;
            __instance.roleIcon.sprite = customRole.Configuration.Icon?.LoadAsset();
            __instance.roleCount.text += $" ({customRole.ParentMod.MiraPlugin.GetAbbreviatedModName()})";
        }
        else
        {
            __instance.roleName.text = role.NiceName;
            __instance.roleDescription.text = $"<size=60%>{role.GetCategoryTitle()}</size>\n" + role.BlurbMed;
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

    public static string GetCategoryTitle(this RoleBehaviour role)
    {
        if (role is ICustomRole customRole)
        {
            return customRole.GetCategoryTitle();
        }

        return TranslationController.Instance.GetString(
            role.TeamType is RoleTeamTypes.Crewmate ? StringNames.Crewmate : StringNames.Impostor);
    }

    public static string GetCategoryTitle(this ICustomRole customRole)
    {
        return customRole.RoleFactionTitle;
    }
}

public enum SortingOrder
{
    RoleName,
    Faction,
    AmountChance
}

public enum SortingMethod
{
    Alphabetical,
    AlphabeticalDescending
}

public enum SortingFilter
{
    EnabledOnly,
    DisabledOnly,
    AllRoles
}
