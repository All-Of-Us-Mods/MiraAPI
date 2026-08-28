using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.HnsReimplemented;

[RegisterInIl2Cpp]
public sealed class HnsDangerMeter(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static HnsDangerMeter Instance;

    private void Awake()
    {
        Instance = this;
    }

    private DangerMeter dangerMeter;

    private List<PlayerControl> impostors = [];

    private float scaryMusicDistance;

    private float veryScaryMusicDistance;

    private float dangerLevel1;

    private float dangerLevel2;

    private bool firstMusicActivation;

    private float firstCrossfadeCountdown;

    public void FixedUpdate()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (impostors == null || localPlayer == null)
        {
            return;
        }

        if (impostors.Count <= 0)
        {
            return;
        }

        float num = float.MaxValue;
        foreach (PlayerControl playerControl in impostors)
        {
            if (!(playerControl == null))
            {
                float sqrMagnitude = (playerControl.transform.position - localPlayer.transform.position).sqrMagnitude;
                if (sqrMagnitude < scaryMusicDistance && num > sqrMagnitude)
                {
                    num = sqrMagnitude;
                }
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
                HnsMusicHandler.Instance.SetMusicCrossfadeSpeed(0.6f);
            }

            if (firstCrossfadeCountdown > 0f)
            {
                firstCrossfadeCountdown -= Time.deltaTime;
                if (firstCrossfadeCountdown <= 0f)
                {
                    HnsMusicHandler.Instance.SetMusicCrossfadeSpeed(5f);
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
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
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

    public void OnGameStart()
    {
        firstMusicActivation = true;
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            dangerMeter = HudManager.Instance.DangerMeter;
            dangerMeter.gameObject.SetActive(true);
        }

        impostors = Helpers.GetAlivePlayers().Where(x => x.Data.Role.IsImpostor).ToList();

        var baseSpeed = OptionGroupSingleton<HnsCrewmateOptions>.Instance.PlayerSpeed.Value;
        scaryMusicDistance = 55f *
                                  baseSpeed;
        veryScaryMusicDistance = 15f *
                                      baseSpeed;
        if (scaryMusicDistance < veryScaryMusicDistance)
        {
            float num = veryScaryMusicDistance;
            float num2 = scaryMusicDistance;
            scaryMusicDistance = num;
            veryScaryMusicDistance = num2;
        }
    }
}
