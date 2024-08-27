using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Modifiers.Types;
public abstract class TimedModifier : BaseModifier
{
    public abstract float Duration { get; }
    public virtual bool AutoStart => false;
    public abstract void OnTimerComplete();

    public bool TimerActive { get; protected set; }

    public float TimeRemaining { get; protected set; }

    public override void FixedUpdate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        if (TimeRemaining > 0 && TimerActive)
        {
            TimeRemaining -= Time.fixedDeltaTime;
        }
        else
        {
            TimerActive = false;
            TimeRemaining = Duration;
            OnTimerComplete();
        }
    }

    public void StartTimer()
    {
        if (TimerActive)
        {
            Logger<MiraApiPlugin>.Error("Can't start a timer that has already been started.");
            return;
        }

        TimerActive = true;
        TimeRemaining = Duration;
    }
}