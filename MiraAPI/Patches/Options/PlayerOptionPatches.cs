using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(PlayerOption))]
public static class PlayerOptionPatches
{
    // Updates the list of players according to the filter.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerOption.FixedUpdate))]
    public static bool PlayerOptionUpdatePatch(PlayerOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        if (!ModdedOptionsManager.CreatedPlayerOptions.TryGetValue(__instance, out var moddedPlrOpt)) return false;

        var filteredPlayers = moddedPlrOpt.GetFilteredPlayers();
        if (filteredPlayers.Count != __instance.Values.Count)
        {
            filteredPlayers.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

            moddedPlrOpt.Values = filteredPlayers;
            __instance.Values = filteredPlayers.ToIl2CppList();
            __instance.playerIndex = filteredPlayers.FindIndex(p => p.PlayerId == __instance.Value);
            __instance.SetValueText();
        }

        __instance.CheckValueChanged();

        return false;
    }

    // Disable going under 0 if you aren't allowed to choose None.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerOption.UpdatePlayerIndex))]
    public static bool UpdatePlayerIndexPatch(PlayerOption __instance, int index)
    {
        if (!__instance.IsCustom() || ModdedOptionsManager.CreatedPlayerOptions[__instance].AllowNone)
        {
            return true;
        }

        var newIndex = Mathf.Clamp(index, 0, __instance.Values.Count - 1);
        __instance.playerIndex = newIndex;
        __instance.Value = __instance.Values[newIndex].PlayerId;
        return false;
    }

    // Replace "Round Robin" with "None" in the Host Options Menu.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerOption.SetValueText))]
    public static void SetValueTextPatch(PlayerOption __instance)
    {
        if (__instance.playerIndex < 0)
        {
            __instance.ValueText.text = TranslationController.Instance.GetString(StringNames.FilterNone);
        }
    }

    // Innersloth hardcode literally replaces the value with the HNS impostor option OnEnable, so I have to patch that out. Smh.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerOption.OnEnable))]
    public static bool OnEnablePatch(PlayerOption __instance)
    {
        return !__instance.IsCustom();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerOption.GetInt))]
    public static bool GetIntPatch(PlayerOption __instance, ref int __result)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __result = __instance.Value;
        return false;
    }

    // Used to replace "Round Robin" with "None" for option notification & view pane.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerSelectionGameSetting), nameof(PlayerSelectionGameSetting.GetValueString))]
    public static bool GetStringPatch(PlayerSelectionGameSetting __instance, float value, ref string __result)
    {
        var option = ModdedOptionsManager.ModdedOptions.FirstOrDefault(x => x.Value.Data == __instance).Value;
        if (option == null) return true;

        var networkedPlayerInfo = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(p => p.PlayerId == (int)value);
        if (networkedPlayerInfo != null)
        {
            __result = networkedPlayerInfo.PlayerName;
            return false;
        }

        __result = TranslationController.Instance.GetString(StringNames.FilterNone);
        return false;
    }

    // "UpdateValue" was inlined and I needed to replace it, so I just rewrote the method where it's called from.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerOption.CheckValueChanged))]
    public static bool UpdateValuePatch(PlayerOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        if (__instance.oldValue == __instance.Value) return false;

        __instance.oldValue = __instance.Value;
        __instance.SetValueText();

        return false;
    }
}
