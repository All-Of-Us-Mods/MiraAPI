using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace MiraAPI.Modifiers;

[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public List<BaseModifier> ActiveModifiers { get; private set; }

    public PlayerControl player;

    private TextMeshPro _modifierText;

    public void Start()
    {
        player = GetComponent<PlayerControl>();
        ActiveModifiers = [];

        if (!player.AmOwner)
        {
            return;
        }

        _modifierText = Helpers.CreateTextLabel("ModifierText", HudManager.Instance.transform, AspectPosition.EdgeAlignments.RightTop, new Vector3(10.1f, 3.5f, 0), textAlignment: TextAlignmentOptions.Right);
        _modifierText.verticalAlignment = VerticalAlignmentOptions.Top;
    }

    public void FixedUpdate()
    {
        foreach (var modifier in ActiveModifiers)
        {
            modifier.FixedUpdate();
        }
    }

    public void Update()
    {
        foreach (var modifier in ActiveModifiers)
        {
            modifier.Update();
        }

        var filteredModifiers = ActiveModifiers.Where(mod => !mod.HideOnUi);

        var baseModifiers = filteredModifiers as BaseModifier[] ?? filteredModifiers.ToArray();
        
        if (player.AmOwner && baseModifiers.Any())
        {
            var stringBuild = new StringBuilder();
            foreach (var mod in baseModifiers)
            {
                stringBuild.Append($"\n{mod.ModifierName}");
                if (mod is TimedModifier timer)
                {
                    stringBuild.Append($" <size=70%>({Math.Round(timer.Duration - timer.TimeRemaining, 0)}s/{timer.Duration}s)</size>");
                }
            }
            _modifierText.text = $"<b><size=130%>Modifiers:</b></size>{stringBuild}";
        }
        else if (player.AmOwner && !baseModifiers.Any() && _modifierText.text != string.Empty)
        {
            _modifierText.text = string.Empty;
        }
    }

    public static void RemoveModifier(PlayerControl target, uint modifierId)
    {
        if (!ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with id {modifierId} because it is not registered.");
            return;
        }

        var modifierComponent = target.GetModifierComponent();

        var modifier = modifierComponent.ActiveModifiers.Find(x => x.GetType() == type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }

        modifier.OnDeactivate();
        modifierComponent.ActiveModifiers.Remove(modifier);

        if (target.AmOwner)
        {
            HudManager.Instance.SetHudActive(true);
        }
    }

    public static void AddModifier(PlayerControl target, uint modifierId)
    {
        if (!ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
            return;
        }

        if (target.HasModifier(modifierId))
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {modifierId}!");
            return;
        }

        var modifier = (BaseModifier)Activator.CreateInstance(type);

        var modifierComponent = target.GetModifierComponent();

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is null.");
            return;
        }

        modifierComponent.ActiveModifiers.Add(modifier);
        modifier.Player = modifierComponent.player;
        modifier.ModifierId = modifierId;
        modifier.OnActivate();

        if (target.AmOwner)
        {
            if (modifier is TimedModifier { AutoStart: true } timer)
            {
                timer.StartTimer();
            }

            HudManager.Instance.SetHudActive(true);
        }
    }

    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(PlayerControl target, uint modifierId) => RemoveModifier(target, modifierId);

    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static void RpcAddModifier(PlayerControl target, uint modifierId) => AddModifier(target, modifierId);
}