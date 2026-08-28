using System;
using System.Collections;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Networking;

/// <summary>
/// Custom murder RPCs to fix issues with default ones.
/// </summary>
public static class CustomMurderRpc
{
    /// <summary>
    /// Networked Custom Murder method, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void RpcCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.RpcCustomMurder(
            target,
            MeetingCheck.Ignore,
            didSucceed,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="inMeeting">Whether the murder is intended to be triggered in a meeting.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.CustomMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var defenseFlag = beforeMurderEvent.IgnoreDefense;
        var indirectFlag = beforeMurderEvent.IsIndirectAttack;
        var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (!defenseFlag && target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        RpcConfirmCustomMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            indirectFlag,
            defenseFlag,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="host">The game host.</param>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="murderResultFlags">End result for the murder.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.ConfirmCustomMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmCustomMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        bool isIndirect,
        bool ignoreDefense,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance || !host.IsHost() || target.Data.IsDead || target.Data.Disconnected)
        {
            source.isKilling = false;
            return;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (isIndirect || ignoreDefense)
        {
            source.CustomMurder(
                target,
                null,
                isIndirect,
                ignoreDefense,
                murderResultFlags2,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }
        else
        {
            source.CustomMurder(
                target,
                murderResultFlags2,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="inMeeting">Whether the murder is intended to be triggered in a meeting.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.AdvancedCustomMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAdvancedCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool isIndirect,
        bool ignoreDefense = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, isIndirect, ignoreDefense, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var defenseFlag = beforeMurderEvent.IgnoreDefense;
        var indirectFlag = beforeMurderEvent.IsIndirectAttack;
        var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (!defenseFlag && target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        RpcConfirmAdvancedCustomMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            indirectFlag,
            defenseFlag,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="host">The game host.</param>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="murderResultFlags">End result for the murder.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.ConfirmAdvancedCustomMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmAdvancedCustomMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        bool isIndirect,
        bool ignoreDefense,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance || !host.IsHost() || target.Data.IsDead || target.Data.Disconnected)
        {
            source.isKilling = false;
            return;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(
            target,
            null,
            isIndirect,
            ignoreDefense,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="framed">The player to frame.</param>
    /// <param name="inMeeting">Whether the murder is intended to be triggered in a meeting.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the framed player be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.FramedCustomMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFramedCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        MeetingCheck inMeeting,
        bool isIndirect = true,
        bool ignoreDefense = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, framed, isIndirect, ignoreDefense, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var defenseFlag = beforeMurderEvent.IgnoreDefense;
        var indirectFlag = beforeMurderEvent.IsIndirectAttack;
        var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (!defenseFlag && target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        RpcConfirmFramedCustomMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            framed,
            indirectFlag,
            defenseFlag,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well, ran by clients for the host to confirm kills.
    /// </summary>
    /// <param name="host">The game host.</param>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="framed">The player to frame.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="murderResultFlags">End result for the murder.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the framed player be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.ConfirmFramedCustomMurder, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmFramedCustomMurder(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool isIndirect,
        bool ignoreDefense,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance || !host.IsHost() || target.Data.IsDead || target.Data.Disconnected)
        {
            source.isKilling = false;
            return;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(
            target,
            framed,
            isIndirect,
            ignoreDefense,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Custom Murder method without networking. If you need a networked version, use <see cref="RpcFramedCustomMurder"/> or <see cref="RpcAdvancedCustomMurder"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="framed">The player to frame, if any.</param>
    /// <param name="isIndirect">Should the kill be indirect.</param>
    /// <param name="ignoreDefense">Should the kill ignore defense.</param>
    /// <param name="resultFlags">Murder result flags.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void CustomMurder(
        this PlayerControl source,
        PlayerControl target,
        PlayerControl? framed,
        bool isIndirect,
        bool ignoreDefense,
        MurderResultFlags resultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.isKilling = false;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError) || LobbyBehaviour.Instance)
        {
            return;
        }

        if (!ignoreDefense && (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
                               (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) &&
                                target.protectedByGuardianId > -1)))
        {
            target.protectedByGuardianThisRound = true;
            var flag = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (flag && PlayerControl.LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_GuardianAngel_CrewmatesProtected);
                AchievementManager.Instance.OnProtectACrewmate();
            }

            if (source.AmOwner || flag)
            {
                target.ShowFailedMurder();

                if (resetKillTimer)
                {
                    source.SetKillTimer(
                        GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                }
            }
            else
            {
                target.RemoveProtection();
            }

            return;
        }

        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) &&
            !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            return;
        }

        DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
        if (source.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(
                GameManager.Instance.IsHideAndSeek()
                    ? StatID.HideAndSeek_ImpostorKills
                    : StatID.ImpostorKills);

            if (source.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_Shapeshifter_ShiftedKills);
            }

            if (Constants.ShouldPlaySfx() && playKillSound)
            {
                SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f);
            }

            if (resetKillTimer)
            {
                source.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            }
        }

        UnityTelemetry.Instance.WriteMurder();
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
            }

            if (showKillAnim)
            {
                try
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(source.Data, target.Data);
                }
                catch (Exception e)
                {
                    Error($"Error with kill animation: {e.ToString()}");
                }
            }

            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }

        AchievementManager.Instance.OnMurder(
            source.AmOwner,
            target.AmOwner,
            source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            source.shapeshiftTargetPlayerId,
            target.PlayerId);
        source.MyPhysics.StartCoroutine(source.KillAnimations.RandomSnapshot().CoPerformCustomKill(source, target, framed, isIndirect, ignoreDefense, createDeadBody, teleportMurderer));
    }

    /// <summary>
    /// Custom Murder method without networking. If you need a networked version, use <see cref="RpcCustomMurder(PlayerControl, PlayerControl, bool, bool, bool, bool, bool, bool)"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="resultFlags">Murder result flags.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void CustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MurderResultFlags resultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.isKilling = false;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError) || LobbyBehaviour.Instance)
        {
            return;
        }

        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) && target.protectedByGuardianId > -1))
        {
            target.protectedByGuardianThisRound = true;
            var flag = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (flag && PlayerControl.LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_GuardianAngel_CrewmatesProtected);
                AchievementManager.Instance.OnProtectACrewmate();
            }

            if (source.AmOwner || flag)
            {
                target.ShowFailedMurder();

                if (resetKillTimer)
                {
                    source.SetKillTimer(
                        CustomGameModeManager.ActiveMode != null
                            ? CustomGameModeManager.ActiveMode.DefaultImpostorKillCooldown / 2f
                            : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) /
                              2f);
                }
            }
            else
            {
                target.RemoveProtection();
            }

            return;
        }

        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) &&
            !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            return;
        }

        DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
        if (source.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(
                GameManager.Instance.IsHideAndSeek()
                    ? StatID.HideAndSeek_ImpostorKills
                    : StatID.ImpostorKills);

            if (source.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_Shapeshifter_ShiftedKills);
            }

            if (Constants.ShouldPlaySfx() && playKillSound)
            {
                SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f);
            }

            if (resetKillTimer)
            {
                source.SetKillTimer(CustomGameModeManager.ActiveMode != null
                    ? CustomGameModeManager.ActiveMode.DefaultImpostorKillCooldown
                    : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            }
        }

        UnityTelemetry.Instance.WriteMurder();
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
            }

            if (showKillAnim)
            {
                try
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(source.Data, target.Data);
                }
                catch (Exception e)
                {
                    Error($"Error with kill animation: {e.ToString()}");
                }
            }

            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }

        AchievementManager.Instance.OnMurder(
            source.AmOwner,
            target.AmOwner,
            source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            source.shapeshiftTargetPlayerId,
            target.PlayerId);
        source.MyPhysics.StartCoroutine(source.KillAnimations.RandomSnapshot().CoPerformCustomKill(source, target, null, false, false, createDeadBody, teleportMurderer));
    }

    /// <summary>
    /// Perform a custom kill animation.
    /// </summary>
    /// <param name="anim">The kill animation.</param>
    /// <param name="source">The murderer.</param>
    /// <param name="target">The murdered player.</param>
    /// <param name="framed">The framed player, if any.</param>
    /// <param name="isIndirect">Was the kill indirect.</param>
    /// <param name="ignoreDefense">Would the kill ignore defense.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the murder be teleported.</param>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoPerformCustomKill(
        this KillAnimation anim,
        PlayerControl source,
        PlayerControl target,
        PlayerControl? framed,
        bool isIndirect,
        bool ignoreDefense,
        bool createDeadBody = true,
        bool teleportMurderer = true)
    {
        if (LobbyBehaviour.Instance)
        {
            yield break;
        }
        var cam = Camera.main?.GetComponent<FollowerCamera>();
        var playerToLunge = framed ?? source;
        var isParticipant = playerToLunge.AmOwner || target.AmOwner;
        var sourcePhys = playerToLunge.MyPhysics;

        if (teleportMurderer)
        {
            KillAnimation.SetMovement(playerToLunge, false);
        }

        KillAnimation.SetMovement(target, false);

        if (isParticipant)
        {
            PlayerControl.LocalPlayer.isKilling = true;
            source.isKilling = true;
        }

        DeadBody? deadBody = null;

        if (createDeadBody)
        {
            deadBody = Object.Instantiate(GameManager.Instance.GetDeadBody(source.Data.Role));
            deadBody.enabled = false;
            deadBody.ParentId = target.PlayerId;
            deadBody.bodyRenderers.ToList().ForEach(target.SetPlayerMaterialColors);

            target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            var vector = target.transform.position + anim.BodyOffset;
            vector.z = vector.y / 1000f;
            deadBody.transform.position = vector;
        }

        source.Data.Role.KillAnimSpecialSetup(deadBody, source, target);
        target.Data.Role.KillAnimSpecialSetup(deadBody, source, target);

        // no idea if this causes bugs, but innersloth is brain-dead
        // I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF
        PlayerControl.LocalPlayer.Data.Role.KillAnimSpecialSetup(deadBody, source, target);

        if (isParticipant)
        {
            if (cam != null)
            {
                cam.Locked = true;
            }

            ConsoleJoystick.SetMode_Task();
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            }
        }

        target.Die(DeathReason.Kill, true);

        if (teleportMurderer)
        {
            yield return source.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
            sourcePhys.Animations.PlayIdleAnimation();
            source.NetTransform.SnapTo(target.transform.position);
            KillAnimation.SetMovement(playerToLunge, true);
        }

        KillAnimation.SetMovement(target, true);

        if (deadBody != null)
        {
            deadBody.enabled = true;
        }

        var afterMurderEvent = framed != null ? new AfterMurderEvent(source, target, framed, deadBody, isIndirect, ignoreDefense) : new AfterMurderEvent(source, target, deadBody, isIndirect, ignoreDefense);
        MiraEventManager.InvokeEvent(afterMurderEvent);

        if (!isParticipant)
        {
            yield break;
        }

        if (cam != null)
        {
            cam.Locked = false;
        }

        PlayerControl.LocalPlayer.isKilling = false;
        source.isKilling = false;
    }
}

/// <summary>
/// Checks for custom murders.
/// </summary>
public enum MeetingCheck
{
    /// <summary>
    /// Checks for meetings.
    /// </summary>
    ForMeeting,

    /// <summary>
    /// Checks for a meeting to not be active.
    /// </summary>
    OutsideMeeting,

    /// <summary>
    /// Doesn't check for a meeting at all.
    /// </summary>
    Ignore,
}
