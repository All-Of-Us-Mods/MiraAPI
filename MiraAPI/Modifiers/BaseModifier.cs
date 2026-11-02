using System;
using System.Collections.Generic;
using System.Globalization;
using Il2CppInterop.Runtime.Attributes;
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
public abstract class BaseModifier : IBaseModifier
{
    /// <inheritdoc/>
    public PlayerControl Player { get; internal set; } = null!;

    /// <summary>
    /// Gets the <see cref="Modifiers.ModifierComponent"/> that the modifier is attached to.
    /// </summary>
    public ModifierComponent? ModifierComponent { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the modifier has been initialized.
    /// </summary>
    public bool Initialized { get; internal set; }

    /// <inheritdoc/>
    public Guid UniqueId { get; internal set; } = Guid.Empty;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets a value indicating whether to exclude the "amount" from the displayed amount text within the role guide.
    /// </summary>
    public virtual bool ShowModifierAsUnique => false;

    /// <inheritdoc/>
    public virtual string GetAmountText(int amount, int chance, string source)
    {
        if (amount == 0)
        {
            chance = 0;
        }

        if (ShowModifierAsUnique)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}% ({1})",
                chance.ToString(CultureInfo.InvariantCulture),
                source);
        }

        if (chance == 0)
        {
            amount = 0;
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} at {1}% ({2})",
            amount.ToString(CultureInfo.InvariantCulture),
            chance.ToString(CultureInfo.InvariantCulture),
            source);
    }

    /// <summary>
    /// Gets whether the modifier is forcibly shown or disabled in the wiki screen.
    /// </summary>
    /// <returns><see langword="true"/> if the modifier is always displayed, otherwise <see langword="false"/> if it is never displayable, or <see langword="null"/> if it is dictated by amount and chance.</returns>
    public virtual bool? ForceShowModifierOnWiki => false;

    /// <inheritdoc/>
    public virtual GameObject GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, SpriteRenderer titleIcon, Scroller parent)
    {
        var abilities = WikiAbilities;
        return Helpers.CreateAdvancedWikiPage(
            guide,
            titleText,
            titleIcon,
            parent,
            ModifierNameLocale,
            $"{ModifierName} ({ModifierCategoryTitle})".TrimStart(),
            ModifierIcon ?? MiraAssets.IconNeutral.LoadAsset(),
            ModifierWikiDescription,
            abilities);
    }

    /// <summary>
    /// Gets the list of abilities or other similar information to display below the text of a modifier's wiki page.
    /// </summary>
    [HideFromIl2Cpp]
    public virtual List<AdvancedWikiAbilityDescription> WikiAbilities => [];

    /// <inheritdoc/>
    public virtual LoadableAsset<Sprite>? ModifierIcon => null;

    /// <inheritdoc/>
    [HideFromIl2Cpp]
    public virtual TMP_SpriteAsset IconTmp => null!;

    /// <inheritdoc/>
    public virtual bool HideOnUi => GetDescription() == string.Empty;

    /// <inheritdoc/>
    public virtual bool ShowInFreeplay => false;

    /// <inheritdoc/>
    public virtual Color FreeplayFileColor => Color.gray;

    /// <summary>
    /// Gets the <see cref="Color"/> of the modifier, used in the wiki and other UI outside freeplay.
    /// </summary>
    public virtual Color GeneralColor => FreeplayFileColor;

    /// <inheritdoc/>
    public virtual bool Unique => true;

    /// <inheritdoc/>
    public virtual string GetDescription()
    {
        return string.Empty;
    }

    /// <inheritdoc/>
    public virtual void OnActivate()
    {
    }

    /// <inheritdoc/>
    public virtual void OnDeactivate()
    {
    }

    /// <inheritdoc/>
    public virtual void Update()
    {
    }

    /// <inheritdoc/>
    public virtual void FixedUpdate()
    {
    }

    /// <inheritdoc/>
    public virtual void OnDeath(DeathReason reason)
    {
    }

    /// <inheritdoc/>
    public virtual void OnMeetingStart()
    {
    }

    /// <inheritdoc/>
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
