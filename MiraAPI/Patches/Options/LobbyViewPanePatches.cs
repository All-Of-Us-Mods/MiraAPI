using HarmonyLib;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewPanePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.transform.localPosition = new Vector3(-2.3f, 2.4f, -2);

        // Create the next button
        GameObject nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.3f);
        nextButton.transform.localPosition = new Vector3(-5.4f, 2.4f, -2f);
        nextButton.transform.localScale = new Vector3(3, 3, 2);
        nextButton.name = "RightArrowButton";
        
        var normal = nextButton.transform.FindChild("Normal").GetComponentInChildren<SpriteRenderer>();
        normal.transform.localPosition = new Vector3(0, 0f, 0.3f);
        normal.sprite = MiraAssets.NextButton.LoadAsset();
        
        var hover = nextButton.transform.FindChild("Hover").GetComponentInChildren<SpriteRenderer>();
        hover.transform.localPosition = new Vector3(0, 0f, 0.3f);
        hover.sprite = MiraAssets.NextButtonActive.LoadAsset();
        
        PassiveButton passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
        }));

        // Create the back button
        GameObject backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-6.3f, 2.4f, -2f);
        backButton.name = "LeftArrowButton";
        backButton.transform.FindChild("Normal").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
            = backButton.transform.FindChild("Hover").gameObject.GetComponentInChildren<SpriteRenderer>().flipX 
                = true;
        
        PassiveButton passiveButton2 = backButton.gameObject.GetComponent<PassiveButton>();
        passiveButton2.OnClick = new Button.ButtonClickedEvent();
        passiveButton2.OnClick.AddListener((UnityAction)(() =>
        {
        }));
    }
}