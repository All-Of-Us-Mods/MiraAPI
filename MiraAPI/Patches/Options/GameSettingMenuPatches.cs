using HarmonyLib;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatches
{

    public static int currentSelectedMod = 0;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        Transform helpThing = __instance.transform.FindChild("What Is This?");
        GameObject tmpText = GameObject.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, -0.4019f, -2f);

        TextMeshPro tmp = tmpText.GetComponent<TextMeshPro>();
        tmp.text = "Mira API";
        tmp.fontSizeMax = 3;
        tmp.fontSize = 3;

        tmp.alignment = TextAlignmentOptions.Center;

        GameObject nextButton = GameObject.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-2.2663f, -0.4037f, -25f);
        nextButton.name = "NextButton";
        nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButtonActive.LoadAsset();
        nextButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            currentSelectedMod += 1;
        }));

        GameObject backButton = GameObject.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.transform.localPosition = new Vector3(-4.4209f, -0.4037f, -25f);
        nextButton.name = "BackButton";
        nextButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX = nextButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            currentSelectedMod -= 1;
        }));

        __instance.GamePresetsButton.gameObject.SetActive(false);
    }

    /*public static void CreateOptionsFor(GameSettingMenu __instance, ToggleOption togglePrefab, NumberOption numberPrefab, StringOption stringPrefab, Transform container,
        List<IModdedOption> options)
    {
        foreach (IModdedOption option in options)
        {
            if (option.AdvancedRole is not null || option.OptionBehaviour)
            {
                continue;
            }

            OptionBehaviour newOption = option.CreateOption(togglePrefab, numberPrefab, stringPrefab, container);
            __instance.AllItems = __instance.AllItems.AddItem(newOption.transform).ToArray();
        }
    }*/
}
