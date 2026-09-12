using AmongUs.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class ChameleonRole : CrewmateRole, ICustomRole
{
    public string IdPart => "Chameleon";
    public string IdPrefix => "ApiExample.Role.Crewmate";
    public Color RoleColor => Palette.AcceptedGreen;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    private bool _shouldHide;

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = ExampleAssets.Banner,
        Icon = MiraAssets.CrewmateFile,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            MiraAssets.CrewmateFile.LoadAsset(),
            "ApiExample.Role.Impostor.ChameleonRole"),
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter),
    };

    public override void Initialize(PlayerControl player)
    {
        Logger<ExamplePlugin>.Info("Initializing ChameleonRole for player: " + player.PlayerId);
        RoleBehaviourStubs.Initialize(this, player);
        _shouldHide = true;
    }

    public void FixedUpdate()
    {
        if (!Player || !_shouldHide)
        {
            return;
        }

        if (Player.MyPhysics.Velocity.magnitude > 0)
        {
            var rend = Player.cosmetics.currentBodySprite.BodySprite;
            var tmp = Player.cosmetics.nameText;
            tmp.color = Color.Lerp(tmp.color, new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1), Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);

            foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(cosmetic.color, new Color(1, 1, 1, 1), Time.deltaTime * 4f);
            }
        }
        else
        {
            var rend = Player.cosmetics.currentBodySprite.BodySprite;
            var tmp = Player.cosmetics.nameText;
            tmp.color = Color.Lerp(
                tmp.color,
                new Color(tmp.color.r, tmp.color.g, tmp.color.b, Player.AmOwner ? 0.3f : 0),
                Time.deltaTime * 4f);
            rend.color = Color.Lerp(rend.color, new Color(1, 1, 1, Player.AmOwner ? 0.3f : 0), Time.deltaTime * 4f);

            foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                cosmetic.color = Color.Lerp(
                    cosmetic.color,
                    new Color(1, 1, 1, Player.AmOwner ? 0.3f : 0),
                    Time.deltaTime * 4f);
            }
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        Logger<ExamplePlugin>.Info("Deinitializing ChameleonRole for player: " + targetPlayer.PlayerId);
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        _shouldHide = false;
        foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>(true))
        {
            cosmetic.color = Color.white;
        }

        Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
        Player.cosmetics.nameText.color = Color.white;
    }
}
