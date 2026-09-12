using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Hud;

/// <summary>
/// Custom Player Menu using the <see cref="ShapeshifterPanel"/> as a base.
/// </summary>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity Convention.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public class CustomPlayerMenu : CustomMultiSelectMenu<PlayerControl>
{
    [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Read above.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<ShapeshifterPanel> potentialVictims;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Creates a <see cref="CustomPlayerMenu"/>.
    /// </summary>
    /// <returns>New <see cref="CustomPlayerMenu"/> object.</returns>
    public static CustomPlayerMenu Create()
    {
        return Create<CustomPlayerMenu>();
    }

    /// <summary>
    /// Begins/opens the custom player menu.
    /// </summary>
    /// <param name="playerMatch">Function to determine if player should show in the custom menu.</param>
    /// <param name="onClick"><see cref="PassiveButton.OnClick"/> action for player.</param>
    /// <param name="shouldConfirm">Whether the set of both selections should be confirmed manually.</param>
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl?> onClick, bool shouldConfirm = false)
    {
        Begin(PlayerControl.AllPlayerControls.ToArray().Where(playerMatch), onClick, shouldConfirm);
        potentialVictims = EntryPanels;
    }

    /// <summary>
    /// Begins/opens the custom player menu.
    /// </summary>
    /// <param name="playerMatch">Function to determine if player should show in the custom menu.</param>
    /// <param name="onClick"><see cref="PassiveButton.OnClick"/> action for the two-player selection.</param>
    /// <param name="shouldConfirm">Whether the set of both selections should be confirmed manually.</param>
    /// <param name="canRepeat">If the same entry can be selected both times, else unselect entry on click.</param>
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl?, PlayerControl?> onClick, bool shouldConfirm = false, bool canRepeat = false)
    {
        Begin(PlayerControl.AllPlayerControls.ToArray().Where(playerMatch), onClick, shouldConfirm, canRepeat);
        potentialVictims = EntryPanels;
    }

    /// <inheritdoc/>
    protected override void SetupPanelEntry(ShapeshifterPanel panel, int i, PlayerControl entry, Action onClick)
    {
        var flag = PlayerControl.LocalPlayer.Data.Role.NameColor == entry.Data.Role.NameColor;
        panel.SetPlayer(i, entry.Data, (Il2CppSystem.Action)onClick);
        panel.NameText.color = flag ? entry.Data.Role.NameColor : Color.white;
    }
}
