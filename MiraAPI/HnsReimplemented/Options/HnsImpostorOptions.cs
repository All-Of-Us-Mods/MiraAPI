using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Seeker settings on Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
public class HnsImpostorOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Impostors";

    /// <summary>
    /// Gets the player who will be the seeker.
    /// </summary>
    public ModdedPlayerOption SelectedSeeker { get; } = new(
        "Forced Impostor");

    /// <summary>
    /// Gets the range of a seeker's vision.
    /// </summary>
    public ModdedNumberOption ImpostorVision { get; } = new(
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
    /// Gets the range of a seeker's flashlight.
    /// </summary>
    public ModdedNumberOption ImpostorFlashlightSize { get; } = new(
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
