using System;
using MiraAPI.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Modifiers.Types;

/// <summary>
/// The base class for all timed modifiers. Timed modifiers have a duration and can be started and stopped.
/// </summary>
public abstract class TimedModifier : BaseModifier
{
    /// <summary>
    /// Gets the duration of the modifier.
    /// </summary>
    public abstract float Duration { get; }

    /// <summary>
    /// Gets a value indicating whether the timer should start automatically when added.
    /// </summary>
    public virtual bool AutoStart => false;

    /// <summary>
    /// Gets a value indicating whether the modifier should be removed when the timer completes.
    /// </summary>
    public virtual bool RemoveOnComplete => true;

    /// <summary>
    /// Called when the timer completes.
    /// </summary>
    public abstract void OnTimerComplete();

    /// <summary>
    /// Gets or sets a value indicating whether the timer is active.
    /// </summary>
    public bool TimerActive { get; protected set; }

    /// <summary>
    /// Gets or sets the time remaining on the timer.
    /// </summary>
    public float TimeRemaining { get; protected set; }

    /// <summary>
    /// Gets the HUD information for Timed Modifier, including the time remaining.
    /// </summary>
    /// <returns>A string with the hud information + timer.</returns>
    public override string GetHudString()
    {
        return base.GetHudString() + $" <size=70%>({Math.Round(Duration - TimeRemaining, 0)}s/{Duration}s)</size>";
    }

    /// <summary>
    /// The FixedUpdate method for timed modifiers. Automatically handles the timer logic.
    /// </summary>
    public override void FixedUpdate()
    {
        if (Player?.AmOwner == false)
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

            if (RemoveOnComplete)
            {
                ModifierComponent?.RemoveModifier(ModifierId);
            }
        }
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
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
