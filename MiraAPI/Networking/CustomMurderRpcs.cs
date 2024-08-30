using AmongUs.GameOptions;
using Assets.CoreScripts;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Networking;
public static class CustomMurderRpcs
{
    [MethodRpc((uint)MiraRpc.CustomMurder)]
    public static void RpcCustomMurder(this PlayerControl source, PlayerControl target, bool didSucceed = true,
        bool resetKillTimer = true, bool createDeadBody = true, bool teleportMurderer = true, bool showKillAnim = true, bool playKillSound = true)
    {
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;
        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(target, murderResultFlags2, resetKillTimer, createDeadBody, teleportMurderer, showKillAnim, playKillSound);
    }

    public static void CustomMurder(this PlayerControl source, PlayerControl target, MurderResultFlags resultFlags,
        bool resetKillTimer = true, bool createDeadBody = true, bool teleportMurderer = true, bool showKillAnim = true, bool playKillSound = true)
    {
        source.isKilling = false;
        source.logger.Debug(string.Format("{0} trying to murder {1}", source.PlayerId, target.PlayerId), null);
        var data = target.Data;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError))
        {
            return;
        }
        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) || (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) && target.protectedByGuardianId > -1))
        {
            target.protectedByGuardianThisRound = true;
            var flag = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (flag && PlayerControl.LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsGuardianAngelCrewmatesProtected);
                DestroyableSingleton<AchievementManager>.Instance.OnProtectACrewmate();
            }
            if (source.AmOwner || flag)
            {
                target.ShowFailedMurder();

                if (resetKillTimer)
                {
                    source.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                }
            }
            else
            {
                target.RemoveProtection();
            }
            source.logger.Debug(string.Format("{0} failed to murder {1} due to guardian angel protection", source.PlayerId, target.PlayerId), null);
            return;
        }
        if (resultFlags.HasFlag(MurderResultFlags.Succeeded) || resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DestroyableSingleton<DebugAnalytics>.Instance.Analytics.Kill(target.Data, source.Data);
            if (source.AmOwner)
            {
                if (GameManager.Instance.IsHideAndSeek())
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills_HideAndSeek);
                }
                else
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
                }
                if (source.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
                }
                if (Constants.ShouldPlaySfx() && playKillSound)
                {
                    SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f, null);
                }

                if (resetKillTimer)
                {
                    source.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
                }
            }
            DestroyableSingleton<UnityTelemetry>.Instance.WriteMurder();
            target.gameObject.layer = LayerMask.NameToLayer("Ghost");
            if (target.AmOwner)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
                if (Minigame.Instance)
                {
                    try
                    {
                        Minigame.Instance.Close();
                        Minigame.Instance.Close();
                    }
                    catch
                    {
                    }
                }

                if (showKillAnim)
                {
                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(source.Data, data);
                }

                target.cosmetics.SetNameMask(false);
                target.RpcSetScanner(false);
            }
            DestroyableSingleton<AchievementManager>.Instance.OnMurder(source.AmOwner, target.AmOwner, source.CurrentOutfitType == PlayerOutfitType.Shapeshifted, source.shapeshiftTargetPlayerId, (int)target.PlayerId);
            Coroutines.Start(source.KillAnimations.Random().CoPerformCustomKill(source, target, createDeadBody, teleportMurderer));
            source.logger.Debug(string.Format("{0} succeeded in murdering {1}", source.PlayerId, target.PlayerId), null);
        }
    }

    public static IEnumerator CoPerformCustomKill(this KillAnimation anim, PlayerControl source, PlayerControl target,
        bool createDeadBody = true, bool teleportMurderer = true)
    {
        var cam = Camera.main.GetComponent<FollowerCamera>();
        var isParticipant = PlayerControl.LocalPlayer == source || PlayerControl.LocalPlayer == target;
        var sourcePhys = source.MyPhysics;

        if (teleportMurderer)
        {
            KillAnimation.SetMovement(source, false);
        }

        KillAnimation.SetMovement(target, false);

        if (isParticipant)
        {
            PlayerControl.LocalPlayer.isKilling = true;
            source.isKilling = true;
        }

        DeadBody deadBody = null;

        if (createDeadBody)
        {
            deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.enabled = false;
            deadBody.ParentId = target.PlayerId;
            deadBody.bodyRenderers.ToArray().ToList().ForEach(target.SetPlayerMaterialColors);

            target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            var vector = target.transform.position + anim.BodyOffset;
            vector.z = vector.y / 1000f;
            deadBody.transform.position = vector;
        }

        if (isParticipant)
        {
            cam.Locked = true;
            ConsoleJoystick.SetMode_Task();
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            }
        }

        target.Die(DeathReason.Kill, true);
        yield return source.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
        sourcePhys.Animations.PlayIdleAnimation();

        if (teleportMurderer)
        {
            source.NetTransform.SnapTo(target.transform.position);
            KillAnimation.SetMovement(source, true);
        }
        KillAnimation.SetMovement(target, true);

        if (deadBody)
        {
            deadBody.enabled = true;
        }

        if (isParticipant)
        {
            cam.Locked = false;
            PlayerControl.LocalPlayer.isKilling = false;
            source.isKilling = false;
        }

        yield break;
    }
}