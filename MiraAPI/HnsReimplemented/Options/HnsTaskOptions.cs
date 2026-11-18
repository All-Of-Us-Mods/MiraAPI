using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Task options for Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
[SearchPrivate]
public class HnsTaskOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "MiraApi.Option.Group.HnsTask";

    /// <inheritdoc />
    public override string GroupPrefix => "MiraApi.HnsTask.Option.";

    /// <summary>
    /// Gets the number of common tasks.
    /// </summary>
    [ModdedNumberOption(0, 4, 1, "#", "#", MiraNumberSuffixes.None)]
    public int CommonTasks { get; } = 1;

    /// <summary>
    /// Gets the number of long tasks.
    /// </summary>
    [ModdedNumberOption(0, 4, 1, "#", "#", MiraNumberSuffixes.None)]
    public int LongTasks { get; } = 1;

    /// <summary>
    /// Gets the number of short tasks.
    /// </summary>
    [ModdedNumberOption(0, 8, 1, "#", "#", MiraNumberSuffixes.None)]
    public int ShortTasks { get; } = 2;
}
