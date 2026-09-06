using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Gets or sets the Final Hide options for Hide and Seek.
/// </summary>
public class HnsFinalHideOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Final Hide";

    /// <summary>
    /// Gets or sets the duration of the Final Hide phase.
    /// </summary>
    public ModdedNumberOption FinalHideTime { get; set; } = new(
        "Hiding Time",
        50,
        30,
        120,
        5,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

    /// <summary>
    /// Gets or sets the Seeker's speed when the Final Hide phase begins.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether the Seeker sees the Final hide player pings.
    /// </summary>
    public ModdedToggleOption FinalHidePings { get; set; } = new(
        "Final Hide Pings",
        true);

    /// <summary>
    /// Gets or sets the interval between each ping.
    /// </summary>
    public ModdedNumberOption PingInterval { get; set; } = new(
        "Ping Interval",
        6,
        3,
        10,
        5,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

    /// <summary>
    /// Gets or sets whether the Seeker can see players on an admin map.
    /// </summary>
    public ModdedToggleOption FinalHideSeekMap { get; set; } = new(
        "Final Hide Seek Map",
        true);
}
