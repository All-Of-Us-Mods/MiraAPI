using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Gets or sets the task options for Hide and Seek.
/// </summary>
public class HnsTaskOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Tasks";

    /// <summary>
    /// Gets or sets the umber of common tasks.
    /// </summary>
    public ModdedNumberOption CommonTasks { get; set; } = new(
        "# Common Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    /// <summary>
    /// Gets or sets the number of long tasks.
    /// </summary>
    public ModdedNumberOption LongTasks { get; set; } = new(
        "# Long Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    /// <summary>
    /// Gets or sets the number of short tasks.
    /// </summary>
    public ModdedNumberOption ShortTasks { get; set; } = new(
        "# Short Tasks",
        2,
        0,
        8,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);
}
