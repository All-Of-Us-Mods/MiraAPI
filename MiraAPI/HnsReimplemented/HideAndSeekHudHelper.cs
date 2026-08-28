using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.HnsReimplemented;

[RegisterInIl2Cpp]
public sealed class HideAndSeekHudHelper(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static HideAndSeekHudHelper Instance { get; private set; }

    public AudioClip FinalHideAlertSfx;

    public AudioClip FinalHideCountdownSfx;

    private AudioClip TaskFinishedSound;

    private const int SECONDS_TO_BEEP = 10;

    private const float SECONDS_TO_SET_DIRTY = 1f;

    private HideAndSeekTimerBar timerBar;

    private float totalHideTime = float.MaxValue;

    private float currentHideTime = float.MaxValue;

    private float totalFinalHideTime = float.MaxValue;

    private float currentFinalHideTime = float.MaxValue;

    private float secondsSinceLastSetDirty;
    private Coroutine beepCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsFinalCountdown
    {
        get { return currentHideTime <= 0f; }
    }

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

    public void OnTaskComplete(float timeDeduction)
    {
        if (timerBar != null)
        {
            timerBar.TaskComplete();
        }

        AdjustEscapeTimer(timeDeduction, true);
    }

    public float GetTotalRoundTime()
    {
        float escapeTime = OptionGroupSingleton<HnsCrewmateOptions>.Instance.HidingTime.Value;
        float finalCountdownTime = OptionGroupSingleton<HnsFinalHideOptions>.Instance.FinalHideTime.Value;
        return escapeTime + finalCountdownTime;
    }

    public float GetTotalTimeRemaining()
    {
        return currentHideTime + currentFinalHideTime;
    }

    public float GetRoundTimeElapsed()
    {
        return GetTotalRoundTime() - GetTotalTimeRemaining();
    }

    public void Start()
    {
        totalHideTime = OptionGroupSingleton<HnsCrewmateOptions>.Instance.HidingTime.Value;
        currentHideTime = totalHideTime;
        totalFinalHideTime = OptionGroupSingleton<HnsFinalHideOptions>.Instance.FinalHideTime.Value;
        currentFinalHideTime = totalFinalHideTime;
        if (timerBar != null)
        {
            Object.Destroy(timerBar);
        }

        FinalHideAlertSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX;
        FinalHideCountdownSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX;
        TaskFinishedSound = GameManagerCreator.Instance.HideAndSeekManagerPrefab.TaskFinishedSound;
        timerBar = Object.Instantiate<HideAndSeekTimerBar>(
            GameManagerCreator.Instance.HideAndSeekManagerPrefab.TimerBarPrefab,
            HudManager.Instance.transform.parent);
    }

    public void OnGameEnd()
    {
        if (timerBar != null)
        {
            Object.Destroy(timerBar.gameObject);
        }

        if (beepCoroutine != null)
        {
            StopCoroutine(beepCoroutine);
        }

        beepCoroutine = null!;
    }
    private float taskDirtyTimer;
    public float HideCountdown { get; set; }

    public void LateUpdate()
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
    public void FixedUpdate()
    {
        secondsSinceLastSetDirty += Time.fixedDeltaTime;
        if (IsFinalCountdown)
        {
            AdjustFinalEscapeTimer(Time.fixedDeltaTime);
            return;
        }

        AdjustEscapeTimer(Time.fixedDeltaTime, false);
    }

    public void OnDestroy()
    {
        if (timerBar != null)
        {
            Object.Destroy(timerBar);
        }
    }

    private void OnFinalCountdownTriggered()
    {
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (!playerControl.Data.Role.IsImpostor && !playerControl.Data.IsDead)
            {
                playerControl.ClearTasks();
                PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0).Text =
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
        SoundManager.Instance.PlaySound(FinalHideAlertSfx, false, 1f, null);
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

    private IEnumerator BeepAlmostEverySecond()
    {
        while (!IsFinalCountdown)
        {
            float num = currentHideTime / 10f;
            float pitch = 1.5f - num / 2f;
            SoundManager.Instance.PlaySoundImmediate(FinalHideCountdownSfx, false, 1f, pitch, null);
            yield return new WaitForSeconds(1f);
        }

        yield return Effects.Wait(currentFinalHideTime - 10f);
        while (currentFinalHideTime > 0f)
        {
            float num2 = currentFinalHideTime / 10f;
            float pitch2 = 1.5f - num2 / 2f;
            SoundManager.Instance.PlaySoundImmediate(FinalHideCountdownSfx, false, 1f, pitch2, null);
            yield return new WaitForSeconds(1f);
        }

        yield break;
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

    public bool AllTimersExpired()
    {
        return currentHideTime <= 0f && currentFinalHideTime <= 0f;
    }
}
