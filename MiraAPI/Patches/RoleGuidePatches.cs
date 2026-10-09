using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    private static readonly List<DetailedPanel> RoleEntries = [];
    private static readonly List<DetailedPanel> ModifierEntries = [];

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
    private static bool _suppressSearchRefresh;
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
        __instance.transitionOpen.targetSize = 1.3f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.Awake))]
    private static void AwakePostfix(MatchInfoGuide __instance)
    {
        Coroutines.Start(CoPreload(__instance));
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

        Initialize(instance);
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
            Initialize(__instance);
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
        __instance.MatchInfoParent.SetActive(true);
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
            OpenTab(RolesTabIndex);
        }

        return false;
    }

    private static void Initialize(MatchInfoGuide instance)
    {
        if (_guide == instance)
        {
            return;
        }

        if (_populate != null)
        {
            Coroutines.Stop(_populate);
            _populate = null;
        }

        _guide = instance;
        _loading = false;
        RoleEntries.Clear();
        ModifierEntries.Clear();
        var sortingOrderButton = Object.Instantiate(
            HudManager.Instance.SettingsButton,
            instance.TabButtons[2].transform.parent);
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

        instance.numOfTabs = 4;
        instance.TabButtons[2].SelectButton(true);
        ApplyPanelLayout(instance);
        instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
        instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);

        var modifiersTab = Object.Instantiate(
            instance.settingsTabs[RolesTabIndex],
            instance.settingsTabs[RolesTabIndex].transform.parent);
        modifiersTab.name = "ModifiersPanel";
        modifiersTab.transform.FindChild("MaskArea")?.transform.localPosition =
            new Vector3(-0.0184f, 0.15f, -0.1f);
        modifiersTab.transform.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
        _modifiersScroller = modifiersTab.GetComponent<Scroller>();
        instance.settingsTabs.Add(modifiersTab);

        var wikiTab = Object.Instantiate(
            instance.settingsTabs[RolesTabIndex],
            instance.settingsTabs[RolesTabIndex].transform.parent);
        wikiTab.name = "AdvancedWikiPanels";
        wikiTab.transform.FindChild("MaskArea")?.transform.localPosition = new Vector3(-0.0184f, 0.15f, -0.1f);
        wikiTab.transform.GetAllChildren()
            .First(x => x.name.Contains("BG_Gradient")).transform.localPosition = new Vector3(0, -0.62f, -5);
        _advancedInfoTabScroller = wikiTab.GetComponent<Scroller>();

        var spinnerParent = instance.settingsTabs[RolesTabIndex].transform.parent;
        var spinnerPos = spinnerParent.InverseTransformPoint(instance.MatchInfoRoleMaskArea.transform.position);
        spinnerPos.z = -5f;
        _loadingSpinner = LoadingRing.Create(spinnerParent, spinnerPos, 0.6f);
        _loadingSpinner.GetComponent<SpriteRenderer>().color = MiraAssets.AcceptedTeal;

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
            instance.TabButtons[2].gameObject,
            instance.TabButtons[2].transform.parent);
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

                RefreshActiveTab(true);
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

        playerButton.transform.localPosition = new Vector3(-3.6f, 0.656f, -0.2f);
        settingButton.transform.localPosition = new Vector3(-3.6f, 0.056f, -0.2f);
        rolesButton.transform.localPosition = new Vector3(-3.6f, -0.544f, -0.2f);
        modifiersButton.transform.localPosition = new Vector3(-3.6f, -1.144f, -0.2f);
        BuildEntries();
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

    private static void OpenTab(int tabIndex)
    {
        _advancedInfoTabScroller?.gameObject.SetActive(false);
        MatchInfoGuide.Instance.SetActiveTab(tabIndex);
        foreach (var entry in tabIndex == ModifiersTabIndex ? ModifierEntries : RoleEntries)
        {
            if (entry.Panel && entry.Panel!.gameObject.activeSelf)
            {
                entry.Panel.gameObject.SetActive(false);
            }
        }

        StartPopulate();
    }

    public static void OpenModifiersTab() => OpenTab(ModifiersTabIndex);

    public static void OpenRolesTab() => OpenTab(RolesTabIndex);

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

    private static void StartPopulate()
    {
        if (_populate != null)
        {
            Coroutines.Stop(_populate);
        }

        _loading = true;
        UpdateSpinner();
        _populate = Coroutines.Start(CoPopulate());
    }

    private static IEnumerator CoPopulate()
    {
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
        var budget = 1000L / (fps * 2);
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

        var panel = Object.Instantiate(
            _guide!.MatchInfoRolePanelPrefab,
            entry.Modifier != null ? _modifiersScroller.Inner : _rolesScroller.Inner);
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
        entry.Visible = ShouldShowPanel(forceShow, amount == 0 || chance == 0, isHidden);
    }

    private static List<DetailedPanel> UpdateEntries(List<DetailedPanel> entries, bool modifiers)
    {
        foreach (var entry in entries)
        {
            ComputeCounts(entry);
            if (entry.Panel)
            {
                SetPanelCount(entry.Panel!, entry.Amount, entry.Chance, entry.ModId);
                if (!entry.Visible && entry.Panel!.gameObject.activeSelf)
                {
                    entry.Panel.gameObject.SetActive(false);
                }
            }
        }

        var searchText = GetSearchText();
        var sorted = entries.Where(p => p.Visible)
            .OrderByDescending(p => p.Title.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p =>
                p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        var shown = (_sortMethod is SortingMethod.Alphabetical
                ? sorted.ThenBy(p => p.GetSortKey(_sortGrouping))
                : sorted.ThenByDescending(p => p.GetSortKey(_sortGrouping)))
            .ToList();

        var instance = MatchInfoGuide.Instance;
        if (modifiers)
        {
            _modifiersScroller.SetYBoundsMax(GetGridScrollBounds(shown.Count));
            /*instance.rolesEnabledMessage.SetActive(shown.Count == 0);*/
        }
        else
        {
            instance.MatchInfoRoleScroller.SetYBoundsMax(GetGridScrollBounds(shown.Count));
            instance.rolesEnabledMessage.SetActive(shown.Count == 0);
        }

        return shown;
    }

    private static void RefreshActiveTab(bool scrollToTop)
    {
        StartPopulate();
        if (scrollToTop)
        {
            (IsModifiersTabActive ? _modifiersScroller : _rolesScroller).ScrollToTop();
        }
    }

    private static void ResetSearchBox()
    {
        _searchIconIdle.sprite = MiraAssets.SearchIconIdleSprite;
        _searchIconHover.sprite = MiraAssets.SearchIconHoverSprite;
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
        UpdateSpinner();
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
        UpdateSpinner();
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
