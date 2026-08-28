using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <inheritdoc />
public class HnsFinalHideOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Final Hide";

    public ModdedNumberOption FinalHideTime { get; set; } = new(
        "Hiding Time",
        50,
        30,
        120,
        5,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

    public ModdedToggleOption FinalHidePings { get; set; } = new(
        "Final Hide Pings",
        true);

    public ModdedNumberOption FinalHideImpostorSpeed { get; set; } = new(
        "Final Hide Impostor Speed",
        1.2f,
        1f,
        3,
        0.05f,
        "#",
        "#",
        MiraNumberSuffixes.Multiplier,
        "0.00");

    public ModdedNumberOption PingInterval { get; set; } = new(
        "Ping Interval",
        6,
        3,
        10,
        5,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

    public ModdedToggleOption FinalHideSeekMap { get; set; } = new(
        "Final Hide Seek Map",
        true);
}
