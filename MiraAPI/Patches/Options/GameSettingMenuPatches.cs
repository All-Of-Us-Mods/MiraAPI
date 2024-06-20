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
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        Transform helpThing = __instance.transform.FindChild("What Is This?");
        GameObject tmpText = GameObject.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        TextMeshPro tmp = tmpText.GetComponent<TextMeshPro>();
        tmp.text = "<b>Default</b>";
        tmp.fontSizeMax = 3.2f;

        tmp.alignment = TextAlignmentOptions.Center;

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
            currentSelectedMod += 1;
        }));

        GameObject backButton = GameObject.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX = backButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            currentSelectedMod -= 1;
        }));
    }
}
