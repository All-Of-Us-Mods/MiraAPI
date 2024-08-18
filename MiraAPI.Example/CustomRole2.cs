using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example
{
    [RegisterCustomRole]
    public class CustomRole2 : CrewmateRole, ICustomRole
    {
        public string RoleName => "fortnite defender";
        public string RoleLongDescription => "ok so your objective is to save everyone from the fortnite killer";
        public string RoleDescription => RoleLongDescription;
        public Color RoleColor => Palette.AcceptedGreen;
        public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
        public LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Banner;

        [ModdedToggleOption("Teleportation ability", roleType: typeof(CustomRole2))] public bool teleport { get; set; } = true;

    }
}
