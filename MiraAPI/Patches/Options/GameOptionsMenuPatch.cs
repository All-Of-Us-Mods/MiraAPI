using System;
using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Presets;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameOptionsMenu))]
internal static class GameOptionsMenuPatch
{
    private static List<CategoryHeaderMasked> _vanillaHeaders = new();
    private static List<OptionBehaviour> _vanillaOptions = new();
    private static List<OptionBehaviour> _mainOptions = new();
    private static System.Collections.Generic.Dictionary<AbstractGameMode, System.Collections.Generic.List<AbstractOptionGroup>> _gameModeGroups = new();
    private static System.Collections.Generic.Dictionary<AbstractGameMode, System.Collections.Generic.List<OptionBehaviour>> _gameModeOptions = new();
    private static System.Collections.Generic.Dictionary<AbstractGameMode, System.Collections.Generic.List<CategoryHeaderMasked>> _gameModeHeaders = new();
    private static TextMeshPro _gamemodeDescription = null!;
    private static SpriteRenderer _modIcon = null!;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameOptionsMenu.Initialize))]
    // ReSharper disable once InconsistentNaming
    public static bool InitPatch(GameOptionsMenu __instance)
    {
        __instance.Children ??= new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
        __instance.Children.Clear();

        if (MenuState.Instance.CurrentModIdx == 0)
        {
            __instance.MapPicker.gameObject.SetActive(true);
            __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
            var container = MenuState.Instance.CurrentContainer.transform;

            if (container.childCount == 1)
            {
                __instance.MapPicker.Initialize(20);
                var mapNameSetting = GameManager.Instance.GameSettingsList.MapNameSetting;
                __instance.MapPicker.SetUpFromData(mapNameSetting, 20);
                __instance.Children.Add(__instance.MapPicker);
                CreateSettings(__instance, container);
                if (CustomGameModeManager.ActiveMode != null)
                {
                    ToggleGamemodeOptions(CustomGameModeManager.ActiveMode, __instance);
                }
            }
            else
            {
                foreach (var child in container.GetComponentsInChildren<OptionBehaviour>(true))
                {
                    __instance.Children.Add(child);
                }
            }
        }
        else
        {
            __instance.MapPicker.gameObject.SetActive(false);
            CustomCreateSettings(__instance);
        }

        return false;
    }

    public static float AdditionalVanillaScrollNum;
    internal static void ToggleGamemodeOptions(AbstractGameMode gameMode, GameOptionsMenu instance)
    {
        float num = -1.217f;
        instance.Children.Clear();
        foreach (var opt in _mainOptions)
        {
            instance.Children.Add(opt);
        }

        var icon = gameMode.Icon;
        if (icon != null)
        {
            _modIcon.gameObject.SetActive(true);
            _gamemodeDescription.transform.localPosition = new Vector3(0.3f, 0, -0.01f);
            _gamemodeDescription.horizontalAlignment = HorizontalAlignmentOptions.Left;
            var texture = icon.LoadAsset().texture;
            var scale = Mathf.Min(128f / texture.width, 128f / texture.height);

            var newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one / 2,
                100f / scale
            );

            _modIcon.sprite = newSprite;
            _modIcon.drawMode = SpriteDrawMode.Simple;
        }
        else
        {
            _gamemodeDescription.transform.localPosition = new Vector3(0f, 0, -0.01f);
            _gamemodeDescription.horizontalAlignment = HorizontalAlignmentOptions.Center;
            _modIcon.gameObject.SetActive(false);
        }

        _gamemodeDescription.text = MiraLocaleManager.Get(gameMode.Description);
        if (gameMode.ShowNormalGameSettings)
        {
            foreach (var opt in _vanillaHeaders)
            {
                num -= 0.63f;
                opt.gameObject.SetActive(true);
            }
            foreach (var opt in _vanillaOptions)
            {
                opt.gameObject.SetActive(true);
                instance.Children.Add(opt);
                num -= 0.45f;
            }
        }
        else
        {
            foreach (var opt in _vanillaHeaders)
            {
                opt.gameObject.SetActive(false);
            }
            foreach (var opt in _vanillaOptions)
            {
                opt.gameObject.SetActive(false);
            }
        }

        foreach (var pair in _gameModeGroups)
        {
            if (pair.Value.Count != 0)
            {
                if (gameMode == pair.Key)
                {
                    foreach (var opt in _gameModeHeaders[gameMode])
                    {
                        num -= 0.63f;
                        opt.gameObject.SetActive(true);
                    }

                    foreach (var opt in _gameModeOptions[gameMode])
                    {
                        opt.gameObject.SetActive(true);
                        instance.Children.Add(opt);
                        num -= 0.45f;
                    }
                }
                else
                {
                    foreach (var opt in _gameModeHeaders[pair.Key])
                    {
                        opt.gameObject.SetActive(false);
                    }

                    foreach (var opt in _gameModeOptions[pair.Key])
                    {
                        opt.gameObject.SetActive(false);
                    }
                }
            }
        }

        instance.ControllerSelectable.Clear();
        foreach (var obj in instance.scrollBar.GetComponentsInChildren<UiElement>())
        {
            if (obj.gameObject.activeSelf)
            {
                instance.ControllerSelectable.Add(obj);
            }
        }
        instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }


    private static void CreateSettings(GameOptionsMenu instance, Transform container)
    {
        float num = 0.713f;
        _vanillaHeaders.Clear();
        _vanillaOptions.Clear();
        _mainOptions.Clear();
        _gameModeHeaders.Clear();
        _gameModeOptions.Clear();
        _gameModeGroups.Clear();

        CategoryHeaderMasked gmCategory = Object.Instantiate(instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, container);
        gmCategory.SetHeader(GameModeOption.CustomName, 20);
        gmCategory.transform.localScale = Vector3.one * 0.63f;
        gmCategory.transform.localPosition = new Vector3(-0.903f, num, -2f);
        GameModeOption.OptionBehaviour = Object.Instantiate(
            instance.stringOptionOrigin,
            Vector3.zero,
            Quaternion.identity,
            container);
        num -= 0.63f;
        GameModeOption.OptionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
        GameModeOption.OptionBehaviour.SetClickMask(instance.ButtonClickMask);
        StringGameSetting setting = ScriptableObject.CreateInstance<StringGameSetting>();
        setting.Type = OptionTypes.MultipleChoice;
        setting.Title = GameModeOption.GamemodeName;
        setting.Index = GameModeOption._lastValue;
        setting.Values = new Il2CppStructArray<StringNames>([GameModeOption.Values[0]]);
        GameModeOption.OptionBehaviour.SetUpFromData(setting, 20);
        GameModeOption.Set(GameModeOption._lastValue);
        GameModeOption.OptionBehaviour.TitleText.fontSize = 3;
        instance.Children.Add(GameModeOption.OptionBehaviour);
        foreach (var optionBehaviour in instance.Children)
        {
            _mainOptions.Add(optionBehaviour);
        }
        for (var i = 1; i < GameModeOption.Values.Count; i++)
        {
            GameModeOption.OptionBehaviour.Values =
                (Il2CppStructArray<StringNames>)GameModeOption.OptionBehaviour.Values.Add(
                    GameModeOption.Values.ElementAt(i).Value);
        }

        var modeInfoHolder = new GameObject("GamemodeInfo")
        {
            transform =
            {
                parent = container,
                localPosition = new Vector3(1.2f, -0.675f, 1),
                localScale = Vector3.one,
            },
            layer = container.gameObject.layer,
        }.transform;

        var gamemodeBg = Object.Instantiate(GameModeOption.OptionBehaviour.transform.GetChild(0).gameObject, Vector3.zero, Quaternion.identity, modeInfoHolder);
        gamemodeBg.transform.localPosition = Vector3.zero;
        gamemodeBg.transform.localScale = new Vector3(1.35f, 0.85f, 1);
        gamemodeBg.name = "Background";

        var modIconObj = new GameObject("ModIcon")
        {
            transform =
            {
                parent = modeInfoHolder,
                localPosition = new Vector3(-1.75f, 0, -0.1f),
                localScale = new Vector3(0.45f, 0.45f, 1),
            },
            layer = gamemodeBg.layer,
        };
        _modIcon = modIconObj.AddComponent<SpriteRenderer>();
        _modIcon.sprite = MiraAssets.BlankSprite.LoadAsset();
        _modIcon.material = gamemodeBg.gameObject.GetComponent<SpriteRenderer>().material;

        var gamemodeTextObj = Object.Instantiate(GameModeOption.OptionBehaviour.transform.GetChild(1).gameObject, Vector3.zero, Quaternion.identity, modeInfoHolder);
        gamemodeTextObj.transform.localPosition = new Vector3(0.3f, 0, -0.01f);
        gamemodeTextObj.transform.localScale = new Vector3(2.12f, 2.12f, 1);
        _gamemodeDescription = gamemodeTextObj.GetComponentInChildren<TextMeshPro>();
        _gamemodeDescription.fontSizeMin = 0.65f;
        _gamemodeDescription.fontSizeMax = 0.9f;
        _gamemodeDescription.alignment = TextAlignmentOptions.Left;

        num -= 1.3f;
        AdditionalVanillaScrollNum = 0f;
        foreach (RulesCategory rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
        {
            CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate<CategoryHeaderMasked>(
                instance.categoryHeaderOrigin,
                Vector3.zero,
                Quaternion.identity,
                container);
            categoryHeaderMasked.SetHeader(rulesCategory.CategoryName, 20);
            categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
            categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
            _vanillaHeaders.Add(categoryHeaderMasked);
            num -= 0.63f;
            AdditionalVanillaScrollNum -= 0.63f;
            foreach (BaseGameSetting baseGameSetting in rulesCategory.AllGameSettings)
            {
                switch (baseGameSetting.Type)
                {
                    case OptionTypes.Checkbox:
                    {
                        OptionBehaviour optionBehaviour = Object.Instantiate<ToggleOption>(
                            instance.checkboxOrigin,
                            Vector3.zero,
                            Quaternion.identity,
                            container);
                        optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                        optionBehaviour.SetClickMask(instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        _vanillaOptions.Add(optionBehaviour);
                        break;
                    }
                    case OptionTypes.String:
                    {
                        OptionBehaviour optionBehaviour = Object.Instantiate<StringOption>(
                            instance.stringOptionOrigin,
                            Vector3.zero,
                            Quaternion.identity,
                            container);
                        optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                        optionBehaviour.SetClickMask(instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        _vanillaOptions.Add(optionBehaviour);
                        break;
                    }
                    case OptionTypes.Float:
                    case OptionTypes.Int:
                    {
                        OptionBehaviour optionBehaviour = Object.Instantiate<NumberOption>(
                            instance.numberOptionOrigin,
                            Vector3.zero,
                            Quaternion.identity,
                            container);
                        optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                        optionBehaviour.SetClickMask(instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        _vanillaOptions.Add(optionBehaviour);
                        break;
                    }
                    case OptionTypes.Player:
                    {
                        OptionBehaviour optionBehaviour = Object.Instantiate<PlayerOption>(
                            instance.playerOptionOrigin,
                            Vector3.zero,
                            Quaternion.identity,
                            container);
                        optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                        optionBehaviour.SetClickMask(instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        _vanillaOptions.Add(optionBehaviour);
                        break;
                    }
                }

                num -= 0.45f;
                AdditionalVanillaScrollNum -= 0.45f;
            }
        }
        foreach (var optionBehaviour in _vanillaOptions)
        {
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
            instance.Children.Add(optionBehaviour);
        }

        foreach (var optionBehaviour in instance.Children)
        {
            optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(instance.ValueChanged);
        }

        foreach (var mode in CustomGameModeManager.IdToModeMap.Values)
        {
            var filteredGroups =
                ModdedOptionsManager.GameModeOptionGroups
                    .Where(x => x.Key == mode.GetType()).Select(y => y.Value).SelectMany(y => y) ?? [];
            _gameModeGroups.Add(mode, filteredGroups.ToList());

            var optionBehaviours = new System.Collections.Generic.List<OptionBehaviour>();
            var categoryHeaders = new System.Collections.Generic.List<CategoryHeaderMasked>();
            var newNum = mode.ShowNormalGameSettings ? num : num - AdditionalVanillaScrollNum;
            foreach (var group in filteredGroups)
            {
                CreateGroup(instance, group, container, ref newNum, ref optionBehaviours, ref categoryHeaders);
                group.Ready = true;
            }
            _gameModeHeaders.Add(mode, categoryHeaders);
            _gameModeOptions.Add(mode, optionBehaviours);
        }

        instance.ControllerSelectable.Clear();
        foreach (var obj in instance.scrollBar.GetComponentsInChildren<UiElement>())
        {
            instance.ControllerSelectable.Add(obj);
        }
        instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameOptionsMenu.OnEnable))]
    public static void OpenPatch()
    {
        HudManager.Instance.PlayerCam.OverrideScreenShakeEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameOptionsMenu.OnDisable))]
    public static void ClosePatch()
    {
        HudManager.Instance.PlayerCam.OverrideScreenShakeEnabled = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameOptionsMenu.Update))]
    // ReSharper disable once InconsistentNaming
    public static void UpdatePatch(GameOptionsMenu __instance)
    {
        if (MenuState.Instance.CurrentModIdx == 0 && _gameModeGroups.Count <= 0)
        {
            return;
        }
        var num = 2.1f;

        switch (MenuState.Instance.CurrentMenu)
        {
            case MenuCategory.Modifiers:
                ModifiersUpdate(ref num);
                break;
            case MenuCategory.CustomOne:
                CustomMenuOneUpdate(ref num);
                break;
            case MenuCategory.CustomTwo:
                CustomMenuTwoUpdate(ref num);
                break;
            default:
                if (MenuState.Instance.CurrentModIdx == 0)
                {
                    num = -1.217f;
                    GamemodeOptionsUpdate(ref num);
                    break;
                }
                var filteredGroups =
                    MenuState.Instance.CurrentMod.InternalOptionGroups
                        .Where(x => (x.OptionableType == null || CustomGameModeManager.ActiveMode!.GetType().IsAssignableTo(x.OptionableType)) &&
                                    x.ParentMenu == MenuCategory.Game) ?? [];

                foreach (var group in filteredGroups)
                {
                    UpdateGroup(group, ref num);
                }

                break;
        }

        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }
    private static void CreateGroup(GameOptionsMenu menu, AbstractOptionGroup group, Transform container, ref float num, ref System.Collections.Generic.List<OptionBehaviour> optionBehaviours, ref System.Collections.Generic.List<CategoryHeaderMasked> categoryHeaders)
    {
        var categoryHeaderMasked = Object.Instantiate(
            menu.categoryHeaderOrigin,
            Vector3.zero,
            Quaternion.identity,
            container);
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num + 1, -2f);
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaders.Add(categoryHeaderMasked);

        num -= 0.58f;

        categoryHeaderMasked.SetHeader(MiraLocaleManager.GetOrCreateLocaleString(group.GroupName), 20);
        categoryHeaderMasked.Background.color = group.GroupColor;
        categoryHeaderMasked.Divider.color = group.GroupColor;
        categoryHeaderMasked.Title.color = group.GroupColor.Equals(MiraApiPlugin.DefaultHeaderColor)
            ? Color.white
            : group.GroupColor.FindAlternateColor();

        categoryHeaderMasked.Background.sprite = MiraAssets.CategoryHeader.LoadAsset();
        categoryHeaderMasked.Background.sprite.texture.filterMode = FilterMode.Bilinear;
        categoryHeaderMasked.Background.sprite.texture.wrapMode = TextureWrapMode.Clamp;

        categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.5f, -0.1833f, 0);
        categoryHeaderMasked.Background.size = new Vector2(
            categoryHeaderMasked.Background.size.x + 1.5f,
            categoryHeaderMasked.Background.size.y);

        categoryHeaderMasked.gameObject.SetActive(true);
        group.Header = categoryHeaderMasked;
        categoryHeaderMasked.transform.localPosition += Vector3.down;

        var options = group.Options.Select(opt => opt.CreateOption(
            menu.checkboxOrigin,
            menu.numberOptionOrigin,
            menu.stringOptionOrigin,
            menu.playerOptionOrigin,
            container));

        foreach (var newOpt in options)
        {
            newOpt.SetClickMask(menu.ButtonClickMask);
            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in componentsInChildren)
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    renderer.color = group.GroupColor.FindAlternateColor();
                    if (renderer.transform.parent.TryGetComponent<GameOptionButton>(out var btn))
                    {
                        btn.interactableColor = group.GroupColor.FindAlternateColor();
                        btn.interactableHoveredColor = Color.white;
                    }
                }

                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }

            foreach (var textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    textMeshPro.color = group.GroupColor;
                }

                textMeshPro.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                textMeshPro.fontMaterial.SetFloat(ShaderID.Stencil, 20);
            }

            if (newOpt is ToggleOption toggle)
            {
                toggle.CheckMark.sprite = MiraAssets.Checkmark.LoadAsset();
                toggle.CheckMark.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor
                    ? group.GroupColor
                    : MiraAssets.AcceptedTeal;
                var rend = toggle.CheckMark.transform.parent.FindChild("ActiveSprite")
                    .GetComponent<SpriteRenderer>();
                rend.sprite = MiraAssets.CheckmarkBox.LoadAsset();
                rend.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor
                    ? group.GroupColor
                    : MiraAssets.AcceptedTeal;
            }

            optionBehaviours.Add(newOpt);
            newOpt.transform.localPosition = new Vector3(0.952f, num, -2f);
            num -= 0.45f;

            newOpt.Initialize();
            newOpt.gameObject.SetActive(true);
        }
    }

    internal static void UpdateGroup(AbstractOptionGroup? group, ref float num)
    {
        if (group is null || !group.Ready || group.Options.Count == 0 || group.Header == null)
        {
            return;
        }

        if (!group.GroupVisible.Invoke())
        {
            if (group.Header != null && group.Header.gameObject != null)
                group.Header.gameObject.SetActive(false);
            foreach (var option in group.Options)
            {
                if (option.OptionBehaviour != null && option.OptionBehaviour.gameObject != null)
                    option.OptionBehaviour?.gameObject.SetActive(false);
            }

            return;
        }

        if (group.Header != null)
        {
            if (group.Header.gameObject != null)
                group.Header.gameObject.SetActive(true);
            group.Header.transform.localScale = Vector3.one * 0.63f;
            group.Header.transform.localPosition = new Vector3(-0.903f, num, -2f);
        }

        num -= 0.58f;

        foreach (var opt in group.Options)
        {
            var newOpt = opt.OptionBehaviour;

            if (!newOpt || newOpt == null)
            {
                continue;
            }

            if (!opt.Visible.Invoke())
            {
                newOpt.gameObject.SetActive(false);
                continue;
            }

            if (!group.AllOptionsHidden)
            {
                newOpt.gameObject.SetActive(true);
                newOpt.transform.localPosition = new Vector3(0.952f, num, -2f);
                num -= 0.45f;
            }
            else
            {
                newOpt.gameObject.SetActive(false);
            }
        }
    }

    private static void GamemodeOptionsUpdate(ref float num)
    {
        var mode = CustomGameModeManager.ActiveMode;
        if (mode == null) return;

        if (mode.ShowNormalGameSettings)
        {
            num += AdditionalVanillaScrollNum;
        }

        if (!_gameModeGroups.TryGetValue(mode, out var groups) || groups.Count == 0)
        {
            return;
        }

        foreach (var group in groups)
        {
            UpdateGroup(group, ref num);
        }
    }
    private static void CustomMenuOneUpdate(ref float num)
    {
        var groups = MenuState.Instance.CurrentMod.InternalOptionGroups
            .Where(x => x.ParentMenu == MenuCategory.CustomOne);

        foreach (var modGroup in groups)
        {
            UpdateGroup(modGroup, ref num);
        }
    }

    private static void CustomMenuTwoUpdate(ref float num)
    {
        var groups = MenuState.Instance.CurrentMod.InternalOptionGroups
            .Where(x => x.ParentMenu == MenuCategory.CustomTwo);

        foreach (var modGroup in groups)
        {
            UpdateGroup(modGroup, ref num);
        }
    }

    private static void ModifiersUpdate(ref float num)
    {
        var groups = MenuState.Instance.CurrentMod.InternalOptionGroups
            .Where(x => x.ParentMenu is MenuCategory.Modifiers ||
                        x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true);

        foreach (var modGroup in groups)
        {
            UpdateGroup(modGroup, ref num);
        }
    }

    private static Coroutine? _creationCoroutine;

    public static void CustomCreateSettings(GameOptionsMenu menu)
    {
        menu.MapPicker.gameObject.SetActive(false);
        var mod = MenuState.Instance.CurrentMod;

        var filteredGroups = MenuState.Instance.CurrentMenu switch
        {
            MenuCategory.Game => mod.InternalOptionGroups.Where(x =>
                x.OptionableType == null && x.ParentMenu == MenuCategory.Game),
            MenuCategory.Modifiers => mod.InternalOptionGroups.Where(x =>
                x.ParentMenu is MenuCategory.Modifiers ||
                x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true),
            MenuCategory.CustomOne => mod.InternalOptionGroups.Where(x => x.ParentMenu == MenuCategory.CustomOne),
            MenuCategory.CustomTwo => mod.InternalOptionGroups.Where(x => x.ParentMenu == MenuCategory.CustomTwo),
            _ => [],
        };

        if (_creationCoroutine != null)
        {
            GameSettingMenu.Instance.StopCoroutine(_creationCoroutine);
        }

        _creationCoroutine = GameSettingMenu.Instance.StartCoroutine(CreateGroups().WrapToIl2Cpp());

        IEnumerator CreateGroups()
        {
            foreach (var group in filteredGroups)
            {
                yield return CoCreateGroup(menu, group);
            }
        }
    }

    private static IEnumerator CoCreateGroup(GameOptionsMenu menu, AbstractOptionGroup group)
    {
        var container = MenuState.Instance.CurrentContainer.transform;

        group.Ready = false;
        if (group.Header == null || !group.Header)
        {
            var categoryHeaderMasked = Object.Instantiate(
                menu.categoryHeaderOrigin,
                Vector3.zero,
                Quaternion.identity,
                container);

            categoryHeaderMasked.SetHeader(MiraLocaleManager.GetOrCreateLocaleString(group.GroupName), 20);
            categoryHeaderMasked.Background.color = group.GroupColor;
            categoryHeaderMasked.Divider.color = group.GroupColor;
            categoryHeaderMasked.Title.color = group.GroupColor.Equals(MiraApiPlugin.DefaultHeaderColor)
                ? Color.white
                : group.GroupColor.FindAlternateColor();

            categoryHeaderMasked.Background.sprite = MiraAssets.CategoryHeader.LoadAsset();
            categoryHeaderMasked.Background.sprite.texture.filterMode = FilterMode.Bilinear;
            categoryHeaderMasked.Background.sprite.texture.wrapMode = TextureWrapMode.Clamp;

            categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.5f, -0.1833f, 0);
            categoryHeaderMasked.Background.size = new Vector2(
                categoryHeaderMasked.Background.size.x + 1.5f,
                categoryHeaderMasked.Background.size.y);

            categoryHeaderMasked.gameObject.SetActive(false);

            var newText = Object.Instantiate(categoryHeaderMasked.Title, categoryHeaderMasked.transform);
            newText.text = $"<size=70%>({"MiraApi.GameSetting.Global.ClickToClose".Translate()})</size>";
            newText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);
            newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

            var boxCol = categoryHeaderMasked.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(7, 0.7f);
            boxCol.offset = new Vector2(1.5f, -0.3f);

            var headerBtn = categoryHeaderMasked.gameObject.AddComponent<PassiveButton>();
            headerBtn.ClickSound = menu.BackButton.GetComponent<PassiveButton>().ClickSound;
            headerBtn.OnMouseOver = new UnityEvent();
            headerBtn.OnMouseOut = new UnityEvent();
            headerBtn.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    group.AllOptionsHidden = !group.AllOptionsHidden;
                    newText.text = group.AllOptionsHidden
                        ? $"<size=70%>({"MiraApi.GameSetting.Global.ClickToOpen".Translate()})</size>"
                        : $"<size=70%>({"MiraApi.GameSetting.Global.ClickToClose".Translate()})</size>";
                }));
            headerBtn.SetButtonEnableState(true);

            group.Header = categoryHeaderMasked;
        }

        var options = group.Options.Where(opt => opt.OptionBehaviour == null || !opt.OptionBehaviour
        ).Select(opt => opt.CreateOption(
            menu.checkboxOrigin,
            menu.numberOptionOrigin,
            menu.stringOptionOrigin,
            menu.playerOptionOrigin,
            container)
        );

        OptionPreset? defaultPreset = null;
        if (PresetManager.DefaultPresets.TryGetValue(MenuState.Instance.CurrentMod, out var preset))
        {
            defaultPreset = preset;
        }

        yield return options.CoLoopWithBudget(newOpt =>
        {
            newOpt.SetClickMask(menu.ButtonClickMask);

            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in componentsInChildren)
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    renderer.color = group.GroupColor.FindAlternateColor();
                    if (renderer.transform.parent.TryGetComponent<GameOptionButton>(out var btn))
                    {
                        btn.interactableColor = group.GroupColor.FindAlternateColor();
                        btn.interactableHoveredColor = Color.white;
                    }
                }

                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }

            foreach (var textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
            {
                if (group.GroupColor != MiraApiPlugin.DefaultHeaderColor)
                {
                    textMeshPro.color = group.GroupColor;
                }

                textMeshPro.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                textMeshPro.fontMaterial.SetFloat(ShaderID.Stencil, 20);
            }

            if (newOpt is ToggleOption toggle)
            {
                toggle.CheckMark.sprite = MiraAssets.Checkmark.LoadAsset();
                toggle.CheckMark.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor
                    ? group.GroupColor
                    : MiraAssets.AcceptedTeal;
                var rend = toggle.CheckMark.transform.parent.FindChild("ActiveSprite")
                    .GetComponent<SpriteRenderer>();
                rend.sprite = MiraAssets.CheckmarkBox.LoadAsset();
                rend.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor
                    ? group.GroupColor
                    : MiraAssets.AcceptedTeal;
            }

            menu.Children.Add(newOpt);
            var resetBtn = new GameObject("ResetOption");
            resetBtn.transform.parent = newOpt.transform;
            resetBtn.transform.localScale = new(.5f, .5f, 1);
            resetBtn.layer = LayerMask.NameToLayer("UI");
            resetBtn.transform.localPosition = new Vector3(-3.1f, 0f, -2f);
            var resetRend = resetBtn.AddComponent<SpriteRenderer>();
            resetRend.sprite = MiraAssets.ResetButton.LoadAsset();
            resetRend.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            resetRend.color = group.GroupColor.Equals(MiraApiPlugin.DefaultHeaderColor)
                ? Color.white
                : group.GroupColor.FindAlternateColor();
            var resetBoxCol = resetBtn.gameObject.AddComponent<BoxCollider2D>();
            resetBoxCol.size = new Vector2(1f, 1f);
            resetBoxCol.offset = new Vector2(0, 0);
            var passiveButton = resetBtn.AddComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.ClickSound = menu.BackButton.GetComponent<PassiveButton>().ClickSound;
            passiveButton.OnMouseOver = new UnityEvent();
            passiveButton.OnMouseOver.AddListener(
                (UnityAction)(() =>
                {
                    resetRend.color = group.GroupColor != MiraApiPlugin.DefaultHeaderColor
                        ? group.GroupColor
                        : MiraAssets.AcceptedTeal;
                }));
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOut.AddListener(
                (UnityAction)(() =>
                {
                    resetRend.color = group.GroupColor.Equals(MiraApiPlugin.DefaultHeaderColor)
                        ? Color.white
                        : group.GroupColor.FindAlternateColor();
                }));
            if (newOpt is ToggleOption toggleOpt)
            {
                passiveButton.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        defaultPreset!.ResetOption(toggleOpt);
                    }));
            }
            else if (newOpt is NumberOption numOpt)
            {
                passiveButton.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        defaultPreset!.ResetOption(numOpt);
                    }));
            }
            else if (newOpt is StringOption strOpt)
            {
                passiveButton.OnClick.AddListener(
                    (UnityAction)(() =>
                    {
                        defaultPreset!.ResetOption(strOpt);
                    }));
            }

            if (!defaultPreset!.IsOptionInPreset(newOpt))
            {
                resetBtn.Destroy();
            }

            newOpt.Initialize();
            newOpt.gameObject.SetActive(false);

            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                newOpt.SetAsPlayer();
            }
        }).WrapToIl2Cpp();
        group.Ready = true;
    }
}
