using System.Collections;
using HarmonyLib;
using Il2CppSystem;
using MiraAPI.GameModes;
using MiraAPI.Translation;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(HudManager))]
internal static class HudPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(HudManager.Update))]
    public static void HudUpdatePatch(HudManager __instance)
    {
        if (GameManager.Instance != null && GameManager.Instance.GameHasStarted &&
            CustomGameModeManager.ActiveMode != null)
        {
            CustomGameModeManager.ActiveMode.HudUpdate(__instance);
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void PostHudStart(HudManager __instance)
    {
        Coroutines.Start(CoPostHudStart(__instance));
    }

    private static IEnumerator CoPostHudStart(HudManager instance)
    {
        yield return new WaitUntil((Func<bool>)(() => GameManager.Instance != null));
        if (GameManager.Instance.IsHideAndSeek())
            yield return null;
        var infoPane = instance.gameObject.transform.FindChild("LobbyInfoPane");
        var aspect = infoPane.gameObject.transform.Find("AspectSize");
        var modeLabel = aspect!.Find("ModeLabel");
        var modeValue = aspect.Find("ModeValue");
        var modelText = modeLabel!.Find("Text_TMP").gameObject;
        var modelTextClone = Object.Instantiate(modelText, modeLabel);
        Object.Destroy(modelTextClone.GetComponent<TextTranslatorTMP>());
        modelTextClone.GetComponent<TextMeshPro>().text = "Gamemode";
        var gmText = modeValue!.Find("GameModeText").gameObject;
        var gmTextClone = Object.Instantiate(gmText, modeValue);
        gmText.SetActive(false);
        modelText.SetActive(false);
        _text = gmTextClone.GetComponent<TextMeshPro>();
        _text.text = CustomGameModeManager.ActiveMode != null ? CustomGameModeManager.ActiveMode.ColoredName : MiraLocaleManager.Get("MiraApi.Gamemode.Classic");
        CustomGameModeManager.ActiveMode?.HudStart(instance);
    }

    private static TextMeshPro? _text;
    internal static void SetGameModeText(string text)
    {
        if (_text != null)
            _text.text = text;
    }
}
