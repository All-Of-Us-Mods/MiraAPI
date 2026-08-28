using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Presets;
using MiraAPI.Translation;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Patches.Options;

[RegisterInIl2Cpp]
public class MenuState(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public static MenuState Instance { get; private set; } = null!;

    public static int ModCount => MiraPluginManager.Instance.RegisteredPluginsWithOptions.Length;

    private GameSettingMenu Gsm { get; set; }

    public MenuCategory CurrentMenu { get; private set; }

    public int CurrentModIdx { get; private set; }

    public GameObject CurrentContainer => Containers[CurrentMenu][CurrentModIdx];

    [HideFromIl2Cpp]
    public MiraPluginInfo CurrentMod => MiraPluginManager.Instance.RegisteredPluginsWithOptions[CurrentModIdx - 1];

    // UI elements
    private TextMeshPro _text = null!;

    private enum MenuButton
    {
        Roles,
        Modifiers,
        CustomOne,
        CustomTwo,
    }

    private readonly Dictionary<MenuButton, PassiveButton> _smallButtons = [];
    private readonly Dictionary<MenuButton, PassiveButton> _largeButtons = [];

    [HideFromIl2Cpp]
    public Dictionary<MenuCategory, Dictionary<int, GameObject>> Containers { get; } = new()
    {
        { MenuCategory.Preset, [] },
        { MenuCategory.Game, [] },
        { MenuCategory.Roles, [] },
        { MenuCategory.Modifiers, [] },
        { MenuCategory.CustomOne, [] },
        { MenuCategory.CustomTwo, [] },
    };

    [HideFromIl2Cpp]
    internal Dictionary<int, bool> FinishedRoleMenus { get; } = [];
    [HideFromIl2Cpp]
    internal Dictionary<int, bool> QueuedRoleMenuRefresh { get; } = [];

    public void Awake()
    {
        if (Instance)
        {
            Error($"MenuState already exists! Destroying duplicate {name}.");
            Destroy(this);
            return;
        }

        Instance = this;
        Gsm = GetComponent<GameSettingMenu>();

        if (!Gsm)
        {
            Error($"MenuState could not find GameSettingMenu component! Destroying {name}.");
            Destroy(this);
            return;
        }

        // Reload presets
        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            PresetManager.LoadPresets(plugin);
        }

        // Initialize and setup UI
        InitializeContainers();
        InitializeUi();
        UpdateUi();
    }

    private void InitializeUi()
    {
        // Disable game settings label
        Gsm.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        // Create mod name text
        var helpThing = Gsm.transform.FindChild("What Is This?");
        var tmpText = Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        Destroy(tmpText.GetComponent<TextTranslatorTMP>());
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        _text = tmpText.GetComponent<TextMeshPro>();
        _text.fontSizeMax = 3.2f;
        _text.overflowMode = TextOverflowModes.Overflow;
        _text.alignment = TextAlignmentOptions.Center;
        _text.rectTransform.sizeDelta = new Vector2(1.5f, 0.8f);

        // Create next and previous buttons
        var nextButton = Instantiate(Gsm.BackButton, Gsm.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        nextButton.name = "RightArrowButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var nextPassiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        nextPassiveButton.OnClick = new Button.ButtonClickedEvent();
        nextPassiveButton.OnClick.AddListener((UnityAction)NextMod);

        var previousButton = Instantiate(nextButton, Gsm.BackButton.transform.parent).gameObject;
        previousButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        previousButton.name = "LeftArrowButton";
        previousButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        previousButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX =
            previousButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;

        var prevPassiveButton = previousButton.gameObject.GetComponent<PassiveButton>();
        prevPassiveButton.OnClick = new Button.ButtonClickedEvent();
        prevPassiveButton.OnClick.AddListener((UnityAction)PreviousMod);

        // Create menu buttons
        _largeButtons[(int)MenuButton.Roles] = Gsm.RoleSettingsButton;

        // modifiers button
        var mBtn = Instantiate(Gsm.RoleSettingsButton, Gsm.RoleSettingsButton.transform.parent);
        _largeButtons[MenuButton.Modifiers] = mBtn;
        mBtn.name = "ModifiersButton";
        mBtn.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        mBtn.buttonText.text = "Modifiers".Translate();
        mBtn.OnClick = new Button.ButtonClickedEvent();
        mBtn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Modifiers, false);
            }));
        mBtn.OnMouseOver = new UnityEvent();
        mBtn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Modifiers, true);
            }));

        // first custom menu
        var c1Btn = Instantiate(mBtn, mBtn.transform.parent);
        _largeButtons[MenuButton.CustomOne] = c1Btn;
        c1Btn.name = "CustomOneButton";
        c1Btn.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        c1Btn.OnClick = new Button.ButtonClickedEvent();
        c1Btn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomOne, false);
            }));
        c1Btn.OnMouseOver = new UnityEvent();
        c1Btn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomOne, true);
            }));

        // second custom menu
        var c2Btn = Instantiate(c1Btn, c1Btn.transform.parent);
        _largeButtons[MenuButton.CustomTwo] = c2Btn;
        c2Btn.name = "CustomTwoButton";
        c2Btn.OnClick = new Button.ButtonClickedEvent();
        c2Btn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomTwo, false);
            }));
        c2Btn.OnMouseOver = new UnityEvent();
        c2Btn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomTwo, true);
            }));

        // small roles button
        var smRolesBtn = Instantiate(
            Gsm.RoleSettingsButton,
            Gsm.RoleSettingsButton.transform.parent);
        _smallButtons[MenuButton.Roles] = smRolesBtn;
        smRolesBtn.name = "SmallRolesButton";
        smRolesBtn.OnClick = new Button.ButtonClickedEvent();
        smRolesBtn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Roles, false);
            }));
        smRolesBtn.OnMouseOver = new UnityEvent();
        smRolesBtn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Roles, true);
            }));

        var roleText = smRolesBtn.buttonText;
        roleText.text = "Roles".Translate();
        roleText.GetComponent<TextTranslatorTMP>().Destroy();
        roleText.alignment = TextAlignmentOptions.Center;
        roleText.transform.parent.localPosition = new Vector3(
            -.525f,
            roleText.transform.parent.localPosition.y,
            roleText.transform.parent.localPosition.z);

        foreach (var collider in smRolesBtn.Colliders)
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / 2, col.size.y);
            }
        }

        foreach (var rend in smRolesBtn.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / 2, rend.size.y);
        }

        // small modifiers button
        var smModBtn = Instantiate(smRolesBtn, smRolesBtn.transform.parent);
        _smallButtons[MenuButton.Modifiers] = smModBtn;
        smModBtn.name = "SmallModifiersButton";
        smModBtn.buttonText.text = "Modifiers".Translate();
        smModBtn.OnClick = new Button.ButtonClickedEvent();
        smModBtn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Modifiers, false);
            }));
        smModBtn.OnMouseOver = new UnityEvent();
        smModBtn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.Modifiers, true);
            }));

        // small c1 button
        var smC1Btn = Instantiate(smModBtn, smModBtn.transform.parent);
        _smallButtons[MenuButton.CustomOne] = smC1Btn;
        smC1Btn.name = "SmallCustomOneButton";
        smC1Btn.buttonText.text = "Custom 1";
        smC1Btn.OnClick = new Button.ButtonClickedEvent();
        smC1Btn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomOne, false);
            }));
        smC1Btn.OnMouseOver = new UnityEvent();
        smC1Btn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomOne, true);
            }));

        // small c2 button
        var smC2Btn = Instantiate(smModBtn, smModBtn.transform.parent);
        _smallButtons[MenuButton.CustomTwo] = smC2Btn;
        smC2Btn.name = "SmallCustomTwoButton";
        smC2Btn.buttonText.text = "Custom 2";
        smC2Btn.OnClick = new Button.ButtonClickedEvent();
        smC2Btn.OnClick.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomTwo, false);
            }));
        smC2Btn.OnMouseOver = new UnityEvent();
        smC2Btn.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                Gsm.ChangeTab((int)MenuCategory.CustomTwo, true);
            }));
    }

    private void InitializeContainers()
    {
        // Set vanilla containers
        Containers[MenuCategory.Game][0] = Gsm.GameSettingsTab.settingsContainer.gameObject;
        Containers[MenuCategory.Roles][0] = Gsm.RoleSettingsTab.RoleChancesSettings;

        for (var idx = 0; idx < ModCount; idx++)
        {
            var mod = MiraPluginManager.Instance.RegisteredPluginsWithOptions[idx];

            // Create preset container for each mod
            var presetContainer = new GameObject($"PresetHolder_{mod.PluginInfo.Metadata.Name}");
            presetContainer.transform.SetParent(Gsm.PresetsTab.transform, false);
            presetContainer.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            presetContainer.transform.localPosition = new Vector3(-1.9f, 1.7f, 0);
            presetContainer.gameObject.SetActive(false);

            var arrange = presetContainer.AddComponent<GridArrange>();
            arrange.Alignment = GridArrange.StartAlign.Right;
            arrange.CellSize = new Vector2(2.15f, -.55f);
            arrange.MaxColumns = 2;

            Containers[MenuCategory.Preset][idx + 1] = presetContainer;

            // Create game options container
            var gameContainer = Containers[MenuCategory.Game][idx + 1] = Instantiate(
                Gsm.GameSettingsTab.settingsContainer.gameObject,
                Gsm.GameSettingsTab.settingsContainer.gameObject.transform.parent);
            gameContainer.name = $"GameSettings_{mod.PluginInfo.Metadata.Name}";
            gameContainer.transform.DestroyChildren();
            gameContainer.SetActive(false);

            // Roles container
            var rolesContainer = Containers[MenuCategory.Roles][idx + 1] = Instantiate(
                Gsm.RoleSettingsTab.RoleChancesSettings,
                Gsm.RoleSettingsTab.RoleChancesSettings.transform.parent);
            rolesContainer.name = $"RoleSettings_{mod.PluginInfo.Metadata.Name}";
            rolesContainer.transform.DestroyChildren();
            rolesContainer.SetActive(false);

            // Modifiers + Custom containers
            for (var i = MenuCategory.Modifiers; i <= MenuCategory.CustomTwo; i++)
            {
                var cont = Containers[i][idx + 1] = Instantiate(
                    Gsm.GameSettingsTab.settingsContainer.gameObject,
                    Gsm.GameSettingsTab.settingsContainer.gameObject.transform.parent);
                cont.name = $"{i}_{mod.PluginInfo.Metadata.Name}";
                cont.transform.DestroyChildren();
                cont.SetActive(false);
            }
        }
    }

    private void UpdateContainers()
    {
        foreach (var (_, dict) in Containers)
        {
            foreach (var (_, container) in dict)
            {
                container.SetActive(false);
            }
        }

        Info($"Enabled {CurrentMenu} for {CurrentModIdx}");

        // Preset containers
        if (CurrentModIdx != 0)
        {
            Containers[MenuCategory.Preset][CurrentModIdx].SetActive(true);
        }

        // Game / Modifier / Custom Settings tab
        // GST.settingsContainer is same as GST.scrollBar.Inner
        if (CurrentMenu is not (MenuCategory.Preset or MenuCategory.Roles))
        {
            Gsm.GameSettingsTab.settingsContainer = Gsm.GameSettingsTab.scrollBar.Inner = CurrentContainer.transform;
            CurrentContainer.SetActive(true);
        }

        // Role Settings Tab
        Gsm.RoleSettingsTab.RoleChancesSettings = Containers[MenuCategory.Roles][CurrentModIdx];
        Gsm.RoleSettingsTab.scrollBar.Inner = Containers[MenuCategory.Roles][CurrentModIdx].transform;
        Containers[MenuCategory.Roles][CurrentModIdx].SetActive(true);
    }

    private void UpdateUi()
    {
        // Update mod text
        if (CurrentModIdx == 0)
        {
            _text.text = $"<size=40%>(Page 1/{ModCount + 1})</size>\nMain";
        }
        else
        {
            var modName = CurrentMod.MiraPlugin.OptionsTitleText.Translate();
            _text.text = $"<size=40%>(Page {CurrentModIdx + 1}/{ModCount + 1})</size>\n" +
                         modName[..Math.Min(modName.Length, 25)];
        }

        UpdateContainers();

        // Update buttons
        foreach (var (_, btn) in _smallButtons)
        {
            btn.gameObject.SetActive(false);
        }

        foreach (var (_, btn) in _largeButtons)
        {
            btn.gameObject.SetActive(false);
        }

        if (CurrentModIdx == 0)
        {
            Gsm.GameSettingsButton.gameObject.SetActive(true);
            Gsm.RoleSettingsButton.gameObject.SetActive(
                CustomGameModeManager.ActiveMode != null && CustomGameModeManager.ActiveMode.ShowNormalRoleSettings);
        }
        else
        {
            var hasOptions =
                CurrentMod.InternalOptionGroups.Exists(g =>
                    g.OptionableType == null && g.ParentMenu == MenuCategory.Game);
            var customRoles = CurrentMod.InternalRoles.Values.OfType<ICustomRole>();
            var hasRoles = customRoles.Any(x => x.VisibleInSettings.Invoke());
            var hasModifiers = CurrentMod.InternalOptionGroups.Exists(g =>
                g.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true
                || g.ParentMenu == MenuCategory.Modifiers);
            var hasC1 = CurrentMod.InternalOptionGroups.Exists(g => g.ParentMenu == MenuCategory.CustomOne);
            var hasC2 = CurrentMod.InternalOptionGroups.Exists(g => g.ParentMenu == MenuCategory.CustomTwo);
            const float leftPos = -3.65f;
            const float rightPos = -2.27f;

            var smallRolesModifiers = hasRoles && hasModifiers && (hasC1 || hasC2);
            var smallC1C2 = hasC1 && hasC2;

            Info("hasOptions: " + hasOptions);
            Info("hasRoles: " + hasRoles);
            Info("hasModifiers: " + hasModifiers);
            Info("hasC1: " + hasC1);
            Info("hasC2: " + hasC2);
            Info("smallC1C2: " + smallC1C2);
            Info("smallRolesModifiers: " + smallRolesModifiers);

            var position = Gsm.GameSettingsButton.transform.localPosition;

            // If this mod has game options, enable the game settings button
            if (hasOptions)
            {
                Gsm.GameSettingsButton.gameObject.SetActive(true);
                position.y -= 0.637f;
            }
            else
            {
                Gsm.GameSettingsButton.gameObject.SetActive(false);
            }

            // If we should use small roles/modifiers buttons, enable them and position them accordingly
            if (smallRolesModifiers)
            {
                _smallButtons[MenuButton.Roles].gameObject.SetActive(true);
                _smallButtons[MenuButton.Roles].transform.localPosition = new Vector3(leftPos, position.y, position.z);
                _smallButtons[MenuButton.Modifiers].gameObject.SetActive(true);
                _smallButtons[MenuButton.Modifiers].transform.localPosition =
                    new Vector3(rightPos, position.y, position.z);
                position.y -= 0.637f;
            }

            // If we should use large roles button, enable and position, otherwise disable
            if (hasRoles && !smallRolesModifiers)
            {
                _largeButtons[MenuButton.Roles].gameObject.SetActive(true);
                _largeButtons[MenuButton.Roles].transform.localPosition = position;
                position.y -= 0.637f;
            }
            else
            {
                _largeButtons[MenuButton.Roles].gameObject.SetActive(false);
            }

            // If we should use large modifiers button, enable and position, otherwise disable
            if (hasModifiers && !smallRolesModifiers)
            {
                _largeButtons[MenuButton.Modifiers].gameObject.SetActive(true);
                _largeButtons[MenuButton.Modifiers].transform.localPosition = position;
                position.y -= 0.637f;
            }
            else
            {
                _largeButtons[MenuButton.Modifiers].gameObject.SetActive(false);
            }

            // if we should use small custom buttons, enable and position accordingly
            if (smallC1C2)
            {
                _smallButtons[MenuButton.CustomOne].gameObject.SetActive(true);
                _smallButtons[MenuButton.CustomOne].transform.localPosition =
                    new Vector3(leftPos, position.y, position.z);
                _smallButtons[MenuButton.CustomOne].buttonText.text = CurrentMod.MiraPlugin.CustomOptionMenuNameOne.Translate();

                _smallButtons[MenuButton.CustomTwo].gameObject.SetActive(true);
                _smallButtons[MenuButton.CustomTwo].transform.localPosition =
                    new Vector3(rightPos, position.y, position.z);
                _smallButtons[MenuButton.CustomTwo].buttonText.text = CurrentMod.MiraPlugin.CustomOptionMenuNameTwo.Translate();
                position.y -= 0.637f;
            }

            // C1 only
            if (hasC1 && !smallC1C2)
            {
                _largeButtons[MenuButton.CustomOne].gameObject.SetActive(true);
                _largeButtons[MenuButton.CustomOne].transform.localPosition = position;
                _largeButtons[MenuButton.CustomOne].buttonText.text = CurrentMod.MiraPlugin.CustomOptionMenuNameOne.Translate();
                position.y -= 0.637f;
            }
            else
            {
                _largeButtons[MenuButton.CustomOne].gameObject.SetActive(false);
            }

            // C2 only
            if (hasC2 && !smallC1C2)
            {
                _largeButtons[MenuButton.CustomTwo].gameObject.SetActive(true);
                _largeButtons[MenuButton.CustomTwo].transform.localPosition = position;
                _largeButtons[MenuButton.CustomTwo].buttonText.text = CurrentMod.MiraPlugin.CustomOptionMenuNameTwo.Translate();
                position.y -= 0.637f;
            }
            else
            {
                _largeButtons[MenuButton.CustomTwo].gameObject.SetActive(false);
            }
        }
    }

    internal void ChangeTabPatch(GameSettingMenu menu, int tabNum, bool previewOnly)
    {
        Info($"Changed tab to {(MenuCategory)tabNum} (previewOnly: {previewOnly}) for mod {CurrentModIdx}");
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            CurrentMenu = (MenuCategory)tabNum;
            UpdateContainers();

            menu.PresetsTab.gameObject.SetActive(false);
            menu.GameSettingsTab.gameObject.SetActive(false);
            menu.RoleSettingsTab.gameObject.SetActive(false);

            menu.GamePresetsButton.SelectButton(false);
            menu.GameSettingsButton.SelectButton(false);
            menu.RoleSettingsButton.SelectButton(false);

            foreach (var (_, btn) in _largeButtons)
            {
                btn.SelectButton(false);
            }

            foreach (var (_, btn) in _smallButtons)
            {
                btn.SelectButton(false);
            }

            switch (CurrentMenu)
            {
                case MenuCategory.Preset:
                    menu.PresetsTab.gameObject.SetActive(true);
                    menu.MenuDescriptionText.text = TranslationController.Instance.GetString(StringNames.GamePresetsDescription);
                    break;
                case MenuCategory.Game:
                    menu.GameSettingsTab.gameObject.SetActive(true);
                    menu.MenuDescriptionText.text = TranslationController.Instance.GetString(StringNames.GameSettingsDescription);
                    break;
                case MenuCategory.Roles:
                    menu.RoleSettingsTab.gameObject.SetActive(true);
                    menu.RoleSettingsTab.OpenMenu(false);
                    menu.MenuDescriptionText.text = TranslationController.Instance.GetString(StringNames.RoleSettingsDescription);
                    break;

                case MenuCategory.Modifiers:
                    menu.GameSettingsTab.gameObject.SetActive(true);
                    menu.MenuDescriptionText.text = CurrentMod.MiraPlugin.ModifierMenuDescription.Translate();
                    break;
                case MenuCategory.CustomOne:
                    menu.GameSettingsTab.gameObject.SetActive(true);
                    menu.MenuDescriptionText.text = CurrentMod.MiraPlugin.CustomOptionMenuOneDescription.Translate();
                    break;
                case MenuCategory.CustomTwo:
                    menu.GameSettingsTab.gameObject.SetActive(true);
                    menu.MenuDescriptionText.text = CurrentMod.MiraPlugin.CustomOptionMenuTwoDescription.Translate();
                    break;
            }
        }

        if (previewOnly)
        {
            menu.ToggleLeftSideDarkener(false);
            menu.ToggleRightSideDarkener(true);
            return;
        }

        menu.ToggleLeftSideDarkener(true);
        menu.ToggleRightSideDarkener(false);

        CurrentMenu = (MenuCategory)tabNum;
        UpdateContainers();
        switch (CurrentMenu)
        {
            case MenuCategory.Preset:
                menu.PresetsTab.OpenMenu();
                menu.GamePresetsButton.SelectButton(true);
                break;
            case MenuCategory.Roles:
                menu.RoleSettingsTab.OpenMenu();
                SelectButton(MenuButton.Roles, MenuCategory.Roles);
                break;
            case MenuCategory.Game:
            case MenuCategory.Modifiers:
            case MenuCategory.CustomOne:
            case MenuCategory.CustomTwo:
                menu.GameSettingsButton.SelectButton(CurrentMenu == MenuCategory.Game);
                SelectButton(MenuButton.Modifiers, MenuCategory.Modifiers);
                SelectButton(MenuButton.CustomOne, MenuCategory.CustomOne);
                SelectButton(MenuButton.CustomTwo, MenuCategory.CustomTwo);
                break;
        }
    }

    private void SelectButton(MenuButton button, MenuCategory category)
    {
        if (_largeButtons[button].gameObject.activeInHierarchy)
        {
            _largeButtons[button].SelectButton(CurrentMenu == category);
        }

        if (_smallButtons[button].gameObject.activeInHierarchy)
        {
            _smallButtons[button].SelectButton(CurrentMenu == category);
        }
    }

    private MenuCategory GetNewPageIfNeeded()
    {
        if (CurrentModIdx == 0)
        {
            if (CurrentMenu >= MenuCategory.Modifiers)
            {
                return MenuCategory.Game;
            }
        }
        else
        {
            var hasOptions =
                CurrentMod.InternalOptionGroups.Exists(g =>
                    g.OptionableType == null && g.ParentMenu == MenuCategory.Game);
            var hasRoles = CurrentMod.InternalRoles.Count > 0;
            var hasModifiers = CurrentMod.InternalOptionGroups.Exists(g =>
                g.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true
                || g.ParentMenu == MenuCategory.Modifiers);
            var hasC1 = CurrentMod.InternalOptionGroups.Exists(g => g.ParentMenu == MenuCategory.CustomOne);
            var hasC2 = CurrentMod.InternalOptionGroups.Exists(g => g.ParentMenu == MenuCategory.CustomTwo);

            switch (CurrentMenu)
            {
                case MenuCategory.Game when !hasOptions:
                case MenuCategory.Roles when !hasRoles:
                case MenuCategory.Modifiers when !hasModifiers:
                case MenuCategory.CustomOne when !hasC1:
                case MenuCategory.CustomTwo when !hasC2:
                    return MenuCategory.Preset;
            }
        }

        return CurrentMenu;
    }

    public void NextMod()
    {
        CurrentModIdx++;
        // The existence of vanilla settings offsets the off-by-one errors.
        if (CurrentModIdx > ModCount) CurrentModIdx = 0;
        CurrentMenu = GetNewPageIfNeeded();
        UpdateUi();
        ChangeTabPatch(Gsm, (int)CurrentMenu, false);
    }

    public void PreviousMod()
    {
        CurrentModIdx--;
        // The existence of vanilla settings offsets the off-by-one errors.
        if (CurrentModIdx < 0) CurrentModIdx = ModCount;
        CurrentMenu = GetNewPageIfNeeded();
        UpdateUi();
        ChangeTabPatch(Gsm, (int)CurrentMenu, false);
    }
}
