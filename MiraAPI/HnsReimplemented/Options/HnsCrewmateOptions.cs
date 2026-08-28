using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.HnsReimplemented.Options;

/// <inheritdoc />
public class HnsCrewmateOptions : AbstractOptionGroup<HideAndSeekMode>
{
    /// <inheritdoc />
    public override string GroupName => "Crewmates";

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

    public ModdedNumberOption HidingTime { get; set; } = new(
        "Hiding Time",
        200,
        160,
        300,
        20,
        "#",
        "#",
        MiraNumberSuffixes.Seconds);

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

    public ModdedNumberOption MaxVentUses { get; set; } = new(
        "Max Vent Uses",
        1,
        0,
        5,
        1,
        "#",
        "#",
        MiraNumberSuffixes.None);

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

    public ModdedToggleOption FlashlightMode { get; set; } = new(
        "Flashlight Mode",
        true);

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

    public ModdedToggleOption ShowNames { get; set; } = new(
        "Show Names",
        false);
}
