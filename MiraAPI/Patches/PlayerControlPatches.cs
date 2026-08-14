using System.Globalization;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities.Extensions;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the <see cref="PlayerControl"/> class.
/// </summary>
[HarmonyPatch(typeof(PlayerControl))]
internal static class PlayerControlPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.Start))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlStartPostfix(PlayerControl __instance)
    {
        if (__instance.gameObject.TryGetComponent<ModifierComponent>(out var modifierComp))
        {
            modifierComp.DestroyImmediate();
        }

        if (__instance.gameObject.TryGetComponent<PlayerVoteData>(out var voteComp))
        {
            voteComp.DestroyImmediate();
        }

        __instance.gameObject.AddComponent<ModifierComponent>();
        __instance.gameObject.AddComponent<PlayerVoteData>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.Die))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlDiePostfix(PlayerControl __instance, DeathReason reason)
    {
        var deathEvent = new PlayerDeathEvent(__instance, reason, Helpers.GetBodyById(__instance.PlayerId));
        MiraEventManager.InvokeEvent(deathEvent);

        var modifiersComponent = __instance.GetComponent<ModifierComponent>();

        if (modifiersComponent)
        {
            modifiersComponent.ActiveModifiers.ForEach(x => x.OnDeath(reason));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.CompleteTask))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerCompleteTaskPostfix(PlayerControl __instance, uint idx)
    {
        var playerTask = __instance.myTasks.ToArray().First(playerTask => playerTask.Id == idx);
        if (playerTask != null)
        {
            var completeTaskEvent = new CompleteTaskEvent(__instance, playerTask);
            MiraEventManager.InvokeEvent(completeTaskEvent);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.CheckMurder))]
    public static void PlayerControlCheckMurderPrefix(PlayerControl __instance, PlayerControl target, ref bool __runOriginal)
    {
        __runOriginal = false;

        __instance.logger.Debug($"Checking if {__instance.PlayerId} murdered {(target == null ? "null player" : target.PlayerId.ToString(NumberFormatInfo.InvariantInfo))}");

        __instance.isKilling = false;
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!target || __instance.Data.IsDead || !__instance.Data.Role.IsImpostor || __instance.Data.Disconnected)
        {
            int num = target ? target!.PlayerId : -1;
            __instance.logger.Warning($"Bad kill from {__instance.PlayerId} to {num}");
            __instance.RpcMurderPlayer(target, false);
            return;
        }

        NetworkedPlayerInfo data = target!.Data;
        if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() ||
            target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
        {
            __instance.logger.Warning("Invalid target data for kill");
            __instance.RpcMurderPlayer(target, false);
            return;
        }

        if (MeetingHud.Instance)
        {
            __instance.logger.Warning("Tried to kill while a meeting was starting");
            __instance.RpcMurderPlayer(target, false);
            return;
        }

        var beforeMurderEvent = new BeforeMurderEvent(__instance, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            return;
        }

        __instance.isKilling = true;
        __instance.RpcMurderPlayer(target, true);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.RpcMurderPlayer))]
    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    public static bool MurderPlayerPrefix()
    {
        return !LobbyBehaviour.Instance;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    // ReSharper disable once InconsistentNaming
    public static void PlayerControlFixedUpdatePostfix(PlayerControl __instance)
    {
        if (!__instance.AmOwner)
        {
            return;
        }

        var role = __instance.Data?.Role;
        foreach (var button in CustomButtonManager.CustomButtons)
        {
            if (role == null)
            {
                continue;
            }

            try
            {
                if (!button.Enabled(role))
                {
                    button.SetActive(false, role);
                    continue;
                }

                button.FixedUpdateHandler(__instance);
            }
            catch (System.Exception e)
            {
                Error($"Failed to update custom button {button.GetType().Name}: {e}");
            }
        }
    }
}
