using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MiraAPI.Hud;

/// <summary>
/// Custom Player Menu using the <see cref="ShapeshifterPanel"/> as a base.
/// </summary>
/// <param name="il2CppPtr">Used by Il2Cpp. Do not use constructor, this is a <see cref="MonoBehaviour"/>.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity Convention.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity Convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Unity Convention.")]
public class CustomPlayerMenu(IntPtr il2CppPtr) : Minigame(il2CppPtr)
{
    public ShapeshifterPanel panelPrefab;
    public float xStart = -0.8f;
    public float yStart = 2.15f;
    public float xOffset = 1.95f;
    public float yOffset = -0.65f;
    public UiElement backButton;
    public UiElement defaultButtonSelected;
    public List<ShapeshifterPanel> potentialVictims;

    /// <summary>
    /// Creates a <see cref="CustomPlayerMenu"/>.
    /// </summary>
    /// <returns>New <see cref="CustomPlayerMenu"/> object.</returns>
    public static CustomPlayerMenu Create()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>()!.ShapeshifterMenu;
        var newMenu = Instantiate(ogMenu);
        var customMenu = newMenu.gameObject.AddComponent<CustomPlayerMenu>();

        customMenu.panelPrefab = newMenu.PanelPrefab;
        customMenu.xStart = newMenu.XStart;
        customMenu.yStart = newMenu.YStart;
        customMenu.xOffset = newMenu.XOffset;
        customMenu.yOffset = newMenu.YOffset;
        customMenu.backButton = newMenu.BackButton;
        var back = customMenu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)(() =>
        {
            Instance.Close();
        }));

        customMenu.CloseSound = newMenu.CloseSound;
        customMenu.logger = newMenu.logger;
        customMenu.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main!.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, -50f);
        return customMenu;
    }

    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
    private void OnDisable()
    {
        ControllerManager.Instance.CloseOverlayMenu(name);
    }

    /// <inheritdoc />
    public override void Begin(PlayerTask task)
    {
        throw new NotImplementedException("Use the other Begin method.");
    }

    /// <summary>
    /// Begins/opens the custom player menu.
    /// </summary>
    /// <param name="playerMatch">Function to determine if player should show in the custom menu.</param>
    /// <param name="onClick"><see cref="PassiveButton.OnClick"/> action for player.</param>
    [HideFromIl2Cpp]
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl?> onClick)
    {
        MinigameStubs.Begin(this, null);

        var back = backButton.GetComponent<PassiveButton>();
        back.OnClick.AddListener((UnityAction)(() =>
        {
            onClick(null);
        }));

        DebugAnalytics.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, TaskType);
        var list = PlayerControl.AllPlayerControls.ToArray().Where(playerMatch).ToList();
        potentialVictims = [];
        var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

        for (var i = 0; i < list.Count; i++)
        {
            var player = list[i];
            var num = i % 3;
            var num2 = i / 3;
            var flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;
            var shapeshifterPanel = Instantiate(panelPrefab, transform);
            shapeshifterPanel.transform.localPosition = new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
            shapeshifterPanel.SetPlayer(i, player.Data, (Il2CppSystem.Action)(() => { onClick(player); }));
            shapeshifterPanel.NameText.color = flag ? player.Data.Role.NameColor : Color.white;
            potentialVictims.Add(shapeshifterPanel);
            list2.Add(shapeshifterPanel.Button);
        }
        ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list2);
    }
}
