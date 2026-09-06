using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.HnsReimplemented;

/// <summary>
/// A Unity script designed to help manage the HUD during HnS mode.
/// </summary>
/// <param name="cppPtr">The pointer of this instance's equivalent in the Il2Cpp domain.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
public sealed class HideAndSeekHudHelper(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the instance of the HnS hud helper.
    /// </summary>
    public static HideAndSeekHudHelper Instance { get; private set; }

    private AudioClip finalHideAlertSfx;
    private AudioClip finalHideCountdownSfx;
    [SuppressMessage("Style", "IDE0052:Remove unread private members", Justification = "Ignore for now; tentative code.")]
    private AudioClip taskFinishedSound;

    // private const int SECONDS_TO_BEEP = 10;
    // private const float SECONDS_TO_SET_DIRTY = 1f;

    private float totalHideTime = float.MaxValue;
    private float currentHideTime = float.MaxValue;
    private float totalFinalHideTime = float.MaxValue;
    private float currentFinalHideTime = float.MaxValue;

    private float secondsSinceLastSetDirty;
    private Coroutine beepCoroutine;
    private HideAndSeekTimerBar timerBar;
    private float taskDirtyTimer;

    /// <summary>
    /// Gets or sets the hide countdown timer.
    /// </summary>
    public float HideCountdown { get; set; }

    /// <summary>
    /// Gets a value indicating whether the game has reached the final phase.
    /// </summary>
    public bool IsFinalCountdown => currentHideTime <= 0f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Checks to see if the player can use the Seeker admin.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>true if the player can use the admin ability.</returns>
    public bool SeekerAdminMapEnabled(PlayerControl player)
    {
        int item = Helpers.GetAlivePlayers().Count(x => !x.Data.Role.IsImpostor);
        return !player.inVent && player.Data != null && player.Data.Role != null &&
               ((!player.inVent && player.Data.Role.IsImpostor && IsFinalCountdown &&
                 OptionGroupSingleton<HnsFinalHideOptions>.Instance.FinalHideSeekMap.Value) ||
                (player.Data.Role.IsImpostor &&
                 item <=
                 (GameData.Instance.PlayerCount - 1) / 3));
    }

    /// <summary>
    /// A event that is called when a task is completed.
    /// </summary>
    /// <param name="timeDeduction">The amount of time to deduct from the timer.</param>
    public void OnTaskComplete(float timeDeduction)
    {
        if (timerBar != null)
        {
            timerBar.TaskComplete();
        }

        AdjustEscapeTimer(timeDeduction, true);
    }

    /// <summary>
    /// Gets the match duration.
    /// </summary>
    /// <returns>The match duration in seconds.</returns>
    public static float GetTotalRoundTime()
    {
        float escapeTime = OptionGroupSingleton<HnsCrewmateOptions>.Instance.HidingTime.Value;
        float finalCountdownTime = OptionGroupSingleton<HnsFinalHideOptions>.Instance.FinalHideTime.Value;
        return escapeTime + finalCountdownTime;
    }

    /// <summary>
    /// Gets the amount of total time remaining in the current round.
    /// </summary>
    /// <returns>The remaining time in seconds.</returns>
    public float GetTotalTimeRemaining()
    {
        return currentHideTime + currentFinalHideTime;
    }

    /// <summary>
    /// Gets how much time has passed since the start of the round.
    /// </summary>
    /// <returns>The elapsed time in seconds.</returns>
    public float GetRoundTimeElapsed()
    {
        return GetTotalRoundTime() - GetTotalTimeRemaining();
    }

    private void Start()
    {
        totalHideTime = OptionGroupSingleton<HnsCrewmateOptions>.Instance.HidingTime.Value;
        currentHideTime = totalHideTime;
        totalFinalHideTime = OptionGroupSingleton<HnsFinalHideOptions>.Instance.FinalHideTime.Value;
        currentFinalHideTime = totalFinalHideTime;
        if (timerBar != null)
        {
            Object.Destroy(timerBar);
        }

        finalHideAlertSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX;
        finalHideCountdownSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX;
        taskFinishedSound = GameManagerCreator.Instance.HideAndSeekManagerPrefab.TaskFinishedSound;
        timerBar = Instantiate(
            GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab,
            HudManager.Instance.transform.parent);
    }

    private void OnGameEnd()
    {
        if (timerBar != null)
        {
            Destroy(timerBar.gameObject);
        }

        if (beepCoroutine != null)
        {
            StopCoroutine(beepCoroutine);
        }

        beepCoroutine = null!;
    }

    private void LateUpdate()
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }
        if (HideCountdown > 0f && taskDirtyTimer > 0.25f)
        {
            float num = taskDirtyTimer;
            taskDirtyTimer = 0f;
            if (!PlayerControl.LocalPlayer)
            {
                HudManager.Instance.TaskPanel.SetTaskText(string.Empty);
                return;
            }
            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            if (data == null)
            {
                return;
            }
            bool flag = data.Role != null && data.Role.IsImpostor;
            HudManager.Instance.tasksString.Clear();
            if (PlayerControl.LocalPlayer.myTasks == null || PlayerControl.LocalPlayer.myTasks.Count == 0)
            {
                HudManager.Instance.tasksString.Append("None");
            }
            else
            {
                for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
                {
                    PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
                    if (playerTask)
                    {
                        if (playerTask.TaskType == TaskTypes.FixComms && !flag)
                        {
                            HudManager.Instance.tasksString.Clear();
                            playerTask.AppendTaskText(HudManager.Instance.tasksString);
                            break;
                        }
                        playerTask.AppendTaskText(HudManager.Instance.tasksString);
                    }
                }
                if (data.Role != null)
                {
                    data.Role.AppendTaskHint(HudManager.Instance.tasksString);
                }
                if (HideCountdown > 0f)
                {
                    HideCountdown -= num;
                    HudManager.Instance.tasksString.Append("\n\n" + ((int)HideCountdown));
                }
                HudManager.Instance.tasksString.TrimEnd();
            }
            HudManager.Instance.TaskPanel.SetTaskText(HudManager.Instance.tasksString.ToString());
        }
    }

    private void FixedUpdate()
    {
        secondsSinceLastSetDirty += Time.fixedDeltaTime;

        if (IsFinalCountdown)
        {
            AdjustFinalEscapeTimer(Time.fixedDeltaTime);
            return;
        }

        AdjustEscapeTimer(Time.fixedDeltaTime, false);
    }

    private void OnDestroy()
    {
        if (timerBar != null)
        {
            Destroy(timerBar);
        }
    }

    private void OnFinalCountdownTriggered()
    {
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (!playerControl.Data.Role.IsImpostor && !playerControl.Data.IsDead)
            {
                playerControl.ClearTasks();
                PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl).Text =
                    DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HideActionButton);
            }
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead && Minigame.Instance != null)
        {
            Minigame instance = Minigame.Instance;
            if (instance != null)
            {
                instance.ForceClose();
            }
        }

        timerBar.StartFinalHide();
        SoundManager.Instance.PlaySound(finalHideAlertSfx, false);
        DestroyableSingleton<HudManager>.Instance.SetAlertOverlay(true);
    }

    private void AdjustEscapeTimer(float timeDeduction, bool forceDirty)
    {
        float num = currentHideTime;
        currentHideTime -= timeDeduction;
        currentHideTime = Mathf.Max(currentHideTime, 0f);
        if (currentHideTime <= 10f && beepCoroutine == null)
        {
            beepCoroutine = StartCoroutine(BeepAlmostEverySecond().WrapToIl2Cpp());
        }

        if (num > 0f && currentHideTime <= 0f)
        {
            OnFinalCountdownTriggered();
        }

        timerBar.UpdateTimer(currentHideTime, totalHideTime);
        if (forceDirty || secondsSinceLastSetDirty > 1f)
        {
            secondsSinceLastSetDirty = 0f;
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator BeepAlmostEverySecond()
    {
        while (!IsFinalCountdown)
        {
            float num = currentHideTime / 10f;
            float pitch = 1.5f - num / 2f;
            SoundManager.Instance.PlaySoundImmediate(finalHideCountdownSfx, false, 1f, pitch);
            yield return new WaitForSeconds(1f);
        }

        yield return Effects.Wait(currentFinalHideTime - 10f);
        while (currentFinalHideTime > 0f)
        {
            float num2 = currentFinalHideTime / 10f;
            float pitch2 = 1.5f - num2 / 2f;
            SoundManager.Instance.PlaySoundImmediate(finalHideCountdownSfx, false, 1f, pitch2);
            yield return new WaitForSeconds(1f);
        }
    }

    private void AdjustFinalEscapeTimer(float timeDeduction)
    {
        currentFinalHideTime -= timeDeduction;
        currentFinalHideTime = Mathf.Max(currentFinalHideTime, 0f);
        timerBar.UpdateTimer(currentFinalHideTime, totalFinalHideTime);
        if (secondsSinceLastSetDirty > 1f)
        {
            secondsSinceLastSetDirty = 0f;
        }
    }

    /// <summary>
    /// Checks to see if all timers have expired.
    /// </summary>
    /// <returns>true if all times have expired.</returns>
    public bool AllTimersExpired()
    {
        return currentHideTime <= 0f && currentFinalHideTime <= 0f;
    }
}
