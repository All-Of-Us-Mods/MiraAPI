using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
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
    private const int RolesTabIndex = 2;
    private const int ModifiersTabIndex = 3;

    private static readonly Dictionary<RoleBehaviour, DetailedPanel> RolePanels = [];
    private static readonly Dictionary<GameModifier, DetailedPanel> ModifierPanels = [];

    public static PassiveButton SearchIconButton;
    private static SpriteRenderer _searchIconIdle;
    private static SpriteRenderer _searchIconHover;
    public static PassiveButton SearchSortingOrderButton;
    private static SpriteRenderer _searchSortingOrderIdle;
    private static SpriteRenderer _searchSortingOrderHover;
    public static PassiveButton SearchSortingGroupButton;
    private static SpriteRenderer _searchSortingGroupIdle;
    private static SpriteRenderer _searchSortingGroupHover;
    public static PassiveButton SearchSortingFilterButton;
    private static SpriteRenderer _searchSortingFilterIdle;
    private static SpriteRenderer _searchSortingFilterHover;
    private static Scroller _rolesScroller;
    private static Scroller _modifiersScroller;
    private static Scroller _advancedInfoTabScroller;
    private static TextBoxTMP _searchBoxTmp;
    private static GameObject _currentAdvancedTabObject;
    private static TextMeshPro _titleText;
    private static bool _rolesTabDirty;
    private static bool _modifiersTabDirty;
    private static IEnumerator? _panelCreation;
    private static bool _panelsReady;
    private static SortingFilter _sortFilter = SortingFilter.EnabledOnly;
    private static SortingMethod _sortMethod = SortingMethod.Alphabetical;
    private static SortingGroups _sortGrouping = SortingGroups.Ungrouped;

    private static bool IsModifiersTabActive => MatchInfoGuide.Instance.activeTabIndex == ModifiersTabIndex;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateNormalModeSettings))]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateHnSModeSettings))]
    public static void CreateNormalModeSettings(MatchInfoGuide __instance)
    {
        ApplyPanelLayout(__instance);
    }

    private static void ApplyPanelLayout(MatchInfoGuide instance)
    {
        instance.MatchInfoRoleMaskArea.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);

        instance.matchInfoPlayersMaskArea.transform.localPosition =
            instance.matchInfoSettingsMaskArea.transform.localPosition = new Vector3(1.22f, -0.335f, -0.1f);

        instance.matchInfoPlayersMaskArea.size =
            instance.matchInfoSettingsMaskArea.size =
                instance.MatchInfoRoleMaskArea.size = new Vector2(-6, 1.8f);

        var playerMenuGradient = instance.matchInfoPlayersMaskArea.transform.parent.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient"));
        var settingsenuGradient = instance.matchInfoSettingsMaskArea.transform.parent.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient"));
        var roleMenuGradient = instance.MatchInfoRoleMaskArea.transform.parent.GetAllChildren()
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
        if (!_titleText)
        {
            _titleText = __instance.transitionOpen.transform.FindChild("Text_Title")?.GetComponent<TextMeshPro>()!;
            if (_titleText)
            {
                _titleText.transform.GetComponent<TextTranslatorTMP>().Destroy();
            }
        }

        if (_titleText)
        {
            _titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
        }

        var regGame = GameManager.Instance.TryCast<NormalGameManager>() != null;
        if (regGame)
        {
            if (__instance.NormalModeSettings.Count == 0)
            {
                var sortingOrderButton = Object.Instantiate(
                    HudManager.Instance.SettingsButton,
                    __instance.TabButtons[2].transform.parent);
                SearchSortingOrderButton = sortingOrderButton.GetComponent<PassiveButton>();
                SetupSearchButton(
                    SearchSortingOrderButton,
                    "SearchSortingOrderButton",
                    SwitchMethod,
                    MiraAssets.SortingAzIdleSprite,
                    MiraAssets.SortingAzHoverSprite,
                    new Vector3(2.4f, 0.765f, -0.1f),
                    out _searchSortingOrderIdle,
                    out _searchSortingOrderHover);

                var sortingGroupButton = Object.Instantiate(
                    sortingOrderButton,
                    sortingOrderButton.transform.parent);
                SearchSortingGroupButton = sortingGroupButton.GetComponent<PassiveButton>();
                SetupSearchButton(
                    SearchSortingGroupButton,
                    "SearchSortingGroupButton",
                    SwitchGrouping,
                    MiraAssets.GroupNoneIdleSprite,
                    MiraAssets.GroupNoneHoverSprite,
                    new Vector3(1.8f, 0.765f, -0.1f),
                    out _searchSortingGroupIdle,
                    out _searchSortingGroupHover);

                var sortingFilterButton = Object.Instantiate(
                    sortingOrderButton,
                    sortingOrderButton.transform.parent);
                SearchSortingFilterButton = sortingFilterButton.GetComponent<PassiveButton>();
                SetupSearchButton(
                    SearchSortingFilterButton,
                    "SearchSortingFilterButton",
                    SwitchFilter,
                    MiraAssets.VisibleOnIdleSprite,
                    MiraAssets.VisibleOnHoverSprite,
                    new Vector3(1f, 0.765f, -0.1f),
                    out _searchSortingFilterIdle,
                    out _searchSortingFilterHover);

                __instance.numOfTabs = 4;
                __instance.TabButtons[2].SelectButton(true);
                ApplyPanelLayout(__instance);
                __instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
                __instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);

                var modifiersTab = Object.Instantiate(
                    __instance.settingsTabs[RolesTabIndex],
                    __instance.settingsTabs[RolesTabIndex].transform.parent);
                modifiersTab.name = "ModifiersPanel";
                modifiersTab.transform.FindChild("MaskArea")?.transform.localPosition =
                    new Vector3(-0.0184f, 0.15f, -0.1f);
                modifiersTab.transform.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
                _modifiersScroller = modifiersTab.GetComponent<Scroller>();
                __instance.settingsTabs.Add(modifiersTab);

                var wikiTab = Object.Instantiate(
                    __instance.settingsTabs[RolesTabIndex],
                    __instance.settingsTabs[RolesTabIndex].transform.parent);
                wikiTab.name = "AdvancedWikiPanels";
                wikiTab.transform.FindChild("MaskArea")?.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);
                wikiTab.transform.GetAllChildren()
                    .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
                _advancedInfoTabScroller = wikiTab.GetComponent<Scroller>();

                DisplayNormalRoleSettings(__instance, true);

                var searchIconButton = Object.Instantiate(
                    sortingOrderButton,
                    sortingOrderButton.transform.parent);
                SearchIconButton = searchIconButton.GetComponent<PassiveButton>();
                SetupSearchButton(
                    SearchIconButton,
                    "SearchIconButton",
                    ClearSearchText,
                    MiraAssets.SearchIconIdleSprite,
                    MiraAssets.SearchIconHoverSprite,
                    new Vector3(-2.79f, 0.765f, -0.1f),
                    out _searchIconIdle,
                    out _searchIconHover);

                var searchBox = Object.Instantiate(
                    __instance.TabButtons[2].gameObject,
                    __instance.TabButtons[2].transform.parent);
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
                _searchBoxTmp = searchBox.AddComponent<TextBoxTMP>();
                _searchBoxTmp.outputText = tmpText;
                button.OnClick.AddListener((UnityAction)(() => { _searchBoxTmp.GiveFocus(); }));
                button.OnMouseOver = new UnityEvent();
                button.OnMouseOut = new UnityEvent();
                _rolesScroller = __instance.settingsTabs[RolesTabIndex].GetComponent<Scroller>();
                _searchBoxTmp.OnChange = new Button.ButtonClickedEvent();
                _searchBoxTmp.OnChange.AddListener(
                    (UnityAction)(() =>
                    {
                        if (GetSearchText() != string.Empty)
                        {
                            _searchIconIdle.sprite = MiraAssets.SearchClearIdleSprite;
                            _searchIconHover.sprite = MiraAssets.SearchClearHoverSprite;
                        }
                        else
                        {
                            _searchIconIdle.sprite = MiraAssets.SearchIconIdleSprite;
                            _searchIconHover.sprite = MiraAssets.SearchIconHoverSprite;
                        }

                        RefreshActiveTab(true);
                    }));
                _searchBoxTmp.transform.localPosition = new Vector3(-1.438f, 0.756f, -0.2f);

                var playerButton = __instance.TabButtons[0];
                SetupTabButton(
                    playerButton,
                    MiraAssets.WikiPlayersButtonIdleSprite,
                    MiraAssets.WikiPlayersButtonOpenSprite,
                    MiraAssets.WikiPlayersButtonHoverSprite);

                var settingButton = __instance.TabButtons[1];
                SetupTabButton(
                    settingButton,
                    MiraAssets.WikiSettingsButtonIdleSprite,
                    MiraAssets.WikiSettingsButtonOpenSprite,
                    MiraAssets.WikiSettingsButtonHoverSprite);

                var rolesButton = __instance.TabButtons[2];
                SetupTabButton(
                    rolesButton,
                    MiraAssets.WikiRolesButtonIdleSprite,
                    MiraAssets.WikiRolesButtonOpenSprite,
                    MiraAssets.WikiRolesButtonHoverSprite);
                rolesButton.OnClick = new Button.ButtonClickedEvent();
                rolesButton.OnClick.AddListener((Action)(() => OpenRolesTab()));

                var modifiersButton = Object.Instantiate(
                    __instance.TabButtons[2],
                    __instance.TabButtons[2].transform.parent);
                SetupTabButton(
                    modifiersButton,
                    MiraAssets.WikiRolesButtonIdleSprite,
                    MiraAssets.WikiRolesButtonOpenSprite,
                    MiraAssets.WikiRolesButtonHoverSprite);
                modifiersButton.OnClick = new Button.ButtonClickedEvent();
                modifiersButton.OnClick.AddListener((Action)(() => OpenModifiersTab()));
                __instance.TabButtons.Add(modifiersButton);

                playerButton.transform.localPosition = new Vector3(-3.6f, 0.656f, -0.2f);
                settingButton.transform.localPosition = new Vector3(-3.6f, 0.056f, -0.2f);
                rolesButton.transform.localPosition = new Vector3(-3.6f, -0.544f, -0.2f);
                modifiersButton.transform.localPosition = new Vector3(-3.6f, -1.144f, -0.2f);
            }
            else
            {
                DisplayNormalRoleSettings(__instance, false);
                ClearSearchText();
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
        __instance.SetActiveTab(regGame ? RolesTabIndex : 0);
        return false;
    }

    private static void SetupTabButton(MatchInfoGuideTabButton button, Sprite inactive, Sprite selected, Sprite hover)
    {
        var buttonCollider = button.GetComponent<BoxCollider2D>();
        button.transform.GetChild(0).gameObject.SetActive(false);
        var settingBtnInactive = button.inactiveSprites.GetComponent<SpriteRenderer>();
        settingBtnInactive.sprite = inactive;
        settingBtnInactive.transform.GetChild(0).gameObject.SetActive(false);
        var settingBtnSelected = button.selectedSprites.GetComponent<SpriteRenderer>();
        settingBtnSelected.sprite = selected;
        settingBtnSelected.transform.GetChild(0).gameObject.SetActive(false);
        var settingBtnHighlight = button.activeSprites.GetComponent<SpriteRenderer>();
        settingBtnHighlight.sprite = hover;
        settingBtnInactive.size = settingBtnSelected.size = settingBtnHighlight.size = Vector2.one;
        settingBtnHighlight.transform.GetChild(0).gameObject.SetActive(false);
        buttonCollider.size = new Vector2(0.8f, 0.74f);
        buttonCollider.offset = Vector2.zero;
        button.transform.localScale = new Vector3(0.7f, 0.7f, 1);
    }

    private static void SetupSearchButton(PassiveButton button, string objName, Action onClick, Sprite inactive, Sprite hover, Vector3 pos, out SpriteRenderer inactiveSprite, out SpriteRenderer hoverSprite)
    {
        button.name = objName;
        inactiveSprite = button.inactiveSprites.GetComponent<SpriteRenderer>();
        inactiveSprite.sprite = inactive;
        hoverSprite = button.activeSprites.GetComponent<SpriteRenderer>();
        hoverSprite.sprite = hover;
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(onClick);

        button.transform.localPosition = pos;
        button.transform.localScale = Vector3.one;
        button.transform.GetChild(2).gameObject.SetActive(false);
    }

    public static void OpenModifiersTab()
    {
        _advancedInfoTabScroller?.gameObject.SetActive(false);
        MatchInfoGuide.Instance.SetActiveTab(ModifiersTabIndex);
        if (!_panelsReady)
        {
            return;
        }

        if (_modifiersTabDirty)
        {
            ToggleModifierVisibility();
            _modifiersTabDirty = false;
        }
    }

    public static void OpenRolesTab()
    {
        _advancedInfoTabScroller?.gameObject.SetActive(false);
        MatchInfoGuide.Instance.SetActiveTab(RolesTabIndex);
        if (!_panelsReady)
        {
            return;
        }

        if (_rolesTabDirty)
        {
            ToggleRoleVisibility();
            _rolesTabDirty = false;
        }
    }

    public sealed class DetailedPanel(MatchInfoRolePanel panel, string title, string category, string modId)
    {
        public MatchInfoRolePanel Panel { get; } = panel;
        public string Title { get; } = title;
        public string Category { get; } = category;
        public string ModId { get; } = modId;
        public int Amount { get; set; }
        public int Chance { get; set; }
        public float Likelihood { get; set; }

        public string GetSortKey(SortingGroups grouping)
        {
            return grouping switch
            {
                SortingGroups.Faction =>
                    $"{Category} ({Title}) {Likelihood.ToString("0000.0", CultureInfo.InvariantCulture)}",
                SortingGroups.AmountChance =>
                    $"{Likelihood.ToString("0000.0", CultureInfo.InvariantCulture)} {Title} ({Category})",
                _ =>
                    $"{Title} ({Category}) {Likelihood.ToString("0000.0", CultureInfo.InvariantCulture)}",
            };
        }
    }

    private static string GetSearchText()
    {
        return _searchBoxTmp && _searchBoxTmp.outputText ? _searchBoxTmp.outputText.text : string.Empty;
    }

    private static bool ShouldShowPanel(bool? forceShow, bool isDisabled, bool isHidden)
    {
        return forceShow ??
               !(isHidden || (isDisabled && _sortFilter is SortingFilter.EnabledOnly) ||
                 (!isDisabled && _sortFilter is SortingFilter.DisabledOnly));
    }

    private static float GetGridScrollBounds(int count)
    {
        return Mathf.Clamp(Mathf.Ceil(count / 2f) * 1.3f - 1.5f, 0f, 999f);
    }

    private static void RefreshActiveTab(bool scrollToTop)
    {
        if (!_panelsReady)
        {
            return;
        }

        if (IsModifiersTabActive)
        {
            ToggleModifierVisibility();
            if (scrollToTop)
            {
                _modifiersScroller.ScrollToTop();
            }

            _modifiersTabDirty = false;
            _rolesTabDirty = true;
        }
        else
        {
            ToggleRoleVisibility();
            if (scrollToTop)
            {
                _rolesScroller.ScrollToTop();
            }

            _rolesTabDirty = false;
            _modifiersTabDirty = true;
        }
    }

    public static void ClearSearchText()
    {
        _searchIconIdle.sprite = MiraAssets.SearchIconIdleSprite;
        _searchIconHover.sprite = MiraAssets.SearchIconHoverSprite;
        if (_searchBoxTmp)
        {
            _searchBoxTmp.outputText.SetText(string.Empty);
            _searchBoxTmp.SetText(string.Empty);
        }

        RefreshActiveTab(true);
    }

    public static void SwitchFilter()
    {
        var stepUp = (SortingFilter)((int)_sortFilter + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortFilter = stepUp;
        }
        else
        {
            _sortFilter = SortingFilter.EnabledOnly;
        }

        switch (_sortFilter)
        {
            case SortingFilter.AllRoles:
                _searchSortingFilterIdle.sprite = MiraAssets.VisibleAllIdleSprite;
                _searchSortingFilterHover.sprite = MiraAssets.VisibleAllHoverSprite;
                break;
            case SortingFilter.DisabledOnly:
                _searchSortingFilterIdle.sprite = MiraAssets.VisibleOffIdleSprite;
                _searchSortingFilterHover.sprite = MiraAssets.VisibleOffHoverSprite;
                break;
            default:
                _searchSortingFilterIdle.sprite = MiraAssets.VisibleOnIdleSprite;
                _searchSortingFilterHover.sprite = MiraAssets.VisibleOnHoverSprite;
                break;
        }

        RefreshActiveTab(false);
    }

    public static void SwitchMethod()
    {
        var stepUp = (SortingMethod)((int)_sortMethod + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortMethod = stepUp;
        }
        else
        {
            _sortMethod = SortingMethod.Alphabetical;
        }

        _searchSortingOrderIdle.sprite = _sortMethod is SortingMethod.Alphabetical
            ? MiraAssets.SortingAzIdleSprite
            : MiraAssets.SortingZaIdleSprite;
        _searchSortingOrderHover.sprite = _sortMethod is SortingMethod.Alphabetical
            ? MiraAssets.SortingAzHoverSprite
            : MiraAssets.SortingZaHoverSprite;

        RefreshActiveTab(false);
    }

    public static void SwitchGrouping()
    {
        var stepUp = (SortingGroups)((int)_sortGrouping + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortGrouping = stepUp;
        }
        else
        {
            _sortGrouping = SortingGroups.Ungrouped;
        }

        switch (_sortGrouping)
        {
            case SortingGroups.Faction:
                _searchSortingGroupIdle.sprite = MiraAssets.GroupFactionIdleSprite;
                _searchSortingGroupHover.sprite = MiraAssets.GroupFactionHoverSprite;
                break;
            case SortingGroups.AmountChance:
                _searchSortingGroupIdle.sprite = MiraAssets.GroupCountIdleSprite;
                _searchSortingGroupHover.sprite = MiraAssets.GroupCountHoverSprite;
                break;
            default:
                _searchSortingGroupIdle.sprite = MiraAssets.GroupNoneIdleSprite;
                _searchSortingGroupHover.sprite = MiraAssets.GroupNoneHoverSprite;
                break;
        }

        RefreshActiveTab(false);
    }

    private static void SetupPanelButton(MatchInfoRolePanel panel, Action onClick)
    {
        var collider = panel.roleIcon.gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.13f, 0.13f);
        collider.offset = new Vector2(0, 0);
        var passiveButton = panel.roleIcon.gameObject.AddComponent<PassiveButton>();
        passiveButton.ClickSound = HudManager.Instance.MapButton.ClickSound;
        passiveButton.OnMouseOver = new UnityEvent();
        passiveButton.OnMouseOver.AddListener(
            (UnityAction)(() => { panel.roleIcon.color = new Color32(255, 255, 255, 150); }));
        passiveButton.OnMouseOut = new UnityEvent();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() => { panel.roleIcon.color = Color.white; }));

        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(onClick);
    }

    public static void DisplayNormalRoleSettings(MatchInfoGuide instance, bool reset)
    {
        if (reset)
        {
            RolePanels.Clear();
            ModifierPanels.Clear();
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

            _panelsReady = false;
            if (_panelCreation != null)
            {
                Coroutines.Stop(_panelCreation);
            }

            _panelCreation = Coroutines.Start(CoCreatePanels(instance));
        }

        _advancedInfoTabScroller?.gameObject.SetActive(false);

        if (!_panelsReady)
        {
            return;
        }

        _rolesTabDirty = true;
        _modifiersTabDirty = true;
        RefreshActiveTab(false);
    }

    private static IEnumerator CoCreatePanels(MatchInfoGuide instance)
    {
        var inner = instance.settingsTabs[RolesTabIndex].GetComponent<Scroller>().Inner;
        yield return ModifierManager.Modifiers.OfType<GameModifier>().CoLoopWithBudget(modifier =>
        {
            var panel = Object.Instantiate(
                instance.MatchInfoRolePanelPrefab,
                _modifiersScroller.Inner);
            var amount =
                modifier.GetAmountPerGame();
            var chance =
                modifier.GetAssignmentChance();
            panel.SetModifierPanel(
                modifier,
                amount,
                chance);
            SetupPanelButton(panel, () => { DisplayAdvancedWiki(instance, modifier); });
            ModifierPanels.Add(
                modifier,
                new DetailedPanel(
                    panel,
                    modifier.ModifierName,
                    modifier.ModifierCategoryTitle,
                    modifier.ParentMod.MiraPlugin.GetAbbreviatedModName()));
            panel.gameObject.SetActive(false);
        });
        yield return CustomRoleManager.AllStoredRoleBehaviours.Where(roleBehaviour =>
            roleBehaviour.Role is not RoleTypes.Crewmate and not RoleTypes.Impostor and
            not RoleTypes.CrewmateGhost and
            not RoleTypes.ImpostorGhost).CoLoopWithBudget(roleBehaviour =>
        {
            var panel = Object.Instantiate(
                instance.MatchInfoRolePanelPrefab,
                inner);
            string abbreviation;
            int amount;
            int chance;
            if (roleBehaviour is ICustomRole custom)
            {
                amount = custom.GetCount().GetValueOrDefault(0);
                chance = custom.GetChance().GetValueOrDefault(0);
                abbreviation = custom.ParentMod.MiraPlugin.GetAbbreviatedModName();
            }
            else
            {
                abbreviation = "AU";
                amount =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);
                chance =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
            }

            panel.SetRolePanel(
                roleBehaviour,
                amount,
                chance);
            SetupPanelButton(panel, () => { DisplayAdvancedWiki(instance, roleBehaviour); });
            RolePanels.Add(
                roleBehaviour,
                new DetailedPanel(
                    panel,
                    roleBehaviour.GetRoleName(),
                    roleBehaviour.GetCategoryTitle(),
                    abbreviation));
            panel.gameObject.SetActive(false);
        });

        _panelsReady = true;
        _panelCreation = null;
        RefreshActiveTab(false);
        instance.CreatePlayerEntries();
    }


    public static void ToggleModifierVisibility()
    {
        var num = 0;
        foreach (var (modifier, holder) in ModifierPanels)
        {
            var panel = holder.Panel;
            var amount =
                modifier.GetAmountPerGame();
            var chance =
                modifier.GetAssignmentChance();
            SetPanelCount(panel, amount, chance, holder.ModId);
            holder.Amount = amount;
            holder.Chance = chance;
            holder.Likelihood = amount * chance;
            var isRoleDisabled = amount == 0 || chance == 0;
            var isRoleNotVisible = !modifier.CanSpawnOnCurrentMode() || modifier.GetDescription() == string.Empty;
            if (!ShouldShowPanel(modifier.ForceShowModifierOnWiki, isRoleDisabled, isRoleNotVisible))
            {
                panel.gameObject.SetActive(false);
                continue;
            }

            panel.gameObject.SetActive(true);
            num++;
        }

        SortAllModifiers(GetSearchText());

        /*var instance = MatchInfoGuide.Instance;
        instance.rolesEnabledMessage.SetActive(num == 0);*/

        _modifiersScroller.SetYBoundsMax(GetGridScrollBounds(num));
    }

    public static void ToggleRoleVisibility()
    {
        var num = 0;
        foreach (var (roleData, holder) in RolePanels)
        {
            var panel = holder.Panel;
            int amount;
            int chance;
            bool? forceShow = null;
            if (roleData is ICustomRole custom)
            {
                amount = custom.GetCount().GetValueOrDefault(0);
                chance = custom.GetChance().GetValueOrDefault(0);
                forceShow = custom.ForceShowRoleOnWiki;
            }
            else
            {
                amount =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleData.Role);
                chance =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleData.Role);
            }

            SetPanelCount(panel, amount, chance, holder.ModId);
            holder.Amount = amount;
            holder.Chance = chance;
            holder.Likelihood = amount * chance;
            var isRoleDisabled = amount == 0 || chance == 0;
            var isRoleNotVisible = (roleData is ICustomRole custom2 &&
                                    !custom2.CanSpawnOnCurrentMode()) ||
                                   (Enum.IsDefined(roleData.Role) && roleData.IsRoleBlacklisted());
            if (!ShouldShowPanel(forceShow, isRoleDisabled, isRoleNotVisible))
            {
                panel.gameObject.SetActive(false);
                continue;
            }

            panel.gameObject.SetActive(true);
            num++;
        }

        SortAllRoles(GetSearchText());

        var instance = MatchInfoGuide.Instance;
        instance.rolesEnabledMessage.SetActive(num == 0);

        instance.MatchInfoRoleScroller.SetYBoundsMax(GetGridScrollBounds(num));
    }

    public static void SortAllRoles(string searchText)
    {
        SortPanels(RolePanels, searchText);
    }

    public static void SortAllModifiers(string searchText)
    {
        SortPanels(ModifierPanels, searchText);
    }

    private static void SortPanels<T>(Dictionary<T, DetailedPanel> panels, string searchText)
        where T : notnull
    {
        var sorted = panels.Values
            .OrderByDescending(p => p.Title.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p =>
                p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        var ordered = _sortMethod is SortingMethod.Alphabetical
            ? sorted.ThenBy(p => p.GetSortKey(_sortGrouping))
            : sorted.ThenByDescending(p => p.GetSortKey(_sortGrouping));

        foreach (var panel in ordered)
        {
            panel.Panel.transform.SetAsLastSibling();
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

            if (namePlate != null)
            {
                __instance.StartCoroutine(
                    __instance.CoLoadAssetAsync<NamePlateViewData>(
                        namePlate.GetAssetReference(),
                        (Action<NamePlateViewData>?)LoadNameplate));
            }

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
    private static void SetActiveTab(int tabIndex)
    {
        if (_titleText)
        {
            switch (tabIndex)
            {
                case 0:
                    _titleText.text = MiraLocaleManager.Get("MiraApi.Wiki.PlayersTab");
                    break;
                case 1:
                    _titleText.text = MiraLocaleManager.Get("MiraApi.Wiki.SettingsTab");
                    break;
                case RolesTabIndex:
                    _titleText.text = MiraLocaleManager.Get("Roles");
                    break;
                case ModifiersTabIndex:
                    _titleText.text = MiraLocaleManager.Get("Modifiers");
                    break;
                default:
                    _titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
                    break;
            }
        }

        _advancedInfoTabScroller?.gameObject.SetActive(false);
    }

    private static bool TryPrepareAdvancedWiki(MatchInfoGuide instance)
    {
        instance.settingsTabs[RolesTabIndex].SetActive(false);
        _modifiersScroller?.gameObject.SetActive(false);
        if (_currentAdvancedTabObject)
        {
            _currentAdvancedTabObject.SetActive(false);
            _currentAdvancedTabObject.Destroy();
        }

        if (_advancedInfoTabScroller == null)
        {
            Warning("Wiki tab is null.");
            return false;
        }

        _advancedInfoTabScroller.gameObject.SetActive(true);
        return true;
    }

    public static void DisplayAdvancedWiki(MatchInfoGuide instance, BaseModifier modifier)
    {
        var name = modifier.ModifierName;
        Info($"Opening advanced tab for {name}.");
        if (!TryPrepareAdvancedWiki(instance))
        {
            return;
        }

        _currentAdvancedTabObject = modifier.GetAdvancedWiki(instance, _titleText, _advancedInfoTabScroller);
    }

    public static void DisplayAdvancedWiki(MatchInfoGuide instance, RoleBehaviour role)
    {
        Info($"Opening advanced tab for {role.GetRoleName()}.");
        if (!TryPrepareAdvancedWiki(instance))
        {
            return;
        }

        if (role is ICustomRole custom)
        {
            _currentAdvancedTabObject = custom.GetAdvancedWiki(instance, _titleText, _advancedInfoTabScroller);
        }
        else
        {
            var titleTxt = role.GetRoleName() +
                           $" ({TranslationController.Instance.GetString(role.TeamType is RoleTeamTypes.Crewmate ? StringNames.Crewmate : StringNames.Impostor)})";
            var description = TranslationController.Instance.GetString(role.BlurbNameLong);
            if (description.Contains("STRMISS"))
            {
                var baseName = $"{role.StringName}".Replace("Role", string.Empty);
                if (Enum.TryParse<StringNames>($"RolesHelp_{baseName}_01", out var helpName))
                {
                    description = TranslationController.Instance.GetString(helpName);
                }
            }

            _currentAdvancedTabObject = Helpers.CreateAdvancedWikiPage(
                instance,
                _titleText,
                _advancedInfoTabScroller,
                role.Role.ToString(),
                titleTxt,
                description,
                []);
        }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoRolePanel), nameof(MatchInfoRolePanel.SetPanel))]
    public static bool SetPanel(MatchInfoRolePanel __instance, RoleBehaviour role, int numPerGame, int chancePerGame)
    {
        __instance.roleCount.text = string.Format(
            CultureInfo.InvariantCulture,
            "{0} at {1}%",
            numPerGame.ToString(CultureInfo.InvariantCulture),
            chancePerGame);
        if (role is ICustomRole customRole)
        {
            __instance.roleName.text = customRole.RoleName;
            __instance.roleDescription.text =
                $"<size=60%>{role.GetCategoryTitle()}</size>\n" + customRole.RoleMedDescription;
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

    private static void SetPanelCount(MatchInfoRolePanel panel, int amount, int chance, string modId)
    {
        panel.roleCount.text = string.Format(
            CultureInfo.InvariantCulture,
            "{0} at {1}% ({2})",
            amount.ToString(CultureInfo.InvariantCulture),
            chance,
            modId);
    }

    public static void SetRolePanel(
        this MatchInfoRolePanel instance,
        RoleBehaviour role,
        int numPerGame,
        int chancePerGame)
    {
        if (role is ICustomRole customRole)
        {
            instance.roleName.text = customRole.RoleName;
            instance.roleDescription.text =
                $"<size=60%>{role.GetCategoryTitle()}</size>\n" + customRole.RoleMedDescription;
            instance.roleIcon.sprite = customRole.Configuration.Icon?.LoadAsset();
            SetPanelCount(instance, numPerGame, chancePerGame, customRole.ParentMod.MiraPlugin.GetAbbreviatedModName());
        }
        else
        {
            instance.roleName.text = role.NiceName;
            instance.roleDescription.text = $"<size=60%>{role.GetCategoryTitle()}</size>\n" + role.BlurbMed;
            instance.roleIcon.sprite = role.RoleIconColor;
            SetPanelCount(instance, numPerGame, chancePerGame, "AU");
        }

        instance.roleIcon.SetSizeLimit(0.13f);
        instance.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
        instance.roleName.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleDescription.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleCount.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleIcon.transform.localScale = new Vector3(4f, 4f, 1f);
    }

    public static void SetModifierPanel(
        this MatchInfoRolePanel instance,
        GameModifier modifier,
        int numPerGame,
        int chancePerGame)
    {
        instance.roleName.text = modifier.ModifierName;
        instance.roleDescription.text =
            $"<size=60%>{modifier.ModifierCategoryTitle}</size>\n" + modifier.ModifierMedDescription;
        instance.roleIcon.sprite = modifier.ModifierIcon?.LoadAsset();
        SetPanelCount(instance, numPerGame, chancePerGame, modifier.ParentMod.MiraPlugin.GetAbbreviatedModName());

        instance.roleIcon.SetSizeLimit(0.13f);
        instance.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
        instance.roleName.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleDescription.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleCount.fontMaterial.SetFloat(instance.STENCIL_NAME, 50f);
        instance.roleIcon.transform.localScale = new Vector3(4f, 4f, 1f);
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

public enum SortingGroups
{
    Ungrouped,
    Faction,
    AmountChance,
}

public enum SortingMethod
{
    Alphabetical,
    AlphabeticalDescending,
}

public enum SortingFilter
{
    EnabledOnly,
    DisabledOnly,
    AllRoles,
}
