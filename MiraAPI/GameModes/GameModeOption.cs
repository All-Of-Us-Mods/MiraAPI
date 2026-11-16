using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Patches;
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
[SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
public static class GameModeOption
{
    /// <summary>
    /// Gets the current index of the Game Mode Option.<br/>
    /// For the value as an <see cref="AbstractGameMode"/>, see <see cref="CustomGameModeManager.ActiveMode"/>.
    /// </summary>
    public static int Value
    {
        get => OptionBehaviour != null
            ? OptionBehaviour.GetInt()
            : LastValue;
        private set
        {
            LastValue = value;

            if (OptionBehaviour == null)
                return;

            OptionBehaviour.Value = value;
            OptionBehaviour.UpdateValue();
        }
    }

    internal static readonly StringNames GamemodeName = MiraLocaleManager.GetOrCreateLocaleString("MiraApi.Gamemode");
    internal static readonly StringNames CustomName = MiraLocaleManager.GetOrCreateLocaleString("Custom");
    internal static readonly List<AbstractGameMode> Values = [];

    internal static StringOption OptionBehaviour = null!;
    internal static int LastValue;

    internal static void AddOption(AbstractGameMode mode)
    {
        mode.StringName = MiraLocaleManager.GetOrCreateLocaleString(mode.Name);
        Values.Add(mode);
    }

    internal static void Set(int val)
    {
        Value = val;
        var previousMode = CustomGameModeManager.ActiveMode;
        CustomGameModeManager.GetAndSetGameMode();
        HudPatches.SetGameModeText(Values[LastValue].ColoredName);
        var gm = CustomGameModeManager.ActiveMode!;

        if (gm == previousMode)
            return;

        ModdedOptionsManager.AddSettingsChangeMessage(
            HudManager.Instance.Notifier,
            GamemodeName,
            gm.ColoredName,
            new Color(0.7333f, 0.7333f, 0.7333f, 1),
            gm.TmpIcon);

        if (MenuState.Instance)
        {
            // should force reset roles for gamemodes properly
            foreach (var roleMenu in MenuState.Instance.QueuedRoleMenuRefresh)
            {
                MenuState.Instance.QueuedRoleMenuRefresh[roleMenu.Key] = true;
            }
        }

        GameOptionsMenuPatch.RefreshForGameMode();
        RoleGuidePatches.RefreshSettingsForGameMode();
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.ValueChanged))]
    [HarmonyPrefix]
    private static bool ValueChanged(OptionBehaviour option)
    {
        if (!OptionBehaviour.Equals(option))
            return true;

        Info($"Game mode changed to {option.GetInt()}");
        RpcSyncGamemode(PlayerControl.LocalPlayer, option.GetInt());

        return false;
    }

    [MethodRpc((uint)MiraRpc.SyncGamemodeOption)]
    internal static void RpcSyncGamemode(PlayerControl _, int data)
    {
        Set(data);
    }
}
