using System;

namespace MiraAPI.Utilities;

/// <summary>
/// A utility class for using delegates.
/// </summary>
public static class DelegateUtilities
{
    /// <summary>
    /// Gets a <c>Func&lt;bool&gt;</c> that always returns <c>true</c>.
    /// </summary>
    public static Func<bool> AlwaysTrue { get; } = () => true;

    /// <summary>
    /// Gets a <c>Func&lt;bool&gt;</c> that always returns <c>false</c>.
    /// </summary>
    public static Func<bool> AlwaysFalse { get; } = () => false;
}
