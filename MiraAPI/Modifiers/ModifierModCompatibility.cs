using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;

#pragma warning disable SA1403 // File may only contain a single namespace
namespace MiraAPI.Modifiers
{
    public partial class ModifierComponent
    {
        /// <summary>
        /// Gets the active <see cref="BaseModifier"/>s on the player.
        /// </summary>
        [HideFromIl2Cpp]
        [Obsolete("Use ActiveBaseModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public ImmutableList<BaseModifier> ActiveModifiers => ActiveBaseModifiers.OfType<BaseModifier>().ToImmutableList();

        /// <inheritdoc cref="RemoveModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public void RemoveModifier(BaseModifier modifier)
        {
            RemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="TryRemoveModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public bool TryRemoveModifier(BaseModifier modifier)
        {
            return TryRemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="AddModifier(IBaseModifier)"/>
        [HideFromIl2Cpp]
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public BaseModifier? AddModifier(BaseModifier modifier)
        {
            return AddModifier(modifier as IBaseModifier) as BaseModifier;
        }

        /// <summary>
        /// Gets a collection of <see cref="BaseModifier"/>s by type, if the type is an interface.
        /// </summary>
        /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
        /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s of type <typeparamref name="T"/>.</returns>
        [HideFromIl2Cpp]
        [Obsolete("Use GetModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public IEnumerable<T> GetModifiersOfType<T>(Func<T, bool>? predicate = null)
            where T : class
        {
            return ActiveBaseModifiers.OfType<T>().Where(x => predicate == null || predicate(x));
        }

        /// <summary>
        /// Tries to get a <see cref="BaseModifier"/> by its type, if the type is an interface.
        /// </summary>
        /// <param name="modifier">The <see cref="BaseModifier"/> of type <typeparamref name="T"/> or null.</param>
        /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
        /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
        /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> of type <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
        [HideFromIl2Cpp]
        [Obsolete("Use TryGetModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public bool TryGetModifierOfType<T>([NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null)
            where T : class
        {
            modifier = GetModifierOfType(predicate);
            return modifier != null;
        }

        /// <summary>
        /// Gets a <see cref="BaseModifier"/> by its type, if the type is an interface.
        /// </summary>
        /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
        /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
        /// <returns>The <see cref="BaseModifier"/> of type <typeparamref name="T"/> if it is found, <see langword="null"/> otherwise.</returns>
        [HideFromIl2Cpp]
        [Obsolete("Use GetModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public T? GetModifierOfType<T>(Func<T, bool>? predicate = null)
            where T : class
        {
            return GetModifiersOfType(predicate).FirstOrDefault();
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
            return ActiveBaseModifiers.Exists(x => x is T modifier && (predicate == null || predicate(modifier)));
        }

        /// <summary>
        /// Checks if a player has an active modifier by its type, if the type is an interface.
        /// </summary>
        /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
        /// <param name="predicate">The predicate to check the modifier.</param>
        /// <typeparam name="T">The Type of the interface of the Modifier.</typeparam>
        /// <returns>True if the Modifier is present, false otherwise.</returns>
        [HideFromIl2Cpp]
        [Obsolete("Use HasModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public bool HasModifierOfType<T>(bool checkInactive, Func<T, bool>? predicate = null)
            where T : class
        {
            return ActiveBaseModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
            bool MatchExpr(IBaseModifier bm)
            {
                return bm is T modifier && (predicate == null || predicate(modifier));
            }
        }
    }

    public static partial class ModifierExtensions
    {
        /// <inheritdoc cref="RemoveModifier(PlayerControl, IBaseModifier)"/>
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
        {
            player.RemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="TryRemoveModifier(PlayerControl, IBaseModifier)"/>
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static bool TryRemoveModifier(this PlayerControl player, BaseModifier modifier)
        {
            return player.TryRemoveModifier(modifier as IBaseModifier);
        }

        /// <inheritdoc cref="AddModifier(PlayerControl, IBaseModifier)"/>
        [Obsolete("Use IBaseModifier overload instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
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

        /// <summary>
        /// Tries to get a <see cref="BaseModifier"/> by its type, if the type is an interface.
        /// </summary>
        /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
        /// <param name="modifier">The <see cref="BaseModifier"/> of type <typeparamref name="T"/> or null.</param>
        /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> of type <typeparamref name="T"/> by.</param>
        /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
        /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> of type <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
        [Obsolete("Use TryGetModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static bool TryGetModifierOfType<T>(this PlayerControl player, [NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null)
            where T : class
        {
            return player.GetModifierComponent().TryGetModifierOfType(out modifier, predicate);
        }

        /// <summary>
        /// Gets a specific <see cref="BaseModifier"/> of type <typeparamref name="T"/> from the <see cref="PlayerControl"/>, if the type is an interface.
        /// </summary>
        /// <typeparam name="T">The type of the interface of the modifier.</typeparam>
        /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
        /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
        /// <returns>The <see cref="BaseModifier"/> of type <typeparamref name="T"/> if found, <see langword="null"/> otherwise.</returns>
        [Obsolete("Use GetModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static T? GetModifierOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null)
            where T : class
        {
            return player.GetModifierComponent().GetModifierOfType(predicate);
        }

        /// <summary>
        /// Gets all <see cref="BaseModifier"/>s of a specific type from the <see cref="PlayerControl"/>, if the type is an interface.
        /// </summary>
        /// <typeparam name="T">The type of the interface of the <see cref="BaseModifier"/>s.</typeparam>
        /// <param name="player">The PlayerControl instance.</param>
        /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s of type <typeparamref name="T"/>.</returns>
        [Obsolete("Use GetModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public static IEnumerable<T> GetModifiersOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null)
            where T : class
        {
            return player.GetModifierComponent().GetModifiersOfType(predicate);
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

namespace MiraAPI.Modifiers.ModifierDisplay
{
    public partial class ModifierUiComponent
    {
        /// <summary>
        /// Gets the <see cref="BaseModifier"/> which this component is for.
        /// </summary>
        [HideFromIl2Cpp]
        [Obsolete("Use BaseModifier instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public BaseModifier? Modifier => BaseModifier as BaseModifier;
    }

    public partial class ModifierDisplayComponent
    {
        /// <summary>
        /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of the created <see cref="ModifierUiComponent"/>s.
        /// </summary>
        [HideFromIl2Cpp]
        [Obsolete("Use BaseModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public ReadOnlyDictionary<BaseModifier, ModifierUiComponent> Modifiers =>
            new(_modifiers.Where(x => x.Key is BaseModifier).ToDictionary(x => (x.Key as BaseModifier)!, x => x.Value));
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

namespace MiraAPI.PluginLoading
{
    public partial class MiraPluginInfo
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of this <see cref="IMiraPlugin"/>'s <see cref="BaseModifier"/>s. This is not safe for <see cref="BaseModifier"/>s with constructors.
        /// </summary>
        [Obsolete("Use BaseModifiers instead.")]
        [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Public API")]
        public IReadOnlyCollection<BaseModifier> Modifiers => [..BaseModifiers.OfType<BaseModifier>()];
    }
}
#pragma warning restore SA1403 // File may only contain a single namespace
