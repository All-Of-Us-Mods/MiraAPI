using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Gets or sets the seeker settings on Hide and Seek.
/// </summary>
public class HnsImpostorOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Impostors";

    /// <summary>
    /// Gets or sets the player who will be the seeker.
    /// </summary>
    public ModdedPlayerOption SelectedSeeker { get; set; } = new(
        "Forced Impostor");

    /// <summary>
    /// Gets or sets the range of a seeker's vision.
    /// </summary>
    public ModdedNumberOption ImpostorVision { get; set; } = new(
        "Impostor Vision",
        0.6f,
        0.25f,
        1,
        0.05f,
        "#",
        "#",
        MiraNumberSuffixes.Multiplier,
        "0.00");

    /// <summary>
    /// Gets or sets the range of a seeker's flashlight.
    /// </summary>
    public ModdedNumberOption ImpostorFlashlightSize { get; set; } = new(
        "Impostor Flashlight Size",
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
}
