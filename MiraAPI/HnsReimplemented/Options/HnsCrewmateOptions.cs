using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Crewmate options for Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
[SearchPrivate]
public class HnsCrewmateOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "MiraApi.Option.Group.HnsCrewmate";

    /// <inheritdoc />
    public override string GroupPrefix => "MiraApi.HnsCrewmate.Option.";

    /// <summary>
    /// Gets the player speed.
    /// </summary>
    [ModdedNumberOption(0.5f, 3f, 0.25f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00")]
    public float PlayerSpeed { get; } = 1f;

    /// <summary>
    /// Gets the hiding time.
    /// </summary>
    [ModdedNumberOption(160, 300, 20, "#", "#", MiraNumberSuffixes.Seconds)]
    public float HidingTime { get; } = 200f;

    /// <summary>
    /// Gets the limits of a crewmate's vision.
    /// </summary>
    [ModdedNumberOption(0.25f, 1, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00")]
    public float CrewmateVision { get; } = 0.6f;

    /// <summary>
    /// Gets a value indicating whether flashlight vision is turned on or not.
    /// </summary>
    [ModdedToggleOption]
    public bool FlashlightMode { get; } = true;

    /// <summary>
    /// Gets the range of a crewmate's flashlight.
    /// </summary>
    [ModdedNumberOption(0.1f, 0.5f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier)]
    [ModdedOptionVisibility(nameof(FlashlightMode))]
    public float CrewmateFlashlightSize { get; } = 0.35f;

    /// <summary>
    /// Gets the maximum number of times a crewmate can vent.
    /// </summary>
    [ModdedNumberOption(0, 5, 1, "#", "#", MiraNumberSuffixes.None)]
    public int MaxVentUses { get; } = 1;

    /// <summary>
    /// Gets the maximum amount of time a crewmate can spend inside a vent.
    /// </summary>
    [ModdedNumberOption(1, 10, 1, "#", "#", MiraNumberSuffixes.Seconds)]
    [ModdedOptionVisibility(nameof(LimitedVentUses))]
    public float MaxTimeInVent { get; } = 3f;

    /// <summary>
    /// Gets a value indicating whether player names are visible.
    /// </summary>
    [ModdedToggleOption]
    public bool ShowNames { get; } = false;

    private bool LimitedVentUses()
    {
        return MaxVentUses > 0;
    }
}
