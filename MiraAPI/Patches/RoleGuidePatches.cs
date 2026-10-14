using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Innersloth.Assets;
using MiraAPI.LocalSettings;
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
    private const int GeneralTabIndex = 1;
    private const int RolesTabIndex = 2;
    private const int ModifiersTabIndex = 3;
    private const float MinGuideScaleFactor = 0.5f;
    private const float MaxGuideScaleFactor = 1f;
    private const float SearchDebounceSeconds = 0.05f;

    private static readonly List<DetailedPanel> RoleEntries = [];
    private static readonly List<DetailedPanel> ModifierEntries = [];

    public static GameObject SearchComponentContainer;
    public static PassiveButton SearchIconButton;
    private static SpriteRenderer _searchIconIdle;
    private static SpriteRenderer _searchIconHover;
    public static TextMeshPro OrderSortingTmp;
    public static PassiveButton SearchSortingOrderButton;
    private static SpriteRenderer _searchSortingOrderIdle;
    private static SpriteRenderer _searchSortingOrderHover;
    public static TextMeshPro GroupSortingTmp;
    public static PassiveButton SearchSortingGroupButton;
    private static SpriteRenderer _searchSortingGroupIdle;
    private static SpriteRenderer _searchSortingGroupHover;
    public static TextMeshPro FilterSortingTmp;
    public static PassiveButton SearchSortingFilterButton;
    private static SpriteRenderer _searchSortingFilterIdle;
    private static SpriteRenderer _searchSortingFilterHover;
    private static Scroller _rolesScroller;
    private static Scroller _modifiersScroller;
    private static GameObject _modifierNoResultsMessage;
    private static Scroller _advancedInfoTabScroller;
    private static TextBoxTMP _searchBoxTmp;
    private static GameObject _currentAdvancedTabObject;
    private static TextMeshPro _titleText;
    private static SpriteRenderer _titleIcon;
    private static bool _suppressSearchRefresh;
    private static IEnumerator? _searchDebounce;
    private static string _appliedSearch = string.Empty;
    private static float _openedAt;
    private static bool _uiInitialized;
    private static MatchInfoGuide? _guide;
    private static IEnumerator? _populate;
    private static bool _loading;
    private static LoadingRing? _loadingSpinner;
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
        __instance.transitionOpen.targetSize = Helpers.MatchInfoGuideBaseScale;
        if (_guide != __instance)
        {
            ResetState();
        }
    }

    public static void ApplyGuideScale(MatchInfoGuide instance, float scaleFactor)
    {
        if (!instance || !instance.transitionOpen)
        {
            return;
        }

        var targetSize = Helpers.MatchInfoGuideBaseScale *
                         Mathf.Clamp(scaleFactor, MinGuideScaleFactor, MaxGuideScaleFactor);
        instance.transitionOpen.targetSize = targetSize;
        if (instance.MatchInfoParent.activeSelf)
        {
            instance.transitionOpen.transform.localScale = Vector3.one * targetSize;
        }
    }

    /// <summary>
    /// Clears all cached state tied to a previous <see cref="MatchInfoGuide"/> instance.
    /// Every game spawns a fresh guide, so anything we cached from the old one is a destroyed Unity object.
    /// </summary>
    private static void ResetState()
    {
        if (_populate != null)
        {
            Coroutines.Stop(_populate);
            _populate = null;
        }

        if (_searchDebounce != null)
        {
            Coroutines.Stop(_searchDebounce);
            _searchDebounce = null;
        }

        RoleEntries.Clear();
        ModifierEntries.Clear();
        _guide = null;
        _uiInitialized = false;
        _loading = false;
        _appliedSearch = string.Empty;
        _suppressSearchRefresh = false;
        _titleText = null!;
        _titleIcon = null!;
        _searchBoxTmp = null!;
        _rolesScroller = null!;
        _modifiersScroller = null!;
        _modifierNoResultsMessage = null!;
        _advancedInfoTabScroller = null!;
        _currentAdvancedTabObject = null!;
        _loadingSpinner = null;
        ResetGeneralState();
        SearchIconButton = null!;
        SearchSortingOrderButton = null!;
        SearchSortingGroupButton = null!;
        SearchSortingFilterButton = null!;
        _searchIconIdle = null!;
        _searchIconHover = null!;
        _searchSortingOrderIdle = null!;
        _searchSortingOrderHover = null!;
        _searchSortingGroupIdle = null!;
        _searchSortingGroupHover = null!;
        _searchSortingFilterIdle = null!;
        _searchSortingFilterHover = null!;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Awake))]
    private static void AwakePostfix(MatchInfoGuide __instance)
    {
        ApplyGuideScale(__instance, LocalSettingsTabSingleton<MiraApiSettings>.Instance.MatchInfoGuideScaleFactor.Value);
        Coroutines.Start(CoPreload(__instance));
    }

    public static void TriggerPreload(MatchInfoGuide instance)
    {
        if (!instance || _populate != null)
        {
            return;
        }

        Coroutines.Start(CoPreload(instance));
    }

    private static IEnumerator CoPreload(MatchInfoGuide instance)
    {
        while (instance && (!HudManager.InstanceExists || !GameManager.Instance))
        {
            yield return null;
        }

        if (!instance || GameManager.Instance.TryCast<NormalGameManager>() == null)
        {
            yield break;
        }

        _guide = instance;
        if (RoleEntries.Count == 0)
        {
            BuildEntries();
        }

        if (_populate != null)
        {
            yield break;
        }

        _populate = Coroutines.Start(CoPreloadPanels());
    }

    private static IEnumerator CoPreloadPanels()
    {
        yield return CoForEachBudgeted(RoleEntries, PreloadPanel);
        yield return CoForEachBudgeted(ModifierEntries, PreloadPanel);
        _populate = null;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Open))]
    public static bool Open(MatchInfoGuide __instance)
    {
        if (!HudManager.InstanceExists || !GameManager.Instance || !PlayerControl.LocalPlayer)
        {
            return false;
        }

        if (HudManager.Instance.GameMenu.IsOpen || HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return false;
        }

        ApplyGuideScale(__instance, LocalSettingsTabSingleton<MiraApiSettings>.Instance.MatchInfoGuideScaleFactor.Value);

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
                _titleText.transform.localPosition += new Vector3(0.5f, 0);
                _titleText.transform.GetComponent<TextTranslatorTMP>().Destroy();
            }
        }
        if (!_titleIcon)
        {
            var titleIconObj = new GameObject("TitleIcon");
            _titleIcon = titleIconObj.AddComponent<SpriteRenderer>();
            _titleIcon.transform.SetParent(_titleText.transform.parent);
            _titleIcon.transform.localPosition = new Vector3(-2.75f, 1.4f, 0);
            titleIconObj.layer = _titleText.gameObject.layer;
        }

        if (_titleText)
        {
            _titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
        }

        var regGame = GameManager.Instance.TryCast<NormalGameManager>() != null;
        if (regGame)
        {
            if (!_uiInitialized)
            {
                Initialize(__instance);
            }

            if (RoleEntries.Count == 0)
            {
                BuildEntries();
            }

            if (__instance.NormalModeSettings.Count == 0)
            {
                __instance.CreateSettingsEntry(
                    StringNames.GameNumImpostors,
                    GameManager.Instance.AllGameSettingData[StringNames.GameNumImpostors]
                        .GetValueString(GameManager.Instance.LogicOptions.NumImpostors));
                __instance.CreateSettingsEntry(
                    StringNames.GameKillCooldown,
                    GameManager.Instance.AllGameSettingData[StringNames.GameKillCooldown]
                        .GetValueString(GameManager.Instance.LogicOptions.GetKillCooldown()));
                __instance.CreateSettingsEntry(
                    StringNames.GameEmergencyCooldown,
                    GameManager.Instance.AllGameSettingData[StringNames.GameEmergencyCooldown]
                        .GetValueString(GameManager.Instance.LogicOptions.GetEmergencyCooldown()));
                __instance.CreateSettingsEntry(
                    StringNames.GameVisualTasks,
                    __instance.GetBoolString(GameManager.Instance.LogicOptions.GetVisualTasks()));
                __instance.CreateSettingsEntry(
                    StringNames.GameAnonymousVotes,
                    __instance.GetBoolString(GameManager.Instance.LogicOptions.GetAnonymousVotes()));
                __instance.CreateSettingsEntry(
                    StringNames.GameConfirmImpostor,
                    __instance.GetBoolString(GameManager.Instance.LogicOptions.GetConfirmImpostor()));
                __instance.CreateSettingsEntry(
                    StringNames.GameTaskBarMode,
                    GameManager.Instance.LogicOptions.GetTaskBarMode().ToString());
                __instance.CreatePlayerEntries();
                __instance.rolesEnabledMessage.SetActive(false);
            }

            ResetSearchBox();
        }
        else if (__instance.HnSModeSettings.Count == 0)
        {
            __instance.numOfTabs = 2;
            __instance.TabButtons[0].SelectButton(true);
            __instance.CreateHnSModeSettings();
        }

        PlayerControl.LocalPlayer.NetTransform.Halt();
        HidePanels(RoleEntries);
        HidePanels(ModifierEntries);
        __instance.MatchInfoParent.SetActive(true);
        _openedAt = Time.time;
        if (__instance.ControllerSelectable.Count > 0)
        {
            var controllerManager = ControllerManager.Instance;
            controllerManager.SetUpSelectables(
                controllerManager.CurrentUiState,
                __instance.ControllerSelectable[^1],
                __instance.ControllerSelectable);
            controllerManager.SetCurrentSelected(__instance.ControllerSelectable[^1]);
        }

        __instance.SetActiveTab(regGame ? RolesTabIndex : 0);
        if (regGame)
        {
            OpenTab(RolesTabIndex, __instance.transitionOpen.duration);
        }

        return false;
    }

    private static void Initialize(MatchInfoGuide instance)
    {
        if (_uiInitialized)
        {
            return;
        }

        _sortFilter = MiraApiPlugin.WikiSortingFilter.Value;
        _sortMethod = MiraApiPlugin.WikiSortingMethod.Value;
        _sortGrouping = MiraApiPlugin.WikiSortingGroup.Value;

        SetNoResultsText(instance.rolesEnabledMessage);

        if (_populate != null)
        {
            Coroutines.Stop(_populate);
            _populate = null;
        }

        _guide = instance;
        _loading = false;
        SearchComponentContainer = new GameObject();
        SearchComponentContainer.transform.SetParent(instance.MatchInfoParent.transform);
        SearchComponentContainer.transform.localPosition = Vector3.zero;
        instance.MatchInfoParent.transform.GetChild(1).transform.SetParent(SearchComponentContainer.transform);

        var sortingOrderButton = Object.Instantiate(
            HudManager.Instance.SettingsButton,
            SearchComponentContainer.transform);
        sortingOrderButton.gameObject.SetActive(true);
        SearchSortingOrderButton = sortingOrderButton.GetComponent<PassiveButton>();
        SetupSearchButton(
            SearchSortingOrderButton,
            "SearchSortingOrderButton",
            SwitchMethod,
            GetSortMethodSprite(_sortMethod, false),
            GetSortMethodSprite(_sortMethod, true),
            new Vector3(2.4f, 0.765f, -0.1f),
            out _searchSortingOrderIdle,
            out _searchSortingOrderHover);

        var sortingGroupButton = Object.Instantiate(
            sortingOrderButton,
            SearchComponentContainer.transform);
        sortingGroupButton.gameObject.SetActive(true);
        SearchSortingGroupButton = sortingGroupButton.GetComponent<PassiveButton>();
        SetupSearchButton(
            SearchSortingGroupButton,
            "SearchSortingGroupButton",
            SwitchGrouping,
            GetSortGroupSprite(_sortGrouping, false),
            GetSortGroupSprite(_sortGrouping, true),
            new Vector3(1.7f, 0.765f, -0.1f),
            out _searchSortingGroupIdle,
            out _searchSortingGroupHover);

        var sortingFilterButton = Object.Instantiate(
            sortingOrderButton,
            SearchComponentContainer.transform);
        sortingFilterButton.gameObject.SetActive(true);
        SearchSortingFilterButton = sortingFilterButton.GetComponent<PassiveButton>();
        SetupSearchButton(
            SearchSortingFilterButton,
            "SearchSortingFilterButton",
            SwitchFilter,
            GetSortFilterSprite(_sortFilter, false),
            GetSortFilterSprite(_sortFilter, true),
            new Vector3(1f, 0.765f, -0.1f),
            out _searchSortingFilterIdle,
            out _searchSortingFilterHover);

        instance.numOfTabs = 4;
        instance.TabButtons[2].SelectButton(true);
        ApplyPanelLayout(instance);
        instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
        instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);

        var modifiersTab = Object.Instantiate(
            instance.settingsTabs[RolesTabIndex],
            instance.settingsTabs[RolesTabIndex].transform.parent);
        modifiersTab.name = "ModifiersPanel";
        _modifierNoResultsMessage = Object.Instantiate(
            instance.rolesEnabledMessage,
            instance.rolesEnabledMessage.transform.parent,
            true);
        _modifierNoResultsMessage.name = "NoResultsFound";
        _modifierNoResultsMessage.SetActive(false);
        SetNoResultsText(_modifierNoResultsMessage);
        modifiersTab.transform.FindChild("MaskArea")?.transform.localPosition =
            new Vector3(-0.0184f, 0.15f, -0.1f);
        modifiersTab.transform.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
        _modifiersScroller = modifiersTab.GetComponent<Scroller>();
        instance.settingsTabs.Add(modifiersTab);
        InitializeGeneralPanel(instance);

        var wikiTab = Object.Instantiate(
            instance.settingsTabs[RolesTabIndex],
            instance.settingsTabs[RolesTabIndex].transform.parent);
        wikiTab.name = "AdvancedWikiPanels";
        var wikiMask = wikiTab.transform.FindChild("MaskArea");
        wikiMask.transform.localPosition = new Vector3(-0.0184f, 0.425f, -0.1f);
        wikiMask.transform.localScale = new Vector3(1, 1.3f, 1);
        wikiTab.transform.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
        _advancedInfoTabScroller = wikiTab.GetComponent<Scroller>();
        var wikiScrollHitbox = wikiTab.transform.FindChild("Hitbox");
        _advancedInfoTabScroller.ScrollbarYBounds.max = 1.2f;
        wikiScrollHitbox.GetComponent<BoxCollider2D>().size = new Vector2(5.75f, 2.4f);
        wikiScrollHitbox.transform.localPosition += new Vector3(0, 0.3f);
        _advancedInfoTabScroller.ScrollbarY.trackGraphic.transform.localPosition = new Vector3(2.8364f, 0.3649f, -0.2f);
        _advancedInfoTabScroller.ScrollbarY.trackGraphic.transform.localScale = new Vector3(0.0234f, 2.068f, 0.5598f);

        var spinnerParent = instance.settingsTabs[RolesTabIndex].transform.parent;
        var spinnerPos = spinnerParent.InverseTransformPoint(instance.MatchInfoRoleMaskArea.transform.position);
        spinnerPos.z = -5f;
        _loadingSpinner = LoadingRing.Create(spinnerParent, spinnerPos, 0.6f);
        _loadingSpinner.GetComponent<SpriteRenderer>().color = MiraAssets.AcceptedTeal;

        var searchIconButton = Object.Instantiate(
            sortingOrderButton,
            sortingOrderButton.transform.parent);
        searchIconButton.gameObject.SetActive(true);
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
            instance.TabButtons[2].gameObject,
            SearchComponentContainer.transform);
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
        var template = HudManager.Instance.Chat.freeChatField.textArea;
        _searchBoxTmp = Object.Instantiate(template, searchBox.transform);
        _searchBoxTmp.gameObject.SetActive(true);
        _searchBoxTmp.name = "SearchField";
        _searchBoxTmp.transform.localPosition = Vector3.zero;
        _searchBoxTmp.OnEnter = new Button.ButtonClickedEvent();
        _searchBoxTmp.OnChange = new Button.ButtonClickedEvent();
        _searchBoxTmp.OnFocus = new Button.ButtonClickedEvent();
        _searchBoxTmp.OnFocusLost = new Button.ButtonClickedEvent();
        _searchBoxTmp.sendButtonGlyph = null;
        _searchBoxTmp.SendOnFullChars = false;
        _searchBoxTmp.ClearOnFocus = false;
        _searchBoxTmp.characterLimit = 40;
        _searchBoxTmp.AllowSymbols = true;
        _searchBoxTmp.allowAllCharacters = true;
        if (_searchBoxTmp.Background)
        {
            _searchBoxTmp.Background.enabled = false;
        }

        var boxCollider = _searchBoxTmp.GetComponent<BoxCollider2D>();
        if (boxCollider)
        {
            boxCollider.size = new Vector2(3f, 0.6f);
            boxCollider.offset = Vector2.zero;
        }

        var outputText = _searchBoxTmp.outputText;
        var textParent = tmpText.transform.parent;
        outputText.transform.SetParent(textParent, false);
        outputText.GetComponent<TextTranslatorTMP>()?.Destroy();
        CopyTextLayout(tmpText, outputText);
        outputText.color = new Color(0.75f, 0.75f, 0.75f);
        outputText.text = string.Empty;

        var placeholderText = _searchBoxTmp.placeholderText;
        if (placeholderText)
        {
            placeholderText.transform.SetParent(textParent, false);
            placeholderText.GetComponent<TextTranslatorTMP>()?.Destroy();
            CopyTextLayout(tmpText, placeholderText);
            placeholderText.color = new Color(0.55f, 0.55f, 0.55f);
            placeholderText.text = MiraLocaleManager.Get("MiraApi.Wiki.SearchPlaceholder");
            placeholderText.gameObject.SetActive(true);
        }

        if (_searchBoxTmp.Pipe)
        {
            _searchBoxTmp.Pipe.transform.SetParent(textParent, false);
            _searchBoxTmp.Pipe.transform.localPosition = new Vector3(
                outputText.transform.localPosition.x -
                (outputText.rectTransform.sizeDelta.x * outputText.rectTransform.pivot.x),
                outputText.transform.localPosition.y + _searchBoxTmp.caretYOffset,
                -0.3f);
        }

        tmpText.gameObject.Destroy();

        button.OnClick.AddListener((UnityAction)(() => { _searchBoxTmp.GiveFocus(); }));
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        _rolesScroller = instance.settingsTabs[RolesTabIndex].GetComponent<Scroller>();
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

                if (_suppressSearchRefresh)
                {
                    return;
                }

                QueueSearchRefresh();
            }));
        searchBox.transform.localPosition = new Vector3(-1.438f, 0.756f, -0.2f);

        var playerButton = instance.TabButtons[0];
        SetupTabButton(
            playerButton,
            MiraAssets.WikiPlayersButtonIdleSprite,
            MiraAssets.WikiPlayersButtonOpenSprite,
            MiraAssets.WikiPlayersButtonHoverSprite);

        var settingButton = instance.TabButtons[1];
        SetupTabButton(
            settingButton,
            MiraAssets.WikiSettingsButtonIdleSprite,
            MiraAssets.WikiSettingsButtonOpenSprite,
            MiraAssets.WikiSettingsButtonHoverSprite);

        var rolesButton = instance.TabButtons[2];
        SetupTabButton(
            rolesButton,
            MiraAssets.WikiRolesButtonIdleSprite,
            MiraAssets.WikiRolesButtonOpenSprite,
            MiraAssets.WikiRolesButtonHoverSprite);
        rolesButton.OnClick = new Button.ButtonClickedEvent();
        rolesButton.OnClick.AddListener((Action)(() => OpenRolesTab()));

        var modifiersButton = Object.Instantiate(
            instance.TabButtons[2],
            instance.TabButtons[2].transform.parent);
        SetupTabButton(
            modifiersButton,
            MiraAssets.WikiRolesButtonIdleSprite,
            MiraAssets.WikiRolesButtonOpenSprite,
            MiraAssets.WikiRolesButtonHoverSprite);
        modifiersButton.OnClick = new Button.ButtonClickedEvent();
        modifiersButton.OnClick.AddListener((Action)(() => OpenModifiersTab()));
        instance.TabButtons.Add(modifiersButton);

        OrderSortingTmp = Object.Instantiate(
            _titleText,
            sortingOrderButton.transform);
        OrderSortingTmp.text = MiraLocaleManager.Get("MiraApi.Wiki.OrderLabel");
        OrderSortingTmp.fontSize = OrderSortingTmp.fontSizeMax = 3f;
        OrderSortingTmp.alignment = TextAlignmentOptions.Center;
        OrderSortingTmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        OrderSortingTmp.transform.localPosition = new Vector3(-0.65f, 0.4f, 0);

        GroupSortingTmp = Object.Instantiate(
            OrderSortingTmp,
            sortingGroupButton.transform);
        OrderSortingTmp.text = MiraLocaleManager.Get("MiraApi.Wiki.GroupingLabel");
        GroupSortingTmp.transform.localPosition = new Vector3(-0.65f, 0.4f, 0);

        FilterSortingTmp = Object.Instantiate(
            OrderSortingTmp,
            sortingFilterButton.transform);
        FilterSortingTmp.text = MiraLocaleManager.Get("MiraApi.Wiki.FilterLabel");
        FilterSortingTmp.transform.localPosition = new Vector3(-0.65f, 0.4f, 0);

        playerButton.transform.localPosition = new Vector3(-3.6f, 0.656f, -0.2f);
        settingButton.transform.localPosition = new Vector3(-3.6f, 0.056f, -0.2f);
        rolesButton.transform.localPosition = new Vector3(-3.6f, -0.544f, -0.2f);
        modifiersButton.transform.localPosition = new Vector3(-3.6f, -1.144f, -0.2f);
        _uiInitialized = true;
    }

    private static void BuildEntries()
    {
        foreach (var modifier in ModifierManager.Modifiers.OfType<GameModifier>())
        {
            ModifierEntries.Add(new DetailedPanel(
                modifier.ModifierName,
                modifier.ModifierCategoryTitle,
                modifier.ParentMod.MiraPlugin.GetAbbreviatedModName(),
                null,
                modifier));
        }

        foreach (var roleBehaviour in CustomRoleManager.AllStoredRoleBehaviours.Where(x =>
                     x.Role is not RoleTypes.Crewmate and not RoleTypes.Impostor and
                     not RoleTypes.CrewmateGhost and not RoleTypes.ImpostorGhost))
        {
            var modId = roleBehaviour is ICustomRole custom
                ? custom.ParentMod.MiraPlugin.GetAbbreviatedModName()
                : "AU";
            RoleEntries.Add(new DetailedPanel(
                roleBehaviour.GetRoleName(),
                roleBehaviour.GetCategoryTitle(),
                modId,
                roleBehaviour,
                null));
        }
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
        settingBtnInactive.drawMode = settingBtnSelected.drawMode = settingBtnHighlight.drawMode = SpriteDrawMode.Sliced;
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

    private static void OpenTab(int tabIndex, float delay = 0f)
    {
        _advancedInfoTabScroller?.gameObject.SetActive(false);
        MatchInfoGuide.Instance.SetActiveTab(tabIndex);
        HidePanels(tabIndex == ModifiersTabIndex ? ModifierEntries : RoleEntries);
        StartPopulate(delay);
    }

    private static void HidePanels(List<DetailedPanel> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Panel && entry.Panel!.gameObject.activeSelf)
            {
                entry.Panel.gameObject.SetActive(false);
            }
        }
    }

    public static void OpenModifiersTab() => OpenTab(ModifiersTabIndex);

    public static void OpenRolesTab() => OpenTab(RolesTabIndex);

    private static void SetNoResultsText(GameObject message)
    {
        var text = message.GetComponentInChildren<TextMeshPro>(true);
        if (!text)
        {
            return;
        }

        text.GetComponent<TextTranslatorTMP>()?.Destroy();
        text.text = MiraLocaleManager.Get("MiraApi.Wiki.NoResultsFound", "No Results Found");
    }

    private static void SetNoResultsVisible(GameObject message, bool visible)
    {
        if (message)
        {
            message.SetActive(visible);
        }
    }

    public sealed class DetailedPanel(string title, string category, string modId, RoleBehaviour? role, GameModifier? modifier)
    {
        public MatchInfoRolePanel? Panel { get; set; }
        public string Title { get; } = title;
        public string Category { get; } = category;
        public string ModId { get; } = modId;
        public RoleBehaviour? Role { get; } = role;
        public GameModifier? Modifier { get; } = modifier;
        public int Amount { get; set; }
        public int Chance { get; set; }
        public float Likelihood { get; set; }
        public bool CanShow { get; set; }
        public bool CountsReady { get; set; }
        public bool Visible { get; set; }

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

    private static void UpdateSpinner()
    {
        if (_loadingSpinner != null)
        {
            _loadingSpinner.gameObject.SetActive(
                _loading && MatchInfoGuide.Instance.activeTabIndex is RolesTabIndex or ModifiersTabIndex &&
                !(_advancedInfoTabScroller && _advancedInfoTabScroller.gameObject.activeSelf));
        }
    }

    private static float GetGridScrollBounds(int count)
    {
        return Mathf.Clamp(Mathf.Ceil(count / 2f) * 1.3f - 1.5f, 0f, 999f);
    }

    private static void StartPopulate(float delay = 0f)
    {
        if (_populate != null)
        {
            Coroutines.Stop(_populate);
        }

        _loading = true;
        UpdateSpinner();
        _populate = Coroutines.Start(CoPopulate(delay));
    }

    private static IEnumerator CoPopulate(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null;
        }

        if (!_guide)
        {
            yield break;
        }

        var modifiers = IsModifiersTabActive;
        var entries = modifiers ? ModifierEntries : RoleEntries;
        var shown = UpdateEntries(entries, modifiers);
        yield return CoForEachBudgeted(shown, entry =>
        {
            EnsurePanel(entry);
            entry.Panel!.transform.SetAsLastSibling();
            if (!entry.Panel.gameObject.activeSelf)
            {
                entry.Panel.gameObject.SetActive(true);
            }
        });
        _loading = false;
        UpdateSpinner();
        yield return CoForEachBudgeted(entries, EnsurePanel);
        yield return CoForEachBudgeted(modifiers ? RoleEntries : ModifierEntries, PreloadPanel);
        _populate = null;
    }

    private static IEnumerator CoForEachBudgeted<T>(IEnumerable<T> collection, Action<T> action)
    {
        var fps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
        var budget = Math.Max(1L, 1000L / (fps * 4));
        var timer = new Stopwatch();
        timer.Start();
        foreach (var item in collection)
        {
            action(item);
            if (timer.ElapsedMilliseconds > budget)
            {
                timer.Restart();
                yield return null;
                if (!_guide)
                {
                    yield break;
                }
            }
        }
    }

    private static void EnsurePanel(DetailedPanel entry)
    {
        if (entry.Panel)
        {
            return;
        }

        if (!_guide || !_guide!.MatchInfoRolePanelPrefab)
        {
            return;
        }

        var parent = entry.Modifier != null ? _modifiersScroller?.Inner : _rolesScroller?.Inner;
        if (parent == null)
        {
            return;
        }

        var panel = Object.Instantiate(_guide.MatchInfoRolePanelPrefab, parent);
        if (entry.Modifier != null)
        {
            panel.SetModifierPanel(entry.Modifier, entry.Amount, entry.Chance);
            SetupPanelButton(panel, () => DisplayAdvancedWiki(MatchInfoGuide.Instance, entry.Modifier));
        }
        else
        {
            panel.SetRolePanel(entry.Role!, entry.Amount, entry.Chance);
            SetupPanelButton(panel, () => DisplayAdvancedWiki(MatchInfoGuide.Instance, entry.Role!));
        }

        panel.gameObject.SetActive(false);
        entry.Panel = panel;
    }

    private static void PreloadPanel(DetailedPanel entry)
    {
        if (!entry.Panel)
        {
            ComputeCounts(entry);
            EnsurePanel(entry);
        }
    }

    private static void ComputeCounts(DetailedPanel entry)
    {
        int amount;
        int chance;
        bool? forceShow = null;
        bool isHidden;
        if (entry.Modifier is { } modifier)
        {
            amount = modifier.GetAmountPerGame();
            chance = modifier.GetAssignmentChance();
            forceShow = modifier.ForceShowModifierOnWiki;
            isHidden = !modifier.CanSpawnOnCurrentMode() || modifier.GetDescription() == string.Empty;
        }
        else
        {
            var role = entry.Role!;
            if (role is ICustomRole custom)
            {
                amount = custom.GetCount().GetValueOrDefault(0);
                chance = custom.GetChance().GetValueOrDefault(0);
                forceShow = custom.ForceShowRoleOnWiki;
            }
            else
            {
                amount = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role.Role);
                chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(role.Role);
            }

            isHidden = (role is ICustomRole custom2 && !custom2.CanSpawnOnCurrentMode()) ||
                       (Enum.IsDefined(role.Role) && role.IsRoleBlacklisted());
        }

        entry.Amount = amount;
        entry.Chance = chance;
        entry.Likelihood = amount * chance;
        entry.CanShow = ShouldShowPanel(forceShow, amount == 0 || chance == 0, isHidden);
        entry.CountsReady = true;
        entry.Visible = entry.CanShow;
    }

    private static List<DetailedPanel> UpdateEntries(List<DetailedPanel> entries, bool modifiers)
    {
        var searchText = GetSearchText();
        foreach (var entry in entries)
        {
            ComputeCounts(entry);
            entry.Visible = entry.CanShow && MatchesSearch(entry, searchText);
            if (entry.Panel)
            {
                SetPanelCount(entry.Panel!, entry.Amount, entry.Chance, entry.ModId);
                if (!entry.Visible && entry.Panel!.gameObject.activeSelf)
                {
                    entry.Panel.gameObject.SetActive(false);
                }
            }
        }

        var shown = SortEntries(entries, searchText);

        var instance = MatchInfoGuide.Instance;
        if (modifiers)
        {
            _modifiersScroller.SetYBoundsMax(GetGridScrollBounds(shown.Count));
            SetNoResultsVisible(_modifierNoResultsMessage, shown.Count == 0);
        }
        else
        {
            instance.MatchInfoRoleScroller.SetYBoundsMax(GetGridScrollBounds(shown.Count));
            SetNoResultsVisible(instance.rolesEnabledMessage, shown.Count == 0);
        }

        return shown;
    }

    private static List<DetailedPanel> SortEntries(List<DetailedPanel> entries, string searchText)
    {
        var sorted = entries.Where(p => p.Visible)
            .OrderByDescending(p => p.Title.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p =>
                p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        return (_sortMethod is SortingMethod.Alphabetical
                ? sorted.ThenBy(p => p.GetSortKey(_sortGrouping))
                : sorted.ThenByDescending(p => p.GetSortKey(_sortGrouping)))
            .ToList();
    }

    private static bool MatchesSearch(DetailedPanel entry, string searchText)
    {
        return searchText.Length == 0 ||
               entry.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
               entry.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static void RefreshActiveTab(bool scrollToTop)
    {
        if (IsGeneralTabActive)
        {
            RefreshGeneralTab(scrollToTop);
            return;
        }

        StartPopulate();
        if (scrollToTop)
        {
            (IsModifiersTabActive ? _modifiersScroller : _rolesScroller).ScrollToTop();
        }
    }

    private static void QueueSearchRefresh()
    {
        if (_searchDebounce != null)
        {
            Coroutines.Stop(_searchDebounce);
        }

        _searchDebounce = Coroutines.Start(CoSearchRefresh());
    }

    private static IEnumerator CoSearchRefresh()
    {
        yield return new WaitForSeconds(SearchDebounceSeconds);
        _searchDebounce = null;
        var text = GetSearchText();
        if (text == _appliedSearch)
        {
            yield break;
        }

        _appliedSearch = text;
        if (IsGeneralTabActive)
        {
            RefreshGeneralTab(true);
            yield break;
        }

        if (TryRefreshSearch(text))
        {
            yield break;
        }

        RefreshActiveTab(true);
    }

    private static bool TryRefreshSearch(string searchText)
    {
        var guide = MatchInfoGuide.Instance;
        if (!_guide || guide != _guide)
        {
            return false;
        }

        var entries = guide.activeTabIndex switch
        {
            RolesTabIndex => RoleEntries,
            ModifiersTabIndex => ModifierEntries,
            _ => null,
        };
        if (entries == null || _populate != null || !ArePanelsReady(entries))
        {
            return false;
        }

        foreach (var entry in entries)
        {
            entry.Visible = entry.CanShow && MatchesSearch(entry, searchText);
            if (entry.Panel!.gameObject.activeSelf != entry.Visible)
            {
                entry.Panel.gameObject.SetActive(entry.Visible);
            }
        }

        var shown = SortEntries(entries, searchText);
        foreach (var entry in shown)
        {
            entry.Panel!.transform.SetAsLastSibling();
        }

        var scroller = guide.activeTabIndex == ModifiersTabIndex
            ? _modifiersScroller
            : guide.MatchInfoRoleScroller;
        scroller.SetYBoundsMax(GetGridScrollBounds(shown.Count));
        var noResultsMessage = guide.activeTabIndex == ModifiersTabIndex
            ? _modifierNoResultsMessage
            : guide.rolesEnabledMessage;
        SetNoResultsVisible(noResultsMessage, shown.Count == 0);
        scroller.ScrollToTop();
        return true;
    }

    private static bool ArePanelsReady(List<DetailedPanel> entries)
    {
        if (entries.Count == 0)
        {
            return false;
        }

        foreach (var entry in entries)
        {
            if (!entry.CountsReady || !entry.Panel)
            {
                return false;
            }
        }

        return true;
    }

    private static void ResetSearchBox()
    {
        if (_searchDebounce != null)
        {
            Coroutines.Stop(_searchDebounce);
            _searchDebounce = null;
        }

        _appliedSearch = string.Empty;
        if (_searchIconIdle)
        {
            _searchIconIdle.sprite = MiraAssets.SearchIconIdleSprite;
        }

        if (_searchIconHover)
        {
            _searchIconHover.sprite = MiraAssets.SearchIconHoverSprite;
        }

        if (_searchBoxTmp && GetSearchText().Length > 0)
        {
            _suppressSearchRefresh = true;
            _searchBoxTmp.Clear();
            _suppressSearchRefresh = false;
        }
    }

    public static void ClearSearchText()
    {
        ResetSearchBox();
        RefreshActiveTab(true);
    }

    public static void SwitchFilter()
    {
        if (IsGeneralTabActive)
        {
            ChangeGeneralPage(-1);
            return;
        }

        var stepUp = (SortingFilter)((int)_sortFilter + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortFilter = stepUp;
        }
        else
        {
            _sortFilter = SortingFilter.EnabledOnly;
        }

        MiraApiPlugin.WikiSortingFilter.Value = _sortFilter;

        _searchSortingFilterIdle.sprite = GetSortFilterSprite(_sortFilter, false);
        _searchSortingFilterHover.sprite = GetSortFilterSprite(_sortFilter, true);

        RefreshActiveTab(false);
    }

    private static Sprite GetSortFilterSprite(SortingFilter value, bool hover)
    {
        if (hover)
        {
            switch (value)
            {
                case SortingFilter.AllRoles:
                    return MiraAssets.VisibleAllHoverSprite;
                case SortingFilter.DisabledOnly:
                    return MiraAssets.VisibleOffHoverSprite;
                default:
                    return MiraAssets.VisibleOnHoverSprite;
            }
        }

        switch (value)
        {
            case SortingFilter.AllRoles:
                return MiraAssets.VisibleAllIdleSprite;
            case SortingFilter.DisabledOnly:
                return MiraAssets.VisibleOffIdleSprite;
            default:
                return MiraAssets.VisibleOnIdleSprite;
        }
    }

    public static void SwitchMethod()
    {
        if (IsGeneralTabActive)
        {
            return;
        }

        var stepUp = (SortingMethod)((int)_sortMethod + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortMethod = stepUp;
        }
        else
        {
            _sortMethod = SortingMethod.Alphabetical;
        }

        MiraApiPlugin.WikiSortingMethod.Value = _sortMethod;

        _searchSortingOrderIdle.sprite = GetSortMethodSprite(_sortMethod, false);
        _searchSortingOrderHover.sprite = GetSortMethodSprite(_sortMethod, true);

        RefreshActiveTab(false);
    }

    private static Sprite GetSortMethodSprite(SortingMethod value, bool hover)
    {
        if (hover)
        {
            return value is SortingMethod.Alphabetical
                ? MiraAssets.SortingAzHoverSprite
                : MiraAssets.SortingZaHoverSprite;
        }

        return _sortMethod is SortingMethod.Alphabetical
            ? MiraAssets.SortingAzIdleSprite
            : MiraAssets.SortingZaIdleSprite;
    }

    public static void SwitchGrouping()
    {
        if (IsGeneralTabActive)
        {
            ChangeGeneralPage(1);
            return;
        }

        var stepUp = (SortingGroups)((int)_sortGrouping + 1);
        if (Enum.IsDefined(stepUp))
        {
            _sortGrouping = stepUp;
        }
        else
        {
            _sortGrouping = SortingGroups.Ungrouped;
        }

        MiraApiPlugin.WikiSortingGroup.Value = _sortGrouping;

        _searchSortingGroupIdle.sprite = GetSortGroupSprite(_sortGrouping, false);
        _searchSortingGroupHover.sprite = GetSortGroupSprite(_sortGrouping, true);

        RefreshActiveTab(false);
    }

    private static Sprite GetSortGroupSprite(SortingGroups value, bool hover)
    {
        if (hover)
        {
            switch (value)
            {
                case SortingGroups.Faction:
                    return MiraAssets.GroupFactionHoverSprite;
                case SortingGroups.AmountChance:
                    return MiraAssets.GroupCountHoverSprite;
                default:
                    return MiraAssets.GroupNoneHoverSprite;
            }
        }

        switch (value)
        {
            case SortingGroups.Faction:
                return MiraAssets.GroupFactionIdleSprite;
            case SortingGroups.AmountChance:
                return MiraAssets.GroupCountIdleSprite;
            default:
                return MiraAssets.GroupNoneIdleSprite;
        }
    }

    private static void CopyTextLayout(TextMeshPro source, TextMeshPro target)
    {
        target.font = source.font;
        target.fontSharedMaterial = source.fontSharedMaterial;
        target.fontSize = source.fontSize;
        target.fontSizeMin = source.fontSizeMin;
        target.fontSizeMax = source.fontSizeMax;
        target.enableAutoSizing = source.enableAutoSizing;
        target.overflowMode = source.overflowMode;
        target.alignment = source.alignment;
        target.horizontalAlignment = source.horizontalAlignment;
        target.verticalAlignment = source.verticalAlignment;
        target.margin = source.margin;
        target.enableWordWrapping = source.enableWordWrapping;
        var sourceRect = source.rectTransform;
        var targetRect = target.rectTransform;
        targetRect.anchorMin = sourceRect.anchorMin;
        targetRect.anchorMax = sourceRect.anchorMax;
        targetRect.pivot = sourceRect.pivot;
        targetRect.sizeDelta = sourceRect.sizeDelta;
        targetRect.anchoredPosition = sourceRect.anchoredPosition;
        targetRect.localScale = sourceRect.localScale;
        targetRect.localRotation = sourceRect.localRotation;
        targetRect.localPosition = sourceRect.localPosition;
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
        if (_titleText && _titleIcon)
        {
            switch (tabIndex)
            {
                case 0:
                    _titleText.text = MiraLocaleManager.Get("MiraApi.Wiki.PlayersTab");
                    _titleIcon.sprite = MiraAssets.IconPlayerList.LoadAsset();
                    break;
                case GeneralTabIndex:
                    _titleText.text = GetGeneralTitle();
                    _titleIcon.sprite = MiraAssets.IconSettings.LoadAsset();
                    break;
                case RolesTabIndex:
                    _titleText.text = MiraLocaleManager.Get("Roles");
                    _titleIcon.sprite = GetRandomRoleIcon();
                    break;
                case ModifiersTabIndex:
                    _titleText.text = MiraLocaleManager.Get("Modifiers");
                    _titleIcon.sprite = GetRandomModifierIcon();
                    break;
                default:
                    _titleText.text = TranslationController.Instance.GetString(StringNames.MatchInfoGuideTitle);
                    _titleIcon.sprite = MiraAssets.IconPlayerList.LoadAsset();
                    break;
            }

            _titleIcon.SetSizeLimit(0.55f);
        }

        _advancedInfoTabScroller?.gameObject.SetActive(false);
        SearchComponentContainer?.SetActive(true);
        if (tabIndex == GeneralTabIndex)
        {
            EnterGeneralTab();
        }
        else
        {
            LeaveGeneralTab();
        }

        UpdateSpinner();
    }

    public static Sprite GetRandomModifierIcon()
    {
        var activeMods = PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
            .Where(x => !x.HideOnUi && x.ModifierIcon != null).ToList();
        if (activeMods.Count > 0)
        {
            return activeMods.Random()!.ModifierIcon!.LoadAsset();
        }

        return GetRandomFactionIcon();
    }

    public static Sprite GetRandomRoleIcon()
    {
        var aliveRole = PlayerControl.LocalPlayer.Data?.Role;
        if (aliveRole != null && aliveRole.RoleIconSolid != null)
        {
            return aliveRole.RoleIconSolid;
        }

        return GetRandomFactionIcon();
    }

    public static Sprite GetRandomFactionIcon()
    {
        var random = UnityEngine.Random.Range(1, 4);
        if (random == 1)
        {
            return MiraAssets.IconCrewmate.LoadAsset();
        }

        if (random == 2)
        {
            return MiraAssets.IconImpostor.LoadAsset();
        }

        return MiraAssets.IconNeutral.LoadAsset();
    }

    private static bool TryPrepareAdvancedWiki(MatchInfoGuide instance)
    {
        instance.settingsTabs[RolesTabIndex].SetActive(false);
        _modifiersScroller?.gameObject.SetActive(false);
        LeaveGeneralTab();
        if (_currentAdvancedTabObject)
        {
            _currentAdvancedTabObject.SetActive(false);
            _currentAdvancedTabObject.Destroy();
        }

        if (SearchComponentContainer)
        {
            SearchComponentContainer.SetActive(false);
        }

        if (_advancedInfoTabScroller == null)
        {
            Warning("Wiki tab is null.");
            return false;
        }

        _advancedInfoTabScroller.gameObject.SetActive(true);
        UpdateSpinner();
        return true;
    }

    private static IEnumerator CoDisplayAdvancedWikiLater(float delay, Action display)
    {
        yield return new WaitForSeconds(delay);
        if (_guide && MatchInfoGuide.Instance && MatchInfoGuide.Instance.MatchInfoParent.activeSelf)
        {
            display();
        }
    }

    public static void DisplayAdvancedWiki(MatchInfoGuide instance, BaseModifier modifier)
    {
        var remaining = _openedAt + instance.transitionOpen.duration - Time.time;
        if (remaining > 0f)
        {
            Coroutines.Start(CoDisplayAdvancedWikiLater(remaining, () => DisplayAdvancedWiki(instance, modifier)));
            return;
        }

        var name = modifier.ModifierName;
        Info($"Opening advanced tab for {name}.");
        if (!TryPrepareAdvancedWiki(instance))
        {
            return;
        }

        _currentAdvancedTabObject = modifier.GetAdvancedWiki(instance, _titleText, _titleIcon, _advancedInfoTabScroller);
    }

    public static void DisplayAdvancedWiki(MatchInfoGuide instance, RoleBehaviour role)
    {
        var remaining = _openedAt + instance.transitionOpen.duration - Time.time;
        if (remaining > 0f)
        {
            Coroutines.Start(CoDisplayAdvancedWikiLater(remaining, () => DisplayAdvancedWiki(instance, role)));
            return;
        }

        Info($"Opening advanced tab for {role.GetRoleName()}.");
        if (!TryPrepareAdvancedWiki(instance))
        {
            return;
        }

        if (role is ICustomRole custom)
        {
            _currentAdvancedTabObject = custom.GetAdvancedWiki(instance, _titleText, _titleIcon, _advancedInfoTabScroller);
        }
        else
        {
            var titleTxt = $"{role.GetRoleName()}" +
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
                _titleIcon,
                _advancedInfoTabScroller,
                role.Role.ToString(),
                titleTxt,
                role.RoleIconColor,
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

    // ---- General (non-role) option pages: paginated General tab + cross-page search ----
    private static Scroller? _generalScroller;
    private static GameObject? _generalNoResults;
    private static readonly List<GeneralRow> GeneralRows = [];
    private static GameObject? _generalRowTemplate;
    private static Vector3 _generalRowOrigin;
    private static Vector3 _generalRowColumnStep;
    private static float _generalRowHeight = -0.65f; // negative: rows go down
    private static int _generalColumns = 2;
    private static float _generalVisibleHeight;
    private static bool _generalScrollerFailed;

    /// <summary>One vanilla-looking settings row (label box + value box) that is reused between pages.</summary>
    private sealed class GeneralRow(GameObject root, TextMeshPro label, TextMeshPro value)
    {
        public GameObject Root { get; } = root;
        public TextMeshPro Label { get; } = label;
        public TextMeshPro Value { get; } = value;
    }
    private static List<GuideOptionPage> _generalPages = [];
    private static int _generalPage; // 0 = vanilla settings, 1..N = _generalPages[page - 1]
    private static bool _generalSearching;

    // Saved state of the sort buttons so they can be restored when leaving the General tab.
    private static bool _sortButtonsRepurposed;
    private static string _savedFilterLabel = string.Empty;
    private static string _savedGroupLabel = string.Empty;
    private static Vector3 _savedFilterIconScale = Vector3.one;
    private static Vector3 _savedGroupIconScale = Vector3.one;

    private static bool IsGeneralTabActive =>
        _uiInitialized && MatchInfoGuide.Instance && MatchInfoGuide.Instance.activeTabIndex == GeneralTabIndex;

    private static void ResetGeneralState()
    {
        _generalScroller = null;
        _generalNoResults = null;
        GeneralRows.Clear();
        _generalRowTemplate = null;
        _generalScrollerFailed = false;
        _generalPages = [];
        _generalPage = 0;
        _generalSearching = false;
        _sortButtonsRepurposed = false;
    }

    private static void InitializeGeneralPanel(MatchInfoGuide instance)
    {
        _generalNoResults = Object.Instantiate(
            instance.rolesEnabledMessage,
            instance.rolesEnabledMessage.transform.parent,
            true);
        _generalNoResults.name = "GeneralNoResultsFound";
        _generalNoResults.SetActive(false);
        SetNoResultsText(_generalNoResults);
    }

    /// <summary>
    /// Clones the vanilla settings tab the first time it is needed, so extra pages are drawn with the exact same
    /// rows, fonts, masking and scroll area as the vanilla list. Done lazily because the vanilla rows are only
    /// created when the guide first opens. The clone is NOT added to <c>settingsTabs</c>; its visibility is managed
    /// here so the guide's tab count and tab buttons stay untouched.
    /// </summary>
    private static bool EnsureGeneralScroller(MatchInfoGuide guide)
    {
        if (_generalScroller != null && _generalRowTemplate)
        {
            return true;
        }

        if (_generalScrollerFailed)
        {
            return false;
        }

        var vanillaTab = guide.settingsTabs[GeneralTabIndex];
        var vanillaScroller = vanillaTab.GetComponent<Scroller>() ?? vanillaTab.GetComponentInChildren<Scroller>(true);
        if (!vanillaScroller || vanillaScroller.Inner.childCount < 2)
        {
            return false; // vanilla rows not built yet; try again next refresh
        }

        // Read the vanilla grid: first row, second entry (same row => 2 columns), third entry (next row).
        var vInner = vanillaScroller.Inner;
        var p0 = vInner.GetChild(0).localPosition;
        var p1 = vInner.GetChild(1).localPosition;
        var sameRow = Mathf.Abs(p1.y - p0.y) < 0.01f;
        var vanillaCount = vInner.childCount;
        _generalColumns = sameRow ? 2 : 1;
        _generalRowOrigin = p0;
        _generalRowColumnStep = sameRow ? new Vector3(p1.x - p0.x, 0f, 0f) : Vector3.zero;
        if (sameRow)
        {
            _generalRowHeight = vanillaCount >= 3 ? vInner.GetChild(2).localPosition.y - p0.y : -0.65f;
        }
        else
        {
            _generalRowHeight = p1.y - p0.y;
        }

        if (Mathf.Abs(_generalRowHeight) < 0.05f)
        {
            _generalRowHeight = -0.65f;
        }

        // Self-calibrate the visible height from how the vanilla list was bounded.
        var vanillaRows = Mathf.CeilToInt(vanillaCount / (float)_generalColumns);
        var visible = (vanillaRows * Mathf.Abs(_generalRowHeight)) - vanillaScroller.ContentYBounds.max;
        _generalVisibleHeight = visible > 0.1f && visible < vanillaRows * Mathf.Abs(_generalRowHeight)
            ? visible
            : 3.3f * Mathf.Abs(_generalRowHeight);

        var clone = Object.Instantiate(vanillaTab, vanillaTab.transform.parent);
        clone.name = "GeneralOptionsPanel";
        var cloneScroller = clone.GetComponent<Scroller>() ?? clone.GetComponentInChildren<Scroller>(true);
        var cInner = cloneScroller.Inner;

        // Keep the first row as an inactive prototype, delete the rest.
        var template = cInner.GetChild(0).gameObject;
        for (var i = cInner.childCount - 1; i >= 1; i--)
        {
            var child = cInner.GetChild(i).gameObject;
            child.SetActive(false);
            child.Destroy();
        }

        template.SetActive(false);
        _generalRowTemplate = template;
        _generalScroller = cloneScroller;
        clone.SetActive(false);
        return true;
    }

    private static GeneralRow? CreateGeneralRow()
    {
        if (_generalScroller == null || !_generalRowTemplate)
        {
            return null;
        }

        var go = Object.Instantiate(_generalRowTemplate!, _generalScroller.Inner);
        go.name = "GeneralOptionRow";

        // The game would re-translate these and overwrite our text.
        foreach (var translator in go.GetComponentsInChildren<TextTranslatorTMP>(true))
        {
            translator.Destroy();
        }

        // Label is the left-most text, value is the right-most (same as the vanilla entries).
        var texts = go.GetComponentsInChildren<TextMeshPro>(true).OrderBy(t => t.transform.position.x).ToArray();
        if (texts.Length < 2)
        {
            Warning("Vanilla settings row has fewer than two text fields; cannot clone it for the general pages.");
            go.Destroy();
            _generalScrollerFailed = true;
            return null;
        }

        return new GeneralRow(go, texts[0], texts[^1]);
    }

    private static void EnterGeneralTab()
    {
        if (!_uiInitialized)
        {
            return;
        }

        RefreshGeneralTab(true);
    }

    private static void LeaveGeneralTab()
    {
        if (_generalScroller != null)
        {
            _generalScroller.gameObject.SetActive(false);
        }

        if (_generalNoResults)
        {
            _generalNoResults!.SetActive(false);
        }

        RestoreSortButtons();
    }

    private static void ChangeGeneralPage(int delta)
    {
        var total = _generalPages.Count + 1;
        if (total <= 1)
        {
            return;
        }

        _generalPage = (_generalPage + delta + total) % total;
        RefreshGeneralTab(true);
    }

    /// <summary>
    /// Rebuilds pages from live option data and redraws the General tab.
    /// Pages are rebuilt every time because values, group visibility and the gamemode can change.
    /// </summary>
    private static void RefreshGeneralTab(bool scrollToTop)
    {
        var guide = MatchInfoGuide.Instance;
        if (!_uiInitialized || !guide)
        {
            return;
        }

        _generalPages = GuideOptionPages.BuildPages(guide.GetBoolString);
        if (_generalPage > _generalPages.Count)
        {
            _generalPage = 0;
        }

        var vanillaTab = guide.settingsTabs[GeneralTabIndex];

        // No MiraAPI general options in this match: leave the vanilla tab exactly as it always was.
        if (_generalPages.Count == 0)
        {
            _generalSearching = false;
            _generalScroller?.gameObject.SetActive(false);
            _generalNoResults?.SetActive(false);
            vanillaTab.SetActive(true);
            RestoreSortButtons();
            UpdateGeneralTitle();
            return;
        }

        var searchText = GetSearchText().Trim();
        _generalSearching = searchText.Length > 0;

        if (_generalSearching)
        {
            ShowGeneralEntries(GuideOptionPages.Search(_generalPages, searchText), true, scrollToTop);
            vanillaTab.SetActive(false);
            RepurposeSortButtons(false);
        }
        else if (_generalPage == 0)
        {
            _generalScroller?.gameObject.SetActive(false);
            _generalNoResults?.SetActive(false);
            vanillaTab.SetActive(true);
            RepurposeSortButtons(true);
        }
        else
        {
            ShowGeneralEntries(_generalPages[_generalPage - 1].Entries, false, scrollToTop);
            vanillaTab.SetActive(false);
            RepurposeSortButtons(true);
        }

        UpdateGeneralTitle();
    }

    private static void ShowGeneralEntries(IReadOnlyList<GuideOptionEntry> entries, bool showGroup, bool scrollToTop)
    {
        var guide = MatchInfoGuide.Instance;
        if (!guide || !EnsureGeneralScroller(guide))
        {
            // Could not clone the vanilla rows: fall back to the plain vanilla tab rather than a blank page.
            guide?.settingsTabs[GeneralTabIndex].SetActive(true);
            return;
        }

        while (GeneralRows.Count < entries.Count)
        {
            var row = CreateGeneralRow();
            if (row == null)
            {
                break;
            }

            GeneralRows.Add(row);
        }

        for (var i = 0; i < GeneralRows.Count; i++)
        {
            var row = GeneralRows[i];
            if (i >= entries.Count)
            {
                row.Root.SetActive(false);
                continue;
            }

            var entry = entries[i];
            row.Label.text = showGroup ? $"<size=60%>{entry.GroupName}</size>\n{entry.Title}" : entry.Title;
            row.Value.text = entry.Value;
            row.Root.transform.localPosition = _generalRowOrigin +
                                               (_generalRowColumnStep * (i % _generalColumns)) +
                                               new Vector3(0f, _generalRowHeight * (i / _generalColumns), 0f);
            row.Root.SetActive(true);
        }

        var rows = Mathf.CeilToInt(Mathf.Min(entries.Count, GeneralRows.Count) / (float)_generalColumns);
        _generalScroller!.gameObject.SetActive(true);
        _generalScroller.SetYBoundsMax(Mathf.Clamp((rows * Mathf.Abs(_generalRowHeight)) - _generalVisibleHeight, 0f, 999f));
        _generalNoResults?.SetActive(entries.Count == 0);
        if (scrollToTop)
        {
            _generalScroller.ScrollToTop();
        }
    }

    private static string GetGeneralTitle()
    {
        var baseTitle = MiraLocaleManager.Get("MiraApi.Wiki.SettingsTab");
        if (!_uiInitialized || _generalPages.Count == 0)
        {
            return baseTitle;
        }

        if (_generalSearching)
        {
            return $"{baseTitle}: {MiraLocaleManager.Get("MiraApi.Wiki.SearchResults", "Search Results")}";
        }

        var counter = $"({_generalPage + 1}/{_generalPages.Count + 1})";
        return _generalPage == 0
            ? $"{baseTitle} {counter}"
            : $"{baseTitle}: {_generalPages[_generalPage - 1].Title} {counter}";
    }

    private static void UpdateGeneralTitle()
    {
        if (_titleText && IsGeneralTabActive)
        {
            _titleText.text = GetGeneralTitle();
        }
    }

    /// <summary>
    /// Turns the Filter button into "previous page" and the Grouping button into "next page",
    /// and hides the Order button (it only sorts roles/modifiers).
    /// </summary>
    /// <param name="paginationVisible">False while searching, since search results are a single page.</param>
    private static void RepurposeSortButtons(bool paginationVisible)
    {
        if (!SearchSortingFilterButton || !SearchSortingGroupButton || !SearchSortingOrderButton)
        {
            return;
        }

        if (!_sortButtonsRepurposed)
        {
            _savedFilterLabel = FilterSortingTmp.text;
            _savedGroupLabel = GroupSortingTmp.text;
            _savedFilterIconScale = _searchSortingFilterIdle.transform.localScale;
            _savedGroupIconScale = _searchSortingGroupIdle.transform.localScale;

            SetPaginationSprites(_searchSortingFilterIdle, _searchSortingFilterHover, true);
            SetPaginationSprites(_searchSortingGroupIdle, _searchSortingGroupHover, false);
            FilterSortingTmp.text = MiraLocaleManager.Get("MiraApi.Wiki.PrevPage", "Prev");
            GroupSortingTmp.text = MiraLocaleManager.Get("MiraApi.Wiki.NextPage", "Next");
            _sortButtonsRepurposed = true;
        }

        SearchSortingOrderButton.gameObject.SetActive(false);
        SearchSortingFilterButton.gameObject.SetActive(paginationVisible);
        SearchSortingGroupButton.gameObject.SetActive(paginationVisible);
    }

    private static void SetPaginationSprites(SpriteRenderer idle, SpriteRenderer hover, bool flip)
    {
        var idleSprite = MiraAssets.NextButton.LoadAsset();
        var hoverSprite = MiraAssets.NextButtonActive.LoadAsset();

        // Match the on-screen size of the sort icon being replaced, since the arrow art differs in size.
        var currentHeight = idle.sprite ? idle.sprite.bounds.size.y * idle.transform.localScale.y : 0f;
        var newHeight = idleSprite.bounds.size.y;
        var scale = currentHeight > 0f && newHeight > 0f ? currentHeight / newHeight : 1f;

        idle.sprite = idleSprite;
        hover.sprite = hoverSprite;
        idle.flipX = hover.flipX = flip;
        idle.transform.localScale = hover.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static void RestoreSortButtons()
    {
        if (!_sortButtonsRepurposed)
        {
            return;
        }

        _sortButtonsRepurposed = false;
        if (!SearchSortingFilterButton || !SearchSortingGroupButton || !SearchSortingOrderButton)
        {
            return;
        }

        _searchSortingFilterIdle.sprite = GetSortFilterSprite(_sortFilter, false);
        _searchSortingFilterHover.sprite = GetSortFilterSprite(_sortFilter, true);
        _searchSortingGroupIdle.sprite = GetSortGroupSprite(_sortGrouping, false);
        _searchSortingGroupHover.sprite = GetSortGroupSprite(_sortGrouping, true);
        _searchSortingFilterIdle.flipX = _searchSortingFilterHover.flipX = false;
        _searchSortingGroupIdle.flipX = _searchSortingGroupHover.flipX = false;
        _searchSortingFilterIdle.transform.localScale =
            _searchSortingFilterHover.transform.localScale = _savedFilterIconScale;
        _searchSortingGroupIdle.transform.localScale =
            _searchSortingGroupHover.transform.localScale = _savedGroupIconScale;
        FilterSortingTmp.text = _savedFilterLabel;
        GroupSortingTmp.text = _savedGroupLabel;
        SearchSortingOrderButton.gameObject.SetActive(true);
        SearchSortingFilterButton.gameObject.SetActive(true);
        SearchSortingGroupButton.gameObject.SetActive(true);
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