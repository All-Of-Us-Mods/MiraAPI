using MiraAPI.Example.Options.Roles;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace MiraAPI.Example.Buttons.Teleporter;
[RegisterButton]
public class TeleportButton : CustomActionButton
{
    public override string Name => "Teleport";

    public override float Cooldown => OptionGroupSingleton<TeleporterOptions>.Instance.TeleportCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<TeleporterOptions>.Instance.TeleportDuration;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => ExampleAssets.TeleportButton;
    public static bool IsZoom { get; private set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is TeleporterRole;
    }
    protected override void OnClick()
    {
        Coroutines.Start(ZoomOutCoroutine());
    }

    public override void OnEffectEnd()
    {
        Coroutines.Start(ZoomInCoroutine());
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (!EffectActive) return;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            playerControl.NetTransform.RpcSnapTo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            ResetCooldownAndOrEffect();
        }
    }

    private static IEnumerator ZoomOutCoroutine()
    {
        HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
        IsZoom = true;
        var zoomDistance = OptionGroupSingleton<TeleporterOptions>.Instance.ZoomDistance;
        for (var ft = Camera.main!.orthographicSize; ft < zoomDistance; ft += 0.3f)
        {
            Camera.main.orthographicSize = MeetingHud.Instance ? 3f : ft;
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main.orthographicSize;
            yield return null;
        }

        foreach (var cam in Camera.allCameras) cam.orthographicSize = zoomDistance;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }

    private static IEnumerator ZoomInCoroutine()
    {
        for (var ft = Camera.main!.orthographicSize; ft > 3f; ft -= 0.3f)
        {
            Camera.main.orthographicSize = MeetingHud.Instance ? 3f : ft;
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main.orthographicSize;

            yield return null;
        }

        foreach (var cam in Camera.allCameras) cam.orthographicSize = 3f;
        HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
        IsZoom = false;

        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }
}
