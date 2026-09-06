using System.Collections.Generic;
using System.Linq;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace MiraAPI.MeetingAbilities;

/// <summary>
/// Abstract class for creating multi target meeting abilities.
/// </summary>
public abstract class MultiTargetMeetingButton : TargetedMeetingButton
{
    /// <summary>
    /// Gets he <see cref="LoadableAsset{Sprite}"/> used for when the button is on.
    /// </summary>
    public abstract LoadableAsset<Sprite> SpriteActive { get; }

    /// <summary>
    /// Gets the required targets count.
    /// </summary>
    public abstract int TargetsCount { get; }

    /// <summary>
    /// Gets or sets all selected targets.
    /// </summary>
    public Dictionary<PlayerVoteArea, MeetingAbilityBehaviour> Targets { get; set; } = [];

    /// <inheritdoc />
    public override void ClickHandler(MeetingAbilityBehaviour button, PlayerVoteArea playerVoteArea)
    {
        if (UsesLeft <= 0 && LimitedUses) return;
        if (Timer > 0) return;
        var toggle = !Targets.Remove(playerVoteArea);
        if (toggle) Targets.Add(playerVoteArea, button);
        button.Renderer.sprite = toggle ? SpriteActive.LoadAsset() : Sprite.LoadAsset();
        if (Targets.Count >= TargetsCount)
        {
            playerVoteArea.Cancel();
            HandleFinish();
            return;
        }
        OnSelect(playerVoteArea, toggle);
    }

    /// <summary>
    /// Handles the finish logic, increasing timer, and decreasing uses.
    /// </summary>
    public virtual void HandleFinish()
    {
        Targets = Targets.Take(TargetsCount).ToDictionary(pair => pair.Key, pair => pair.Value);
        foreach (var button in Targets.Values)
        {
            button.Renderer.sprite = Sprite.LoadAsset();
        }
        Timer = Cooldown;
        UsesLeft -= 1;
        OnFinish();
        Targets = [];
    }

    /// <summary>
    /// Callback method for when a player is selected.
    /// </summary>
    /// <param name="playerVoteArea">The target <see cref="PlayerVoteArea"/>.</param>
    /// <param name="toggle">Whether the button was toggled on/off.</param>
    protected abstract void OnSelect(PlayerVoteArea playerVoteArea, bool toggle);

    /// <summary>
    /// Callback method for the button click.
    /// USE <see cref="OnSelect"/> INSTEAD!!!.
    /// </summary>
    /// <param name="playerVoteArea">The target <see cref="PlayerVoteArea"/>.</param>
    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
    }

    /// <summary>
    /// Callback method for when enough targets are chosen.
    /// </summary>
    public abstract void OnFinish();

    /// <inheritdoc />
    public override MeetingAbilityBehaviour CreateButton(PlayerVoteArea playerVoteArea)
    {
        Targets = [];
        var button = base.CreateButton(playerVoteArea);
        button.Button.OnClick = new Button.ButtonClickedEvent();
        button.Button.OnClick.AddListener(new System.Action(() => { ClickHandler(button, playerVoteArea); }));
        return button;
    }
}
