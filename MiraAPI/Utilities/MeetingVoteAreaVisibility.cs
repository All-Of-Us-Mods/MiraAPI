using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.Utilities;

internal static class MeetingVoteAreaVisibility
{
    private static readonly Dictionary<Renderer, bool> originalRendererStates = [];
    private static readonly HashSet<int> activeOwners = [];

    internal static void HideNameplates(UnityEngine.Object owner)
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

        activeOwners.Add(owner.GetInstanceID());
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

                if (!originalRendererStates.ContainsKey(renderer))
                {
                    originalRendererStates.Add(renderer, renderer.enabled);
                }

                renderer.enabled = false;
            }
        }
    }

    internal static void Restore(UnityEngine.Object owner)
    {
        if (!owner || !activeOwners.Remove(owner.GetInstanceID()) || activeOwners.Count > 0)
        {
            return;
        }

        RestoreAll();
    }

    internal static void RestoreAll()
    {
        activeOwners.Clear();
        foreach (var rendererState in originalRendererStates)
        {
            if (rendererState.Key)
            {
                rendererState.Key.enabled = rendererState.Value;
            }
        }

        originalRendererStates.Clear();
    }
}
