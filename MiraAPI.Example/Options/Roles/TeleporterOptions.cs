using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.Example.Options.Roles;

public class TeleporterOptions : AbstractRoleOptionGroup<TeleporterRole>
{
    public override string GroupName => "ApiExample.Role.Teleporter";

    public ModdedNumberOption TeleportCooldown { get; set; } = new("ApiExample.Role.Teleporter.Settings.TeleportCooldown", 10, 5, 60, 2.5f, MiraNumberSuffixes.Seconds);

    [ModdedNumberOption("ApiExample.Role.Teleporter.Settings.TeleportDuration", 5, 25, 1, MiraNumberSuffixes.Seconds)]
    public float TeleportDuration { get; set; } = 10;

    [ModdedNumberOption("ApiExample.Role.Teleporter.Settings.ZoomDistance", 4, 15)]
    public float ZoomDistance { get; set; } = 6;
}
