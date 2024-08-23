using Reactor.Utilities;

namespace MiraAPI.Modifiers.Types;
public abstract class TimedModifier : BaseModifier
{
    public abstract float Duration { get; }
    public abstract void OnTimerComplete();

    internal bool timerOngoing = false;

    public void StartTimer(ModifierManager manager)
    {
        if (!timerOngoing)
        {
            Logger<MiraApiPlugin>.Error("Can't start a timer that is already ongoing.");
            return;
        }

        timerOngoing = true;
        Coroutines.Start(manager.ModifierTimer(this));
    }
}