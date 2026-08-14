using System;
using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Presets;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameOptionsMenu))]
internal static class GameOptionsMenuPatch
{
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
                __instance.CreateSettings();
                foreach (var optionBehaviour in __instance.Children)
                {
                    if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                    {
                        optionBehaviour.SetAsPlayer();
                    }

                    optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
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
        if (MenuState.Instance.CurrentModIdx == 0)
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
                var filteredGroups =
                    MenuState.Instance.CurrentMod.InternalOptionGroups
                        .Where(x => x.OptionableType == null &&
                                    x.ParentMenu == MenuCategory.Game) ?? [];

                foreach (var group in filteredGroups)
                {
                    UpdateGroup(group, ref num);
                }

                break;
        }

        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }

    private static void UpdateGroup(AbstractOptionGroup? group, ref float num)
    {
        if (group is null || !group.Ready || group.Options.Count == 0 || group.Header is null)
        {
            return;
        }

        if (!group.GroupVisible.Invoke())
        {
            group.Header.gameObject.SetActive(false);
            foreach (var option in group.Options)
            {
                option.OptionBehaviour?.gameObject.SetActive(false);
            }

            return;
        }

        group.Header.gameObject.SetActive(true);
        group.Header.transform.localScale = Vector3.one * 0.63f;
        group.Header.transform.localPosition = new Vector3(-0.903f, num, -2f);

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

            categoryHeaderMasked.SetHeader(CustomStringName.CreateAndRegister(group.GroupName), 20);
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
            newText.text = "<size=70%>(Click to close)</size>";
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
                        ? "<size=70%>(Click to open)</size>"
                        : "<size=70%>(Click to close)</size>";
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
