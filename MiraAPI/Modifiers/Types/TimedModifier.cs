using Reactor.Utilities;

namespace MiraAPI.Modifiers.Types;
public abstract class TimedModifier : BaseModifier
{
    public abstract float Duration { get; }
    public abstract void OnTimerComplete();

    internal bool TimerActive;

    public void StartTimer()
    {
        if (TimerActive)
        {
            Logger<MiraApiPlugin>.Error("Can't start a timer that is already started.");
            return;
        }

        TimerActive = true;
        Coroutines.Start(ModifierComponent.ModifierTimer(this));
    }
}