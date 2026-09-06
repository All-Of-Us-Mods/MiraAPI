using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Modifiers;

/// <summary>
/// Utilities to make handling <see cref="BaseModifier"/>s in-game easier.
/// </summary>
public static class ModifierUtils
{
    /// <summary>
    /// Gets a list of all active in-game <typeparamref name="T"/>s.
    /// </summary>
    /// <param name="predicate">Select if <typeparamref name="T"/> is valid to be added to list.</param>
    /// <typeparam name="T">The <see cref="BaseModifier"/> type.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s.</returns>
    public static IEnumerable<T> GetActiveModifiers<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return PlayerControl.AllPlayerControls.ToArray().SelectMany(x => x.GetModifiers(predicate));
    }

    /// <summary>
    /// Gets all <see cref="PlayerControl"/>s with a certain <typeparamref name="T"/>.
    /// </summary>
    /// <param name="predicate">Select if <typeparamref name="T"/> is valid.</param>
    /// <typeparam name="T">The <see cref="BaseModifier"/> type.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PlayerControl"/>s with that <typeparamref name="T"/>.</returns>
    public static IEnumerable<PlayerControl> GetPlayersWithModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier(predicate));
    }
}
