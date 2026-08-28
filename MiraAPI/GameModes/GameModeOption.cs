using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Patches.GameModes;
using MiraAPI.Patches.Options;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace MiraAPI.GameModes;

/// <summary>
/// The game mode option.
/// </summary>
[HarmonyPatch]
public static class GameModeOption
{
    /// <summary>
    /// Gets the current index of the Game Mode Option
    /// For the value as an <see cref="AbstractGameMode"/>, see <see cref="CustomGameModeManager.ActiveMode"/>.
    /// </summary>
    public static int Value
    {
        get =>
            OptionBehaviour != null
                ? OptionBehaviour.GetInt()
                : _lastValue;
        private set
        {
            _lastValue = value;
            if (OptionBehaviour == null)
                return;
            OptionBehaviour.Value = value;
            OptionBehaviour.UpdateValue();
            _lastValue = value;
        }
    }
    internal static StringOption OptionBehaviour { get; set; } = null!;

    internal static int _lastValue;
    internal static readonly StringNames GamemodeName = MiraLocaleManager.GetOrCreateLocaleString("Gamemode");
    internal static readonly StringNames CustomName = MiraLocaleManager.GetOrCreateLocaleString("Custom");
    internal static readonly Dictionary<uint, StringNames> Values = new()
    {
        [0] = MiraLocaleManager.GetOrCreateLocaleString("MiraApi.Gamemode.Classic"),
    };

    internal static void AddOption(AbstractGameMode mode)
    {
        if (!Values.ContainsKey(mode.ID))
            Values.Add(mode.ID, MiraLocaleManager.GetOrCreateLocaleString(mode.Name));
    }
    /*[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPostfix]
    private static void CreateSettingsPatch(GameOptionsMenu __instance)
    {
        if (MenuState.Instance.CurrentModIdx != 0 || GameManager.Instance.IsHideAndSeek() || CustomGameModeManager.ActiveMode == null)
        {
            return;
        }

        var num = 0.713f;

        foreach (var category in __instance.settingsContainer.GetComponentsInChildren<CategoryHeaderMasked>())
        {
            if (category)
                category.gameObject.transform.localPosition -= new Vector3(0, 1.3f, 0);
        }
        CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
        categoryHeaderMasked.SetHeader(CustomName, 20);
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
        OptionBehaviour = Object.Instantiate(
            __instance.stringOptionOrigin,
            Vector3.zero,
            Quaternion.identity,
            __instance.settingsContainer);
        num -= 0.63f;
        OptionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
        OptionBehaviour.SetClickMask(__instance.ButtonClickMask);
        StringGameSetting setting = ScriptableObject.CreateInstance<StringGameSetting>();
        setting.Type = OptionTypes.MultipleChoice;
        setting.Title = GamemodeName;
        setting.Index = _lastValue;
        setting.Values = new Il2CppStructArray<StringNames>([Values[0]]);
        OptionBehaviour.SetUpFromData(setting, 20);
        Set(_lastValue);
        OptionBehaviour.TitleText.fontSize = 3;
        foreach (var optionBehaviour in __instance.Children.ToArray().Skip(1))
        {
            optionBehaviour.gameObject.transform.localPosition -= new Vector3(0, 1.3f, 0);
        }
        __instance.Children.Add(OptionBehaviour);
        for (var i = 1; i < Values.Count; i++)
            OptionBehaviour.Values = (Il2CppStructArray<StringNames>)OptionBehaviour.Values.Add(Values.ElementAt(i).Value);
        __instance.scrollBar.SetYBoundsMax(__instance.scrollBar.GetYBounds().max + 1);
    }*/

    internal static void Set(int val)
    {
        Value = val;
        var previousMode = CustomGameModeManager.ActiveMode;
        CustomGameModeManager.GetAndSetGameMode();
        HudPatches.SetGameModeText(CustomGameModeManager.GetMode(Values.ElementAt(_lastValue).Key).ColoredName);
        var gm = CustomGameModeManager.ActiveMode!;
        if (gm != previousMode)
        {
            ModdedOptionsManager.AddSettingsChangeMessage(
                HudManager.Instance.Notifier,
                GamemodeName,
                gm.ColoredName,
                new Color(0.7333f, 0.7333f, 0.7333f, 1),
                gm.TmpIcon,
                true);
            if (MenuState.Instance)
            {
                // should force reset roles for gamemodes properly
                foreach (var roleMenu in MenuState.Instance.QueuedRoleMenuRefresh)
                {
                    MenuState.Instance.QueuedRoleMenuRefresh[roleMenu.Key] = true;
                }
            }
        }
        // could make Values a dict of AbstractGameMode too
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.ValueChanged))]
    [HarmonyPrefix]
    private static bool ValueChanged(GameOptionsMenu __instance, OptionBehaviour option)
    {
        if (OptionBehaviour.Equals(option))
        {
            Info($"Game mode changed to {option.GetInt()}");
            RpcSyncGamemode(PlayerControl.LocalPlayer, option.GetInt());
            if (GameSettingMenu.Instance && CustomGameModeManager.ActiveMode != null)
            {
                GameOptionsMenuPatch.ToggleGamemodeOptions(CustomGameModeManager.ActiveMode, __instance);
                GameSettingMenu.Instance.RoleSettingsButton.gameObject.SetActive(CustomGameModeManager.ActiveMode.ShowNormalRoleSettings);
            }
            return false;
        }
        return true;
    }

    [MethodRpc((uint) MiraRpc.SyncGamemodeOption)]
    internal static void RpcSyncGamemode(PlayerControl host, int data)
    {
        Set(data);
    }
}
