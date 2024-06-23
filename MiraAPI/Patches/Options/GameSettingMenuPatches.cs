using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatches
{
    public static int currentSelectedMod = 0;
    public static MiraPluginInfo selectedMod = null;
    public static TextMeshPro text;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        Transform helpThing = __instance.transform.FindChild("What Is This?");
        GameObject tmpText = GameObject.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        text = tmpText.GetComponent<TextMeshPro>();
        text.fontSizeMax = 3.2f;
        UpdateText(__instance.GameSettingsTab);

        text.alignment = TextAlignmentOptions.Center;

        GameObject nextButton = GameObject.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        nextButton.name = "RightArrowButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();
        nextButton.gameObject.GetComponent<PassiveButton>().DestroyImmediate();

        PassiveButton passiveButton = nextButton.gameObject.AddComponent<PassiveButton>();
        passiveButton.HoverSound = __instance.BackButton.GetComponent<PassiveButton>().HoverSound;
        passiveButton.ClickSound = __instance.BackButton.GetComponent<PassiveButton>().ClickSound;

        passiveButton.activeSprites = nextButton.transform.FindChild("Active").gameObject;
        passiveButton.inactiveSprites = nextButton.transform.FindChild("Inactive").gameObject;
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (currentSelectedMod != MiraPluginManager.Instance.RegisteredPlugins.Count)
            {
                currentSelectedMod += 1;
            }
            UpdateText(__instance.GameSettingsTab);
        }));

        GameObject backButton = GameObject.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX = backButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            if (currentSelectedMod != 0)
            {
                currentSelectedMod -= 1;
            }
            UpdateText(__instance.GameSettingsTab);
        }));
    }

    public static void UpdateText(GameOptionsMenu settings)
    {
        if (currentSelectedMod == 0)
        {
            text.text = "Default";
            text.fontSizeMax = 3.2f;
        }
        else
        {
            text.fontSizeMax = 2f;
            selectedMod = MiraPluginManager.Instance.RegisteredPlugins.ElementAt(currentSelectedMod - 1).Value;
            if (selectedMod == null)
            {
                currentSelectedMod = 0;
                UpdateText(settings);
            }

            string name = selectedMod.PluginInfo.Metadata.Name;
            text.text = name.Substring(0, Math.Min(name.Length, 15));
        }

        if (settings.Children is null || settings is null) return;

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
