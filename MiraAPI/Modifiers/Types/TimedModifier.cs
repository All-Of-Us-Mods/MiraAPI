namespace MiraAPI.Modifiers.Types;
public abstract class TimedModifier : BaseModifier
{
    public abstract float Duration { get; }
    public abstract bool StartOnAssignment { get; }
    public abstract bool RemoveOnCompletion { get; }
}