using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Patches.Options;


[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameOptionsMenu.Update))]
    public static void UpdatePatch(GameOptionsMenu __instance)
    {
        if (GameSettingMenuPatches.CurrentSelectedMod == 0)
        {
            return;
        }

        float num = 2.1f;
        var filteredGroups = GameSettingMenuPatches.SelectedMod.OptionGroups.Where(x => x.GroupVisible.Invoke() && x.AdvancedRole is null);

        foreach (AbstractOptionGroup group in filteredGroups)
        {
            var filteredOpts = group.Options.Where(x => x.Visible.Invoke()).ToList();
            if (filteredOpts.Count == 0)
            {
                continue;
            }

            group.Header.gameObject.SetActive(true);
            group.Header.transform.localScale = Vector3.one * 0.63f;
            group.Header.transform.localPosition = new Vector3(-0.903f, num, -2f);

            num -= 0.58f;

            foreach (var opt in group.Options)
            {
                OptionBehaviour newOpt = opt.OptionBehaviour;

                if (opt.Visible.Invoke() == false)
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

        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);

    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    public static bool SettingsPatch(GameOptionsMenu __instance)
    {
        if (GameSettingMenuPatches.CurrentSelectedMod == 0)
        {
            return true;
        }

        __instance.MapPicker.gameObject.SetActive(false);

        var filteredGroups = GameSettingMenuPatches.SelectedMod.OptionGroups.Where(x => x.AdvancedRole is null);

        foreach (AbstractOptionGroup group in filteredGroups)
        {
            CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
            categoryHeaderMasked.SetHeader(CustomStringName.CreateAndRegister(group.GroupName), 20);
            if (group.GroupColor != Color.clear)
            {
                categoryHeaderMasked.Background.color = group.GroupColor;
                categoryHeaderMasked.Divider.color = group.GroupColor;
                categoryHeaderMasked.Title.color = group.GroupColor.GetAlternateColor();
            }
            categoryHeaderMasked.Background.size = new Vector2(categoryHeaderMasked.Background.size.x + 1.5f, categoryHeaderMasked.Background.size.y);
            categoryHeaderMasked.gameObject.SetActive(false);
            group.Header = categoryHeaderMasked;

            TextMeshPro newText = GameObject.Instantiate(categoryHeaderMasked.Title, categoryHeaderMasked.transform);
            newText.text = $"<size=70%>(Click to open)</size>";
            newText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);
            newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

            List<OptionBehaviour> newOpts = new();
            foreach (var opt in group.Options)
            {
                OptionBehaviour newOpt = opt.CreateOption(__instance.checkboxOrigin, __instance.numberOptionOrigin, __instance.stringOptionOrigin, __instance.settingsContainer);
                newOpt.SetClickMask(__instance.ButtonClickMask);

                SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    if (group.GroupColor != Color.clear)
                    {
                        componentsInChildren[i].color = group.GroupColor.GetAlternateColor();
                        if (componentsInChildren[i].transform.parent.TryGetComponent<GameOptionButton>(out var btn))
                        {
                            btn.interactableColor = group.GroupColor.GetAlternateColor();
                            btn.interactableHoveredColor = Color.white;
                        }
                    }

                    componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
                }

                foreach (TextMeshPro textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
                {
                    if (group.GroupColor != Color.clear)
                    {
                        textMeshPro.color = group.GroupColor;
                    }

                    textMeshPro.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                    textMeshPro.fontMaterial.SetFloat(ShaderID.Stencil, 20);
                }

                if (newOpt is ToggleOption toggle)
                {
                    toggle.CheckMark.sprite = MiraAssets.Checkmark.LoadAsset();

                    SpriteRenderer rend = toggle.CheckMark.transform.parent.FindChild("ActiveSprite").GetComponent<SpriteRenderer>();
                    rend.sprite = MiraAssets.CheckmarkBox.LoadAsset();
                    rend.color = group.GroupColor;
                }

                __instance.Children.Add(newOpt);

                newOpt.Initialize();
                newOpts.Add(newOpt);
                newOpt.gameObject.SetActive(false);
            }

            BoxCollider2D boxCol = categoryHeaderMasked.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(7, 0.7f);
            boxCol.offset = new Vector2(1.5f, -0.3f);

            PassiveButton headerBtn = categoryHeaderMasked.gameObject.AddComponent<PassiveButton>();
            headerBtn.ClickSound = __instance.BackButton.GetComponent<PassiveButton>().ClickSound;
            headerBtn.OnMouseOver = new UnityEvent();
            headerBtn.OnMouseOut = new UnityEvent();
            headerBtn.OnClick.AddListener((UnityAction)(() =>
            {
                group.AllOptionsHidden = !group.AllOptionsHidden;
                newText.text = group.AllOptionsHidden ? "<size=70%>(Click to open)</size>" : "<size=70%>(Click to close)</size>";
            }));
            headerBtn.SetButtonEnableState(true);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameOptionsMenu.Initialize))]
    public static bool InitPatch(GameOptionsMenu __instance)
    {
        if (__instance.Children == null || __instance.Children.Count == 0)
        {
            __instance.MapPicker.gameObject.SetActive(true);
            __instance.MapPicker.Initialize(20);
            BaseGameSetting mapNameSetting = GameManager.Instance.GameSettingsList.MapNameSetting;
            __instance.MapPicker.SetUpFromData(mapNameSetting, 20);
            __instance.Children = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
            __instance.Children.Add(__instance.MapPicker);
            __instance.CreateSettings();
            __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
            foreach (var optionBehaviour in __instance.Children)
            {
                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                {
                    optionBehaviour.SetAsPlayer();
                }

                if (optionBehaviour.IsCustom())
                {
                    continue;
                }

                optionBehaviour.OnValueChanged = new System.Action<OptionBehaviour>(__instance.ValueChanged);
            }
        }

        return false;
    }
}