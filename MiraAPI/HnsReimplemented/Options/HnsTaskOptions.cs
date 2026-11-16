using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Task options for Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
public class HnsTaskOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Tasks";

    /// <summary>
    /// Gets the umber of common tasks.
    /// </summary>
    public ModdedNumberOption CommonTasks { get; } = new(
        "# Common Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    /// <summary>
    /// Gets the number of long tasks.
    /// </summary>
    public ModdedNumberOption LongTasks { get; } = new(
        "# Long Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    /// <summary>
    /// Gets the number of short tasks.
    /// </summary>
    public ModdedNumberOption ShortTasks { get; } = new(
        "# Short Tasks",
        2,
        0,
        8,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);
}
