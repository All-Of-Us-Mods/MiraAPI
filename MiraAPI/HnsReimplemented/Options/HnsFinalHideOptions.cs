using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Final Hide options for Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
[SearchPrivate]
public class HnsFinalHideOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "MiraApi.Option.Group.HnsFinalHide";

    /// <inheritdoc />
    public override string GroupPrefix => "MiraApi.HnsFinalHide.Option.";

    /// <summary>
    /// Gets the duration of the Final Hide phase.
    /// </summary>
    [ModdedNumberOption(30f, 120f, 5f, "#", "#", MiraNumberSuffixes.Seconds)]
    public float FinalHideTime { get; } = 50f;

    /// <summary>
    /// Gets the Seeker's speed when the Final Hide phase begins.
    /// </summary>
    [ModdedNumberOption(1f, 3f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00")]
    public float FinalHideImpostorSpeed { get; } = 1.2f;

    /// <summary>
    /// Gets a value indicating whether the Seeker sees the Final hide player pings.
    /// </summary>
    [ModdedToggleOption]
    public bool FinalHidePings { get; } = true;

    /// <summary>
    /// Gets the interval between each ping.
    /// </summary>
    [ModdedNumberOption(3f, 10f, 1f, "#", "#", MiraNumberSuffixes.Seconds)]
    [ModdedOptionVisibility(nameof(FinalHidePings))]
    public float PingInterval { get; } = 6f;

    /// <summary>
    /// Gets a value indicating whether the Seeker can see players on an admin map.
    /// </summary>
    [ModdedToggleOption]
    public bool FinalHideSeekMap { get; } = true;
}
