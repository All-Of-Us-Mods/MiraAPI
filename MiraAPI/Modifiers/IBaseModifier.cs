using System;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// Base interface for all modifiers.
/// </summary>
public interface IBaseModifier : IOptionable
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that the modifier is attached to.
    /// </summary>
    PlayerControl Player { get; }

    /// <summary>
    /// Gets the unique ID of the modifier.
    /// </summary>
    Guid UniqueId { get; }

    /// <summary>
    /// Gets the type ID of the modifier.
    /// </summary>
    uint TypeId { get; }

    /// <summary>
    /// Gets the modifier name.
    /// </summary>
    string ModifierName { get; }

    /// <summary>
    /// Gets the displayed amount and chance of the modifier. Used in the role guide.
    /// By default, 0 amount or chance results in a complete 0 amount and chance, so override if necessary.
    /// </summary>
    /// <param name="amount">The amount of the modifier.</param>
    /// <param name="chance">The chance of the modifier.</param>
    /// <param name="source">The mod source of the modifier.</param>
    /// <returns>The <see cref="string"/> result.</returns>
    string GetAmountText(int amount, int chance, string source);

    /// <summary>
    /// Gets the information to display in the wiki.
    /// </summary>
    /// <param name="guide">The guide object.</param>
    /// <param name="titleText">The title text object.</param>
    /// <param name="titleIcon">The title's icon.</param>
    /// <param name="parent">The scroller parent.</param>
    /// <returns>The <see cref="GameObject"/> of the wiki.</returns>
    GameObject GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, SpriteRenderer titleIcon, Scroller parent);

    /// <summary>
    /// Gets the modifier icon. Useless if <see cref="HideOnUi"/> is <see langword="true"/>.
    /// </summary>
    LoadableAsset<Sprite>? ModifierIcon { get; }

    /// <summary>
    /// Gets the <see cref="TMP_SpriteAsset"/> for the Modifier Icon.
    /// </summary>
    [HideFromIl2Cpp]
    TMP_SpriteAsset IconTmp { get; }

    /// <summary>
    /// Gets a value indicating whether the modifier is hidden on the UI. Will be hidden either way if no description is provided.
    /// </summary>
    bool HideOnUi { get; }

    /// <summary>
    /// Gets a value indicating whether the modifier is shown in the freeplay menu.
    /// </summary>
    bool ShowInFreeplay { get; }

    /// <summary>
    /// Gets a value indicating the <see cref="Color"/> that should be used for the modifier within freeplay.
    /// </summary>
    Color FreeplayFileColor { get; }

    /// <summary>
    /// Gets a value indicating whether the modifier is unique. If <see langword="true"/>, the player can only have one instance of this modifier.
    /// </summary>
    bool Unique { get; }

    /// <summary>
    /// Gets the HUD description for this modifier. Does nothing if <see cref="HideOnUi"/> is <see langword="true"/>. Required to be visible on UI.
    /// </summary>
    /// <returns>The description string for the HUD.</returns>
    string GetDescription();

    /// <summary>
    /// Called when the modifier is activated.
    /// </summary>
    void OnActivate();

    /// <summary>
    /// Called when the modifier is deactivated.
    /// </summary>
    void OnDeactivate();

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.Update"/> method.
    /// </summary>
    void Update();

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.FixedUpdate"/> method.
    /// </summary>
    void FixedUpdate();

    /// <summary>
    /// Called when the player dies.
    /// </summary>
    /// <param name="reason">The Death Reason.</param>
    void OnDeath(DeathReason reason);

    /// <summary>
    /// Called when a meeting starts.
    /// </summary>
    void OnMeetingStart();

    /// <summary>
    /// Determines whether the player can vent.
    /// </summary>
    /// <returns><see langword="true"/> if the player can vent, <see langword="false"/> otherwise. <see langword="null"/> for no effect.</returns>
    bool? CanVent();

    /// <summary>
    /// Removes this modifier instance from the player.
    /// </summary>
    void RemoveSelf();
}
