using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <summary>
/// Seeker settings on Hide and Seek.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
[SearchPrivate]
public class HnsImpostorOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc/>
    public override string GroupName => "MiraApi.Option.Group.HnsImpostor";

    /// <inheritdoc/>
    public override string GroupPrefix => "MiraApi.HnsImpostor.Option.";

    /// <summary>
    /// Gets the player who will be the seeker.
    /// </summary>
    [ModdedPlayerOption]
    public int SelectedSeeker { get; } = 0;

    /// <summary>
    /// Gets the range of a seeker's vision.
    /// </summary>
    [ModdedNumberOption(0.25f, 1f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00")]
    public float ImpostorVision { get; } = 0.6f;

    /// <summary>
    /// Gets the range of a seeker's flashlight.
    /// </summary>
    [ModdedNumberOption(0.1f, 0.5f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier)]
    [ModdedOptionVisibility(nameof(IsFlashlightModeEnabled))]
    public float ImpostorFlashlightSize { get; } = 0.35f;

    private static bool IsFlashlightModeEnabled()
    {
        return OptionGroupSingleton<HnsCrewmateOptions>.Instance.FlashlightMode;
    }
}
