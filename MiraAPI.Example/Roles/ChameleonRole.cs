using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class ChameloenRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Chamelon";
    public string RoleLongDescription => "Stay invisible while not moving.";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.AcceptedGreen;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new CustomRoleConfiguration(this)
    {
        OptionsScreenshot = ExampleAssets.Banner,
    };

    public void PlayerControlFixedUpdate(PlayerControl playerControl)
    {
        if (playerControl.MyPhysics.Velocity.magnitude > 0)
        {
            var rend = playerControl.cosmetics.currentBodySprite.BodySprite;
            var tmp = playerControl.cosmetics.nameText;
            tmp.color = Color.Lerp(tmp.color, new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1), Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);

            foreach (var cosmetic in playerControl.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(cosmetic.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);
            }
        }
        else
        {
            var rend = playerControl.cosmetics.currentBodySprite.BodySprite;
            var tmp = playerControl.cosmetics.nameText;
            tmp.color = Color.Lerp(tmp.color, new Color(tmp.color.r, tmp.color.g, tmp.color.b, playerControl.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, playerControl.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);

            foreach (var cosmetic in playerControl.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(cosmetic.color, new Color(1, 1, 1, playerControl.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);
            }
        }
    }
}
