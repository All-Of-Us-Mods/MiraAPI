using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;

#pragma warning disable SA1403 // File may only contain a single namespace
namespace MiraAPI.Modifiers
{
    public partial class ModifierComponent
    {
        /// <inheritdoc cref="RemoveModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        public void RemoveModifier(BaseModifier modifier)
        {
            RemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="TryRemoveModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        public bool TryRemoveModifier(BaseModifier modifier)
        {
            return TryRemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="AddModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        public BaseModifier? AddModifier(BaseModifier modifier)
        {
            return AddModifier(modifier as IBaseModifier) as BaseModifier;
        }

        /// <summary>
        /// Checks if a player has an active modifier by its type, if the type is an interface.
        /// </summary>
        /// <param name="predicate">The predicate to check the modifier.</param>
        /// <typeparam name="T">The Type of the interface of the Modifier.</typeparam>
        /// <returns>True if the Modifier is present, false otherwise.</returns>
        [HideFromIl2Cpp]
        [Obsolete("Use HasModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public bool HasModifierOfType<T>(Func<T, bool>? predicate = null)
            where T : class
        {
            return ActiveModifiers.Exists(x => x is T modifier && (predicate == null || predicate(modifier)));
        }
    }

    public static partial class ModifierExtensions
    {
        /// <inheritdoc cref="RemoveModifier(PlayerControl, IBaseModifier)"/>
        public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
        {
            player.RemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="TryRemoveModifier(PlayerControl, IBaseModifier)"/>
        public static bool TryRemoveModifier(this PlayerControl player, BaseModifier modifier)
        {
            return player.TryRemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="AddModifier(PlayerControl, IBaseModifier)"/>
        public static BaseModifier? AddModifier(this PlayerControl player, BaseModifier modifier)
        {
            return player.AddModifier(modifier as IBaseModifier) as BaseModifier;
        }

        /// <summary>
        /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="BaseModifier"/>, if the type is an interface.
        /// </summary>
        /// <typeparam name="T">The type of the interface of the <see cref="BaseModifier"/>.</typeparam>
        /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
        /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
        /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="BaseModifier"/> of type <typeparamref name="T"/>, <see langword="false"/> otherwise.</returns>
        [Obsolete("Use HasModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static bool HasModifierOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null)
            where T : class
        {
            return player.GetModifierComponent().HasModifierOfType(predicate);
        }
    }

    public static partial class ModifierManager
    {
        /// <inheritdoc cref="AllModifiers"/>
        [Obsolete("Use AllModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static IReadOnlyList<BaseModifier> Modifiers => [.. AllModifiers.OfType<BaseModifier>()];
    }
}

namespace MiraAPI.Patches
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "I don't know what this means tbh.")]
    public static partial class RoleGuidePatches
    {
        /// <inheritdoc cref="DisplayAdvancedWiki(MatchInfoGuide, IBaseModifier)"/>
        [Obsolete("Use the IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static void DisplayAdvancedWiki(MatchInfoGuide instance, BaseModifier modifier)
        {
            DisplayAdvancedWiki(instance, modifier as IBaseModifier);
        }
    }
}

namespace MiraAPI.Utilities
{
    public static partial class Helpers
    {
        /// <inheritdoc cref="GetTmpIcon(IBaseModifier)"/>
        [Obsolete("Use the IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static string GetTmpIcon(this BaseModifier modifier)
        {
            return (modifier as IBaseModifier).GetTmpIcon();
        }
    }
}
#pragma warning restore SA1403 // File may only contain a single namespace
