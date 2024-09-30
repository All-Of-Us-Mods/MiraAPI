using MiraAPI.Events;
using MiraAPI.Events.Attributes;

namespace MiraAPI.Example;

[RegisterEvent]
public class ExampleEvent : AbstractEvent
{
    public ExampleEvent(PlayerControl target)
    {
        TargetPlayer = target;
    }

    public PlayerControl TargetPlayer { get; private set; }
}