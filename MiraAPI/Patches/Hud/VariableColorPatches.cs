using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Colors;
using UnityEngine;

namespace MiraAPI.Patches.Hud;

[HarmonyPatch(typeof(PlayerMaterial))]
internal static class SetPlayerMaterialPatch
{
    [HarmonyPatch(nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
    public static bool Prefix(int colorId, Renderer rend)
    {
        var r = rend.gameObject.GetComponent<VariableColorBehaviour>()
             ?? rend.gameObject.AddComponent<VariableColorBehaviour>();
        r.SetColor(PaletteManager.IsVariable(colorId) ? (VariableColor)PaletteManager.ColorIdToColorMap[colorId] : null);
        return !PaletteManager.IsVariable(colorId);
    }

    [HarmonyPatch(nameof(PlayerMaterial.SetColors), typeof(Color), typeof(Renderer))]
    public static void Prefix(Renderer rend)
    {
        var r = rend.gameObject.GetComponent<VariableColorBehaviour>()
             ?? rend.gameObject.AddComponent<VariableColorBehaviour>();
        r.SetColor(null);
    }
}

[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.Update))]
internal static class PlayerTabPatch
{
    public static void Postfix(PlayerTab __instance)
    {
        for (var i = 0; i < __instance.ColorChips.Count; i++)
        {
            __instance.ColorChips[i].Inner.SpriteColor = PaletteManager.GetMainColor(i);
        }
    }
}

[HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.Update))]
internal static class ChatNotifRainbowPatch
{
    public static void Postfix(ChatNotification __instance)
    {
        if (!__instance.gameObject.active || !PaletteManager.IsVariable(__instance.player.cosmetics.ColorId))
            return;

        var str = ColorUtility.ToHtmlStringRGB(PaletteManager.GetMainColor(__instance.player.cosmetics.ColorId));
        __instance.playerNameText.text = "<color=#" + str + ">" + __instance.playerNameText.text.WithoutRichText();
    }

    private static string WithoutRichText(this string text)
    {
        var richTagRegex = new Regex(@"<[^>]*>", default, Regex.InfiniteMatchTimeout);
        return richTagRegex.Replace(text, string.Empty);
    }
}

[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.Update))]
internal static class RainbowLobbyInfoPanePatch
{
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Operator becomes too large.")]
    public static void Postfix(HostInfoPanel __instance)
    {
        if (!__instance.gameObject.activeInHierarchy || !PaletteManager.IsVariable(__instance.player.cosmetics.ColorId))
        {
            return;
        }

        var host = GameData.Instance.GetHost();
        var text = ColorUtility.ToHtmlStringRGB(PaletteManager.GetMainColor(__instance.player.cosmetics.ColorId));
        __instance.hostLabel.text =
            TranslationController.Instance.GetString(StringNames.HostNounLabel);
        if (__instance.ShouldBoldenHostLabel(DataManager.Settings.Language.CurrentLanguage))
        {
            __instance.hostLabel.text = __instance.hostLabel.text.Insert(0, "<b>");
            __instance.hostLabel.text = __instance.hostLabel.text.Insert(__instance.hostLabel.text.Length, "</b>");
        }

        if (AmongUsClient.Instance.AmHost)
        {
            __instance.playerName.text = (string.IsNullOrEmpty(host.PlayerName)
                                             ? "..."
                                             : $"<color=#{text}>{host.PlayerName}</color>")
                                         + "  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">" +
                                         TranslationController.Instance.GetString(StringNames.HostYouLabel);
        }
        else
        {
            __instance.playerName.text =
                (string.IsNullOrEmpty(host.PlayerName) ? "..." : $"<color=#{text}>{host.PlayerName}</color>") +
                " (" + __instance.player.ColorBlindName + ")";
        }
    }
}
