using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Hud;

/// <summary>
/// Custom Player Menu using the ShapeshifterPanel as a base.
/// </summary>
/// <param name="il2CppPtr">Used by Il2Cpp. Do not use constructor, this is a MonoBehaviour.</param>
[RegisterInIl2Cpp]
public class CustomPlayerMenu(IntPtr il2CppPtr) : Minigame(il2CppPtr)
{
    public ShapeshifterPanel PanelPrefab;

    public float XStart = -0.8f;

    public float YStart = 2.15f;

    public float XOffset = 1.95f;

    public float YOffset = -0.65f;

    public UiElement BackButton;

    public UiElement DefaultButtonSelected;

    public List<ShapeshifterPanel> potentialVictims;

    /// <summary>
    /// Creates a CustomPlayerMenu.
    /// </summary>
    /// <returns>New CustomPlayerMenu object.</returns>
    public static CustomPlayerMenu Create()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>().ShapeshifterMenu;
        var newMenu = Instantiate(ogMenu);
        var customMenu = newMenu.gameObject.AddComponent<CustomPlayerMenu>();

        customMenu.PanelPrefab = newMenu.PanelPrefab;
        customMenu.XStart = newMenu.XStart;
        customMenu.YStart = newMenu.YStart;
        customMenu.XOffset = newMenu.XOffset;
        customMenu.YOffset = newMenu.YOffset;
        customMenu.BackButton = newMenu.BackButton;
        var back = customMenu.BackButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)(() =>
        {
            Instance.Close();
        }));

        customMenu.CloseSound = newMenu.CloseSound;
        customMenu.logger = newMenu.logger;
        customMenu.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, -50f);
        return customMenu;
    }

    private void OnDisable()
    {
        ControllerManager.Instance.CloseOverlayMenu(name);
    }

    public override void Begin(PlayerTask task)
    {
        throw new NotImplementedException("Use the other Begin method.");
    }

    /// <summary>
    /// Begins/opens the custom player menu.
    /// </summary>
    /// <param name="playerMatch">Function to determine if player should show in the custom menu.</param>
    /// <param name="onClick">Onclick action for player.</param>
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl> onClick)
    {
        MinigameStubs.Begin(this, null);

        DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, TaskType);
        var list = PlayerControl.AllPlayerControls.ToArray().Where(playerMatch).ToList();
        potentialVictims = [];
        var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

        for (var i = 0; i < list.Count; i++)
        {
            var player = list[i];
            var num = i % 3;
            var num2 = i / 3;
            var flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;
            var shapeshifterPanel = Instantiate(PanelPrefab, transform);
            shapeshifterPanel.transform.localPosition = new Vector3(XStart + num * XOffset, YStart + num2 * YOffset, -1f);
            shapeshifterPanel.SetPlayer(i, player.Data, (Il2CppSystem.Action)(() => { onClick(player); }));
            shapeshifterPanel.NameText.color = flag ? player.Data.Role.NameColor : Color.white;
            potentialVictims.Add(shapeshifterPanel);
            list2.Add(shapeshifterPanel.Button);
        }
        ControllerManager.Instance.OpenOverlayMenu(name, BackButton, DefaultButtonSelected, list2);
    }
}
