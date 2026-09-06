using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Components;
using UnityEngine;

namespace MiraAPI.MeetingAbilities;

/// <summary>
/// Class for managing all registered meeting abilities.
/// </summary>
public static class MeetingButtonManager
{
    /// <summary>
    /// Gets a list if==of all registered <see cref="TargetedMeetingButton"/>.
    /// </summary>
    public static List<TargetedMeetingButton> TargetedButtons { get; internal set; } = [];

    /// <summary>
    /// Gets a list if==of all registered <see cref="MeetingActionButton"/>.
    /// </summary>
    public static List<MeetingActionButton> UntargetedButtons { get; internal set; } = [];

    internal static bool RegisterTargetedMeetingButton(Type type, MiraPluginInfo info)
    {
        if (!typeof(TargetedMeetingButton).IsAssignableFrom(type))
        {
            return false;
        }

        var button = (Activator.CreateInstance(type) as TargetedMeetingButton)!;
        info.InternalTargetedMeetingButtons.Add(button);
        TargetedButtons.Add(button);
        return true;
    }

    internal static bool RegisterMeetingButton(Type type, MiraPluginInfo info)
    {
        if (!typeof(MeetingActionButton).IsAssignableFrom(type))
        {
            return false;
        }

        var button = (Activator.CreateInstance(type) as MeetingActionButton)!;
        info.InternalMeetingButtons.Add(button);
        UntargetedButtons.Add(button);
        return true;
    }

    /// <summary>
    /// Called on <see cref="TutorialManager.RunTutorial"/> and <see cref="IntroCutscene.CoBegin"/>/<see cref="IntroCutscene.OnDestroy"/>.
    /// </summary>
    public static void OnGameStart()
    {
        foreach (var ability in TargetedButtons.Where(x => x.UsesMode == MeetingButtonUsesMode.PerGame))
        {
            ability.UsesLeft = ability.MaxUses;
        }
        foreach (var ability in UntargetedButtons.Where(x => x.UsesMode == MeetingButtonUsesMode.PerGame))
        {
            ability.UsesLeft = ability.MaxUses;
        }
    }

    /// <summary>
    /// Called on <see cref="MeetingHud.Start"/>. Handles button creation, and ability uses.
    /// </summary>
    /// <param name="meetingHud">The <see cref="MeetingHud"/> instance.</param>
    public static void OnMeetingStart(MeetingHud meetingHud)
    {
        // Untargeted buttons parent.
        var actionButtonsParent = new GameObject("ActionsButtonParent").AddComponent<CenteredGridArrange>();
        actionButtonsParent.transform.SetParent(meetingHud.MeetingAbilityButton.transform.parent, meetingHud.MeetingAbilityButton.transform.parent);
        actionButtonsParent.CellSize = new Vector2(0.7f, 0.7f);
        actionButtonsParent.transform.localPosition = new Vector3(0, -2.1f, -10);
        meetingHud.MeetingAbilityButton.transform.SetParent(actionButtonsParent.transform);

        // Untargeted buttons creation.
        foreach (var ability in UntargetedButtons)
        {
            try
            {
                ability.CreateButton(actionButtonsParent.transform);
                if (ability.UsesMode == MeetingButtonUsesMode.PerMeeting) ability.UsesLeft = ability.MaxUses;
                ability.Timer = ability.InitialCooldown;
            }
            catch (Exception e)
            {
                Error($"Failed to create meeting button {ability.Name}: {e.Message}");
            }
        }

        // Targeted buttons creation.
        foreach (var ability in TargetedButtons)
        {
            try
            {
                foreach (var playerVoteArea in meetingHud.playerStates)
                {
                    var btn = ability.CreateButton(playerVoteArea);
                    btn.transform.SetParent(playerVoteArea.Buttons.transform);
                }
                var btn2 = ability.CreateButton(meetingHud.SkipVoteButton);
                btn2.transform.SetParent(meetingHud.SkipVoteButton.Buttons.transform);

                if (ability.UsesMode == MeetingButtonUsesMode.PerMeeting) ability.UsesLeft = ability.MaxUses;
                if (ability is MultiTargetMeetingButton multiAbility) multiAbility.Targets = [];
            }
            catch (Exception e)
            {
                Error($"Failed to create targeted meeting button {ability.Name}: {e.Message}");
            }
        }
        foreach (var playerVoteArea in meetingHud.playerStates)
        {
            playerVoteArea.CancelButton.transform.SetAsFirstSibling();
        }
        meetingHud.SkipVoteButton.CancelButton.transform.SetAsFirstSibling();
    }
}
