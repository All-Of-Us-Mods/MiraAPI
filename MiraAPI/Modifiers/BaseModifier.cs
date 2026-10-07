using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// Base class for all modifiers.
/// </summary>
public abstract class BaseModifier : IOptionable
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that the modifier is attached to.
    /// </summary>
    public PlayerControl Player { get; internal set; } = null!;

    /// <summary>
    /// Gets the <see cref="Modifiers.ModifierComponent"/> that the modifier is attached to.
    /// </summary>
    public ModifierComponent? ModifierComponent { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the modifier has been initialized.
    /// </summary>
    public bool Initialized { get; internal set; }

    /// <summary>
    /// Gets the unique ID of the modifier.
    /// </summary>
    public Guid UniqueId { get; internal set; } = Guid.Empty;

    /// <summary>
    /// Gets the type ID of the modifier.
    /// </summary>
    public uint TypeId => ModifierManager.GetModifierTypeId(GetType()) ?? throw new InvalidOperationException("Modifier is not registered.");

    /// <summary>
    /// Gets the parent mod of the modifier.
    /// </summary>
    public MiraPluginInfo ParentMod => Array.Find(
        MiraPluginManager.Instance.RegisteredPlugins,
        x => x.InternalModifiers.Exists(y => y.TypeId == TypeId))
        ?? throw new InvalidOperationException("Modifier is not registered.");

    /// <summary>
    /// Gets the id part used to build the modifier's translation keys. It is recommended to simply call it what the modifier is, as other mods may utilize it.
    /// </summary>
    public virtual string IdPart => GetType().Name;

    /// <summary>
    /// Gets the id part used to build the modifier's translation keys. It is recommended to call it ModGuid.Modifier.Type.
    /// </summary>
    public virtual string IdPrefix => GetType().Namespace!;

    /// <summary>
    /// Gets the modifier name.
    /// </summary>
    public virtual string ModifierName => MiraLocaleManager.Get(ModifierNameLocale);

    /// <summary>
    /// Gets the modifier's name id for localization.
    /// </summary>
    public virtual string ModifierNameLocale => MiraLocaleManager.BuildTranslationId(IdPrefix, IdPart);

    /// <summary>
    /// Gets the medium description of the modifier. Used in the modifier guide and options menu.
    /// </summary>
    public virtual string ModifierMedDescription => MiraLocaleManager.Get(ModifierMedDescriptionLocale, GetDescription());

    /// <summary>
    /// Gets the modifier's medium description id for localization.
    /// </summary>
    public virtual string ModifierMedDescriptionLocale => MiraLocaleManager.BuildTranslationId(IdPrefix, IdPart, "MedDescription");

    /// <summary>
    /// Gets the wiki description of the modifier. Used in the wiki and normally appends the options text as well.
    /// </summary>
    public virtual string ModifierWikiDescription => MiraLocaleManager.Get(ModifierWikiDescriptionLocale, GetDescription()) +
                                                     Helpers.GetOptionsText(GetType());

    /// <summary>
    /// Gets the modifier's wiki description id for localization.
    /// </summary>
    public virtual string ModifierWikiDescriptionLocale => MiraLocaleManager.BuildTranslationId(IdPrefix, IdPart, "WikiDescription");

    /// <summary>
    /// Gets the category of the modifier. Used in the wiki.
    /// </summary>
    public virtual string ModifierCategoryTitle => MiraLocaleManager.Get("Modifier");

    /// <summary>
    /// Gets whether the modifier is forcibly shown or disabled in the wiki screen.
    /// </summary>
    /// <returns><see langword="true"/> if the modifier is always displayed, otherwise <see langword="false"/> if it is never displayable, or <see langword="null"/> if it is dictated by amount and chance.</returns>
    public virtual bool? ForceShowModifierOnWiki => false;

    /// <summary>
    /// Gets the information to display in the wiki.
    /// </summary>
    /// <param name="guide">The guide object.</param>
    /// <param name="titleText">The title text object.</param>
    /// <param name="parent">The scroller parent.</param>
    /// <returns>The <see cref="GameObject"/> of the wiki.</returns>
    public virtual GameObject GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, Scroller parent)
    {
        var abilities = WikiAbilities;
        return Helpers.CreateAdvancedWikiPage(
            guide,
            titleText,
            parent,
            ModifierNameLocale,
            ModifierName + $" ({ModifierCategoryTitle})",
            ModifierWikiDescription,
            abilities);
    }

    /// <summary>
    /// Gets the list of abilities or other similar information to display below the text of a modifier's wiki page.
    /// </summary>
    [HideFromIl2Cpp]
    public virtual List<AdvancedWikiAbilityDescription> WikiAbilities => [];

    /// <summary>
    /// Gets the modifier icon. Useless if <see cref="HideOnUi"/> is <see langword="true"/>.
    /// </summary>
    public virtual LoadableAsset<Sprite>? ModifierIcon => null;

    /// <summary>
    /// Gets the <see cref="TMP_SpriteAsset"/> for the Modifier Icon.
    /// </summary>
    [HideFromIl2Cpp]
    public virtual TMP_SpriteAsset IconTmp => null!;

    /// <summary>
    /// Gets a value indicating whether the modifier is hidden on the UI. Will be hidden either way if no description is provided.
    /// </summary>
    public virtual bool HideOnUi => GetDescription() == string.Empty;

    /// <summary>
    /// Gets a value indicating whether the modifier is shown in the freeplay menu.
    /// </summary>
    public virtual bool ShowInFreeplay => false;

    /// <summary>
    /// Gets a value indicating the <see cref="Color"/> that should be used for the modifier within freeplay.
    /// </summary>
    public virtual Color FreeplayFileColor => Color.gray;

    /// <summary>
    /// Gets the <see cref="Color"/> of the modifier, used in the wiki and other UI outside freeplay.
    /// </summary>
    public virtual Color GeneralColor => FreeplayFileColor;

    /// <summary>
    /// Gets a value indicating whether the modifier is unique. If <see langword="true"/>, the player can only have one instance of this modifier.
    /// </summary>
    public virtual bool Unique => true;

    /// <summary>
    /// Gets the HUD description for this modifier. Does nothing if <see cref="HideOnUi"/> is <see langword="true"/>. Required to be visible on UI.
    /// </summary>
    /// <returns>The description string for the HUD.</returns>
    public virtual string GetDescription()
    {
        return string.Empty;
    }

    /// <summary>
    /// Called when the modifier is activated.
    /// </summary>
    public virtual void OnActivate()
    {
    }

    /// <summary>
    /// Called when the modifier is deactivated.
    /// </summary>
    public virtual void OnDeactivate()
    {
    }

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.Update"/> method.
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.FixedUpdate"/> method.
    /// </summary>
    public virtual void FixedUpdate()
    {
    }

    /// <summary>
    /// Called when the player dies.
    /// </summary>
    /// <param name="reason">The Death Reason.</param>
    public virtual void OnDeath(DeathReason reason)
    {
    }

    /// <summary>
    /// Called when a meeting starts.
    /// </summary>
    public virtual void OnMeetingStart()
    {
    }

    /// <summary>
    /// Determines whether the player can vent.
    /// </summary>
    /// <returns><see langword="true"/> if the player can vent, <see langword="false"/> otherwise. <see langword="null"/> for no effect.</returns>
    public virtual bool? CanVent()
    {
        return null;
    }

    /// <summary>
    /// Removes this modifier instance from the player.
    /// </summary>
    public void RemoveSelf()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
