using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <inheritdoc />
public class HnsTaskOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Tasks";

    public ModdedNumberOption CommonTasks { get; set; } = new(
        "# Common Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    public ModdedNumberOption LongTasks { get; set; } = new(
        "# Long Tasks",
        1,
        0,
        4,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

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
