using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Gets or sets the crewmate options for Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
public class HnsCrewmateOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Crewmates";

    /// <summary>
    /// Gets or sets the player speed.
    /// </summary>
    public ModdedNumberOption PlayerSpeed { get; set; } = new(
        "Player Speed",
        1f,
        0.5f,
        3f,
        0.25f,
        "#",
        "#",
        MiraNumberSuffixes.Multiplier,
        "0.00");

    /// <summary>
    /// Gets or sets the hiding time.
    /// </summary>
    public ModdedNumberOption HidingTime { get; set; } = new(
        "Hiding Time",
        200,
        160,
        300,
        20,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

    /// <summary>
    /// Gets or sets the limits of a crewmate's vision.
    /// </summary>
    public ModdedNumberOption CrewmateVision { get; set; } = new(
        "Crewmate Vision",
        0.6f,
        0.25f,
        1,
        0.05f,
        "#",
        "#",
        MiraNumberSuffixes.Multiplier,
        "0.00");

    /// <summary>
    /// Gets or sets a flag that toggles flashlight vision.
    /// </summary>
    public ModdedToggleOption FlashlightMode { get; set; } = new(
        "Flashlight Mode",
        true);

    /// <summary>
    /// Gets or sets the range of a crewmate's flashlight.
    /// </summary>
    public ModdedNumberOption CrewmateFlashlightSize { get; set; } = new(
        "Crewmate Flashlight Size",
        0.35f,
        0.1f,
        0.5f,
        0.05f,
        "#",
        "#",
        MiraNumberSuffixes.Multiplier)
    {
        Visible = () => OptionGroupSingleton<HnsCrewmateOptions>.Instance.FlashlightMode.Value,
    };

    /// <summary>
    /// Gets or sets the maximum number of times a crewmate can vent.
    /// </summary>
    public ModdedNumberOption MaxVentUses { get; set; } = new(
        "Max Vent Uses",
        1,
        0,
        5,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

    /// <summary>
    /// Gets or sets the maximum amount of time a crewmate can spend inside a vent.
    /// </summary>
    public ModdedNumberOption MaxTimeInVent { get; set; } = new(
        "Max Time in Vent",
        3,
        1,
        10,
        1,
        "#",
        "#",
        MiraNumberSuffixes.Seconds)
    {
        Visible = () => (int)OptionGroupSingleton<HnsCrewmateOptions>.Instance.MaxVentUses.Value > 0,
    };

    /// <summary>
    /// Gets or sets the visibility of player names.
    /// </summary>
    public ModdedToggleOption ShowNames { get; set; } = new(
        "Show Names",
        false);
}
