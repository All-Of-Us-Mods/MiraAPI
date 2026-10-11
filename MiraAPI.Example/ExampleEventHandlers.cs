using System.Globalization;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Example.Buttons.Freezer;
using MiraAPI.Example.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;

namespace MiraAPI.Example;

public static class ExampleEventHandlers
{
    public static void Initialize()
    {
        // You can register event handlers with the MiraEventManager class
        var handle = MiraEventManager.RegisterEventHandler<BeforeMurderEvent>(@event =>
        {
            Logger<ExamplePlugin>.Info($"{@event.Source.Data.PlayerName} is about to kill {@event.Target.Data.PlayerName}");
        });

        MiraEventManager.RegisterEventHandler<AfterMurderEvent>(@event =>
        {
            Logger<ExamplePlugin>.Info($"{@event.Source.Data.PlayerName} has killed {@event.Target.Data.PlayerName}");
        });

        MiraEventManager.RegisterEventHandler<CompleteTaskEvent>(@event =>
        {
            Logger<ExamplePlugin>.Info($"{@event.Player.Data.PlayerName} completed {@event.Task.TaskType.ToString()}");
        });

        // You can also unregister event handlers using the handle returned from RegisterEventHandler.
        MiraEventManager.UnregisterEventHandler(handle);
    }

    // If you want to check vanilla buttons, use something like this!
    [RegisterEvent]
    public static void VanillaButtonClickHandler(VanillaButtonClickEvent @event)
    {
        Logger<ExamplePlugin>.Info($"{@event.Button.buttonLabelText.text} is being used!");
        if (@event.PlayerTarget != null)
        {
            Logger<ExamplePlugin>.Info($"{@event.PlayerTarget.Data.PlayerName} is being targeted!");
        }
        else if (@event.VentTarget != null)
        {
            Logger<ExamplePlugin>.Info($"Vent with id {@event.VentTarget.Id.ToString(CultureInfo.InvariantCulture)} is being targeted!");
        }
    }

    // If you want to add extra votes to a player, do something like this.
    [RegisterEvent]
    public static void StartMeetingEvent(StartMeetingEvent _)
    {
        foreach (var plr in PlayerControl.AllPlayerControls.ToArray().Where(player => player.Data.Role is MayorRole))
        {
            plr.GetVoteData().IncreaseRemainingVotes(1);
        }
    }

    // Example of using the Voting API to make a role similar to the Prosecutor from Town of Us.
    [RegisterEvent(15)]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        if (@event.VoteData.Owner.Data.Role is not NeutralKillerRole) return;

        @event.VoteData.SetRemainingVotes(0);

        for (var i = 0; i < 5; i++)
        {
            @event.VoteData.VoteForPlayer(@event.TargetId);
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray().Where(player => player != @event.VoteData.Owner))
        {
            plr.GetVoteData().Votes.Clear();
            plr.GetVoteData().VotesRemaining = 0;
        }

        @event.Cancel();
    }

    // Events can be registered using an attribute as well.
    [RegisterEvent]
    public static void UpdateSystemEventHandler(UpdateSystemEvent @event)
    {
        Logger<ExamplePlugin>.Error(@event.SystemType.ToString());
    }

    // Example event handler
    [RegisterEvent(1)]
    public static void FreezeButtonClickHandler(MiraButtonClickEvent<FreezeButton> @event)
    {
        Logger<ExamplePlugin>.Warning("Freeze button clicked!");

        if (PlayerControl.LocalPlayer.Data.PlayerName != "stupid") return;

        @event.Cancel();
        @event.Button.SetTimer(15f);
    }

    // Example event handler
    [RegisterEvent]
    public static void FreezeButtonCancelledHandler(MiraButtonCancelledEvent<FreezeButton> @event)
    {
        Logger<ExamplePlugin>.Warning("Freeze button cancelled!");
        @event.Button.OverrideName("Freeze Canceled");
    }
}
