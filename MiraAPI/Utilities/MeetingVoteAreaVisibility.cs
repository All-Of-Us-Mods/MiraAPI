using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.Utilities;

internal static class MeetingVoteAreaVisibility
{
    private static readonly Dictionary<Renderer, bool> OriginalRendererStates = [];
    private static readonly HashSet<int> ActiveOwners = [];

    internal static void HideNameplates(Object owner)
    {
        var meetingHud = MeetingHud.Instance;
        if (!owner ||
            !meetingHud ||
            meetingHud.state is not (MeetingHud.MeetingStates.Discussion or
                MeetingHud.MeetingStates.NotVoted or
                MeetingHud.MeetingStates.Voted))
        {
            return;
        }

        ActiveOwners.Add(owner.GetInstanceID());
        foreach (var playerVoteArea in meetingHud.playerStates)
        {
            if (!playerVoteArea)
            {
                continue;
            }

            var buttons = playerVoteArea.Buttons;
            foreach (var renderer in playerVoteArea.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer ||
                    (buttons &&
                     (renderer.transform == buttons.transform || renderer.transform.IsChildOf(buttons.transform))))
                {
                    continue;
                }

                if (!OriginalRendererStates.ContainsKey(renderer))
                {
                    OriginalRendererStates.Add(renderer, renderer.enabled);
                }

                renderer.enabled = false;
            }
        }
    }

    internal static void Restore(Object owner)
    {
        if (!owner || !ActiveOwners.Remove(owner.GetInstanceID()) || ActiveOwners.Count > 0)
        {
            return;
        }

        RestoreAll();
    }

    internal static void RestoreAll()
    {
        ActiveOwners.Clear();
        foreach (var rendererState in OriginalRendererStates)
        {
            if (rendererState.Key)
            {
                rendererState.Key.enabled = rendererState.Value;
            }
        }

        OriginalRendererStates.Clear();
    }
}
