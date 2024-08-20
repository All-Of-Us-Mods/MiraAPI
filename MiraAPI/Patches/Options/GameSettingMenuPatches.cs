using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatches
{
    public static int CurrentSelectedMod { get; private set; }
    
    public static MiraPluginInfo SelectedMod { get; private set; }

    private static TextMeshPro _text;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        Transform helpThing = __instance.transform.FindChild("What Is This?");
        GameObject tmpText = Object.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        _text = tmpText.GetComponent<TextMeshPro>();
        _text.fontSizeMax = 3.2f;
        UpdateText(__instance.GameSettingsTab, __instance.RoleSettingsTab);

        _text.alignment = TextAlignmentOptions.Center;

        GameObject nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        nextButton.name = "RightArrowButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        PassiveButton passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (CurrentSelectedMod != MiraPluginManager.Instance.RegisteredPlugins.Count)
            {
                CurrentSelectedMod += 1;
                UpdateText(__instance.GameSettingsTab, __instance.RoleSettingsTab);
            }
        }));

        GameObject backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX = backButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            if (CurrentSelectedMod != 0)
            {
                CurrentSelectedMod -= 1;
                UpdateText(__instance.GameSettingsTab, __instance.RoleSettingsTab);
            }
        }));
    }

    public static void UpdateText(GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        if (CurrentSelectedMod == 0)
        {
            _text.text = "Default";
            _text.fontSizeMax = 3.2f;
        }
        else
        {
            _text.fontSizeMax = 2f;
            SelectedMod = MiraPluginManager.Instance.RegisteredPlugins.ElementAt(CurrentSelectedMod - 1).Value;
            if (SelectedMod == null)
            {
                CurrentSelectedMod = 0;
                UpdateText(settings, roles);
            }

            string name = SelectedMod.PluginInfo.Metadata.Name;
            _text.text = name.Substring(0, Math.Min(name.Length, 15));
        }

        if (roles is not null && roles.roleChances is not null)
        {
            if (roles.advancedSettingChildren is not null)
            {
                foreach (var child in roles.advancedSettingChildren)
                {
                    child.gameObject.DestroyImmediate();
                }
                roles.advancedSettingChildren.Clear();
                roles.advancedSettingChildren = null;
            }

            foreach (var header in roles.RoleChancesSettings.transform.GetComponentsInChildren<CategoryHeaderEditRole>())
            {
                header.gameObject.DestroyImmediate();
            }

            foreach (var roleChance in roles.roleChances)
            {
                roleChance.gameObject.DestroyImmediate();
            }


            roles.roleChances.Clear();
            roles.roleChances = null;
            roles.AdvancedRolesSettings.gameObject.SetActive(false);
            roles.RoleChancesSettings.gameObject.SetActive(true);
            roles.SetQuotaTab();
        }

        if (settings is not null && settings.Children is not null)
        {
            foreach (var child in settings.Children)
            {
                if (child.TryCast<GameOptionsMapPicker>()) continue;
                if (child.gameObject is null) continue;

                child.gameObject.DestroyImmediate();
            }

            foreach (var header in settings.settingsContainer.GetComponentsInChildren<CategoryHeaderMasked>())
            {
                header.gameObject.DestroyImmediate();
            }

            settings.Children.Clear();
            settings.Children = null;

            settings.Initialize();
        }
    }
}
