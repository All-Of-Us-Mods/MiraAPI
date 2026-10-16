using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.Colors;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Patches.Colors;

[HarmonyPatch(typeof(PlayerMaterial))]
internal static class SetPlayerMaterialPatch
{
    [HarmonyPatch(nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
    public static bool Prefix(int colorId, Renderer rend)
    {
        rend.gameObject.GetOrAddComponent<VariableColorBehaviour>().SetColor(
            PaletteManager.TryGetCustomColor(colorId, out var color) && color is VariableColor variable
                ? variable
                : null);
        return !PaletteManager.IsVariable(colorId);
    }

    [HarmonyPatch(nameof(PlayerMaterial.SetColors), typeof(Color), typeof(Renderer))]
    public static void Prefix(Renderer rend)
    {
        rend.gameObject.GetOrAddComponent<VariableColorBehaviour>().SetColor(null);
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
    private static readonly Regex RichTags = new(@"<[^>]*>", default, Regex.InfiniteMatchTimeout);

    public static void Postfix(ChatNotification __instance)
    {
        if (!__instance.gameObject.active)
            return;

        var colorId = __instance.player.cosmetics.ColorId;

        if (!PaletteManager.IsVariable(colorId))
            return;

        var color = PaletteManager.GetMainColor(colorId);
        var str = color.ToHtmlStringRGBA();
        var nameText = __instance.playerNameText;
        nameText.text = "<color=#" + str + ">" + RichTags.Replace(nameText.text, string.Empty);
        nameText.outlineColor = color.GetShadowColor(0.5f);
    }
}

[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.Update))]
internal static class RainbowLobbyInfoPanePatch
{
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Operator becomes too large.")]
    public static void Postfix(HostInfoPanel __instance)
    {
        if (!__instance.gameObject.activeInHierarchy)
            return;

        var colorId = __instance.player.cosmetics.ColorId;

        if (!PaletteManager.IsVariable(colorId))
            return;

        var host = GameData.Instance.GetHost();

        if (!host)
            return;

        var text = PaletteManager.GetMainColor(colorId).ToHtmlStringRGBA();
        var label = __instance.hostLabel;
        label.text = TranslationController.Instance.GetString(StringNames.HostNounLabel);

        if (__instance.ShouldBoldenHostLabel(DataManager.Settings.Language.CurrentLanguage))
        {
            label.text = "<b>" + label.text + "</b>";
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
