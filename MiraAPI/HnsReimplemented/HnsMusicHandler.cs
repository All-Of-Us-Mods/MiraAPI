using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Amongus.GameModes.HideAndSeek;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.HnsReimplemented;

/// <summary>
/// A Unity script designed to handle the music during Hide and Seek.
/// </summary>
/// <param name="cppPtr">The pointer of this instance's equivalent in the Il2Cpp domain.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
public sealed class HnsMusicHandler(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the instance of the music handler.
    /// </summary>
    public static HnsMusicHandler Instance { get; private set; }

    private HideAndSeekMusicCollection musicCollection;

    private float lastMusicSyncTime;

    private bool isDoingTask;

    private float normalVolume;

    private float taskVolume;

    private float dangerLevel1Volume;

    private float dangerLevel2Volume;

    private AudioSource normalSource;

    private AudioSource taskSource;

    private AudioSource dangerLevel1Source;

    private AudioSource dangerLevel2Source;

    private float musicLerpSpeed = 5f;

    private readonly Dictionary<LogicHnSMusic.HideAndSeekMusicTrack, string> musicNames = new()
    {
        {
            LogicHnSMusic.HideAndSeekMusicTrack.Normal,
            "HnS_Music_Normal"
        },
        {
            LogicHnSMusic.HideAndSeekMusicTrack.Task,
            "HnS_Music_Task"
        },
        {
            LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel1,
            "HnS_Music_DangerLevel1"
        },
        {
            LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel2,
            "HnS_Music_DangerLevel2"
        },
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        musicCollection = GameManagerCreator.Instance.HideAndSeekManagerPrefab.MusicCollection;
    }

    /// <summary>
    /// An event that is executed when the round starts.
    /// </summary>
    public void OnGameStart()
    {
        InitMusic();
        ResetMusic();
    }

    private void OnDestroy()
    {
        ResetMusic();
    }

    private void InitMusic()
    {
        if (normalSource == null)
        {
            normalSource =
                SoundManager.Instance.GetNamedSfxSource(musicNames[LogicHnSMusic.HideAndSeekMusicTrack.Normal]);
        }

        normalSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        normalSource.clip = musicCollection.NormalMusic;
        normalSource.loop = true;
        if (taskSource == null)
        {
            taskSource =
                SoundManager.Instance.GetNamedSfxSource(musicNames[LogicHnSMusic.HideAndSeekMusicTrack.Task]);
        }

        taskSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        taskSource.volume = 0f;
        taskSource.clip = musicCollection.TaskMusic;
        taskSource.loop = true;
        if (dangerLevel1Source == null)
        {
            dangerLevel1Source =
                SoundManager.Instance.GetNamedSfxSource(
                    musicNames[LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel1]);
        }

        dangerLevel1Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        dangerLevel1Source.volume = 0f;
        dangerLevel1Source.clip = musicCollection.DangerLevel1Music;
        dangerLevel1Source.loop = true;
        if (dangerLevel2Source == null)
        {
            dangerLevel2Source =
                SoundManager.Instance.GetNamedSfxSource(
                    musicNames[LogicHnSMusic.HideAndSeekMusicTrack.DangerLevel2]);
        }

        dangerLevel2Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
        dangerLevel2Source.volume = 0f;
        dangerLevel2Source.clip = musicCollection.DangerLevel2Music;
        dangerLevel2Source.loop = true;
        normalSource.Play();
        taskSource.Play();
        dangerLevel1Source.Play();
        dangerLevel2Source.Play();
        SyncMusic();
    }

    /// <summary>
    /// Gets the music with the intro.
    /// </summary>
    public void StartMusicWithIntro()
    {
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
        var clip = OptionGroupSingleton<HnsCrewmateOptions>.Instance.HidingTime.Value <= 180f
            ? musicCollection.ImpostorShortMusic
            : musicCollection.ImpostorLongMusic;
        if (AprilFoolsMode.ShouldHorseAround())
        {
            clip = musicCollection.ImpostorRanchMusic;
        }

        SoundManager.Instance.PlaySound(clip, true, 1f, SoundManager.Instance.MusicChannel);
    }

    /// <summary>
    /// Sets the current task state to alter music behaviour.
    /// </summary>
    /// <param name="taskState">The new value of the state.</param>
    public void SetTaskState(bool taskState)
    {
        isDoingTask = taskState;
    }

    private void FixedUpdate()
    {
        if (normalSource == null || taskSource == null || dangerLevel1Source == null ||
            dangerLevel2Source == null)
        {
            return;
        }

        if (Time.unscaledTime > lastMusicSyncTime + 1f)
        {
            SyncMusic();
        }

        normalSource.volume = Mathf.Lerp(
            normalSource.volume,
            normalVolume,
            Time.fixedDeltaTime * musicLerpSpeed);
        taskSource.volume = Mathf.Lerp(
            taskSource.volume,
            taskVolume,
            Time.fixedDeltaTime * musicLerpSpeed);
        dangerLevel1Source.volume = Mathf.Lerp(
            dangerLevel1Source.volume,
            dangerLevel1Volume,
            Time.fixedDeltaTime * musicLerpSpeed);
        dangerLevel2Source.volume = Mathf.Lerp(
            dangerLevel2Source.volume,
            dangerLevel2Volume,
            Time.fixedDeltaTime * musicLerpSpeed);
    }

    private void SyncMusic()
    {
        taskSource.timeSamples = normalSource.timeSamples;
        dangerLevel1Source.timeSamples = normalSource.timeSamples;
        dangerLevel2Source.timeSamples = normalSource.timeSamples;
        lastMusicSyncTime = Time.unscaledTime;
    }

    /// <summary>
    /// Resets the music currently playing.
    /// </summary>
    public void ResetMusic()
    {
        SetMusicValues(0f, 0f);
    }

    /// <summary>
    /// Sets the music crossfade speed.
    /// </summary>
    /// <param name="lerpSpeed">The new speed.</param>
    public void SetMusicCrossFadeSpeed(float lerpSpeed)
    {
        musicLerpSpeed = lerpSpeed;
    }

    /// <summary>
    /// Sets the current music value based on the danger level.
    /// </summary>
    /// <param name="dangerLevel1">The first danger level.</param>
    /// <param name="dangerLevel2">The second danger level.</param>
    public void SetMusicValues(float dangerLevel1, float dangerLevel2)
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            return;
        }

        if (normalSource == null || taskSource == null || dangerLevel1Source == null ||
            dangerLevel2Source == null)
        {
            return;
        }

        normalVolume = isDoingTask ? 0f : 1f;
        taskVolume = isDoingTask ? 1f : 0f;
        dangerLevel1Volume = 0f;
        dangerLevel2Volume = 0f;
        if (dangerLevel1 > 0f)
        {
            dangerLevel1Volume = dangerLevel1;
            if (isDoingTask)
            {
                taskVolume = 1f - dangerLevel1;
            }
            else
            {
                normalVolume = 1f - dangerLevel1;
            }
        }

        if (dangerLevel2 > 0f)
        {
            dangerLevel2Volume = dangerLevel2;
            dangerLevel1Volume = 1f - dangerLevel2;
        }
    }
}
