using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.HnsReimplemented;

/// <summary>
/// A Unity script designed to mimic the base game's player danger meter during Hide and Seek.
/// </summary>
/// <param name="cppPtr">The pointer of this instance's equivalent in the Il2Cpp domain.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
public sealed class HnsDangerMeter(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the instance of the danger meter.
    /// </summary>
    public static HnsDangerMeter Instance { get; private set; }

    private DangerMeter dangerMeter;

    private List<PlayerControl>? impostors = [];

    private float scaryMusicDistance;

    private float veryScaryMusicDistance;

    private float dangerLevel1;

    private float dangerLevel2;

    private bool firstMusicActivation;

    private float firstCrossfadeCountdown;

    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (impostors == null || localPlayer == null)
        {
            return;
        }

        if (impostors.Count <= 0)
        {
            return;
        }

        var num = float.MaxValue;
        foreach (var playerControl in impostors)
        {
            if (playerControl == null)
                continue;
            var sqrMagnitude = (playerControl.transform.position - localPlayer.transform.position).sqrMagnitude;
            if (sqrMagnitude < scaryMusicDistance && num > sqrMagnitude)
            {
                num = sqrMagnitude;
            }
        }

        if (HideAndSeekHudHelper.Instance.HideCountdown > 0f)
        {
            dangerLevel1 = 0f;
            dangerLevel2 = 0f;
        }
        else
        {
            if (firstMusicActivation)
            {
                firstMusicActivation = false;
                firstCrossfadeCountdown = 3f;
                HnsMusicHandler.Instance.SetMusicCrossFadeSpeed(0.6f);
            }

            if (firstCrossfadeCountdown > 0f)
            {
                firstCrossfadeCountdown -= Time.deltaTime;
                if (firstCrossfadeCountdown <= 0f)
                {
                    HnsMusicHandler.Instance.SetMusicCrossFadeSpeed(5f);
                }
            }

            dangerLevel1 = Mathf.Clamp01(
                (scaryMusicDistance - num) / (scaryMusicDistance - veryScaryMusicDistance));
            dangerLevel2 = Mathf.Clamp01((veryScaryMusicDistance - num) / veryScaryMusicDistance);
        }

        UpdateDangerMeter();
        UpdateDangerMusic();
    }

    private void UpdateDangerMusic()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer != null && localPlayer.Data != null && localPlayer.Data.IsDead)
        {
            HnsMusicHandler.Instance.SetTaskState(false);
            HnsMusicHandler.Instance.ResetMusic();
            return;
        }

        HnsMusicHandler.Instance.SetMusicValues(dangerLevel1, dangerLevel2);
    }

    private void UpdateDangerMeter()
    {
        if (dangerMeter == null)
        {
            return;
        }

        dangerMeter.SetDangerValue(dangerLevel1, dangerLevel2);
    }

    /// <summary>
    /// An event that is executed when the game starts.
    /// </summary>
    public void OnGameStart()
    {
        firstMusicActivation = true;
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            dangerMeter = HudManager.Instance.DangerMeter;
            dangerMeter.gameObject.SetActive(true);
        }

        impostors = [.. Helpers.GetAlivePlayers().Where(x => x.Data.Role.IsImpostor)];

        var baseSpeed = OptionGroupSingleton<HnsCrewmateOptions>.Instance.PlayerSpeed.Value;
        scaryMusicDistance = 55f * baseSpeed;
        veryScaryMusicDistance = 15f * baseSpeed;
        if (scaryMusicDistance >= veryScaryMusicDistance) return;
        var num = veryScaryMusicDistance;
        var num2 = scaryMusicDistance;
        scaryMusicDistance = num;
        veryScaryMusicDistance = num2;
    }
}
