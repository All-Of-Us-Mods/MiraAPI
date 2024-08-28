using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
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

    public void RemoveModifier(Type type)
    {
        var modifier = ActiveModifiers.Find(x => x.GetType() == type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }
        
        RemoveModifier(modifier);
    }

    public void RemoveModifier<T>() where T : BaseModifier
    {
        RemoveModifier(typeof(T));
    }

    public void RemoveModifier(uint modifierId)
    {
        var modifier = ActiveModifiers.Find(x => x.ModifierId == modifierId);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with id {modifierId} because it is not active.");
            return;
        }
        
        RemoveModifier(modifier);
    }

    public void RemoveModifier(BaseModifier modifier)
    {
        if (!ActiveModifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {modifier.ModifierName} because it is not active on this player.");
            return;
        }
        
        modifier.OnDeactivate();
        ActiveModifiers.Remove(modifier);

        if (player.AmOwner)
        {
            HudManager.Instance.SetHudActive(true);
        }
    }

    public BaseModifier AddModifier(BaseModifier modifier)
    {
        if (ActiveModifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {modifier.ModifierId}!");
            return null;
        }
        
        ActiveModifiers.Add(modifier);
        modifier.Player = player;
        modifier.ModifierId = ModifierManager.TypeToIdModifiers[modifier.GetType()];
        modifier.OnActivate();

        if (!player.AmOwner)
        {
            return modifier;
        }

        if (modifier is TimedModifier { AutoStart: true } timer)
        {
            timer.StartTimer();
        }

        HudManager.Instance.SetHudActive(true);

        return modifier;
    }

    public BaseModifier AddModifier(Type type)
    {
        if (!ModifierManager.TypeToIdModifiers.TryGetValue(type, out var modifierId))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is not registered.");
            return null;
        }

        if (ActiveModifiers.Find(x=>x.ModifierId == modifierId) != null)
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {modifierId}!");
            return null;
        }

        var modifier = (BaseModifier)Activator.CreateInstance(type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is null.");
            return null;
        }

        AddModifier(modifier);
        
        return modifier;
    }

    public T AddModifier<T>() where T : BaseModifier
    {
        return AddModifier(typeof(T)) as T;
    }
}