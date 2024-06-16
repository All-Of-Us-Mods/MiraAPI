using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example
{
    [RegisterCustomRole(ExamplePlugin.Id)]
    public class CustomRole : CrewmateRole, ICustomRole
    {
        public string RoleName => "Test Role";
        public string RoleDescription => "Role decs?";
        public string RoleLongDescription => "Extra role decs?";
        public Color RoleColor => Palette.Orange;
        public ModdedRoleTeams Team => ModdedRoleTeams.Neutral;
    }
}
