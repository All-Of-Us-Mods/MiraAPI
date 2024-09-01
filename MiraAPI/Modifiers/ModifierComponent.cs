using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// The component for handling modifiers.
/// </summary>
[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    /// <summary>
    /// Gets the active modifiers on the player.
    /// </summary>
    [HideFromIl2Cpp]
    public ImmutableList<BaseModifier> ActiveModifiers => Modifiers.ToImmutableList();

    [HideFromIl2Cpp]
    private List<BaseModifier> Modifiers { get; set; } = [];

    private PlayerControl? _player;

    private TextMeshPro? _modifierText;

    private readonly List<BaseModifier> _toRemove = [];

    private readonly List<BaseModifier> _toAdd = [];

    internal void ClearModifiers()
    {
        foreach (var modifier in Modifiers)
        {
            modifier.OnDeactivate();
        }

        _toRemove.AddRange(Modifiers);
    }

    private void Start()
    {
        _player = GetComponent<PlayerControl>();
        Modifiers = [];

        if (!_player.AmOwner)
        {
            return;
        }

        _modifierText = Helpers.CreateTextLabel("ModifierText", HudManager.Instance.transform, AspectPosition.EdgeAlignments.RightTop, new Vector3(10.1f, 3.5f, 0), textAlignment: TextAlignmentOptions.Right);
        _modifierText.verticalAlignment = VerticalAlignmentOptions.Top;
    }

    private void FixedUpdate()
    {
        foreach (var modifier in _toRemove)
        {
            modifier.OnDeactivate();
            Modifiers.Remove(modifier);
        }

        foreach (var modifier in _toAdd)
        {
            Modifiers.Add(modifier);
            modifier.OnActivate();

            if (_player?.AmOwner == true && modifier is TimedModifier { AutoStart: true } timer)
            {
                timer.StartTimer();
            }
        }

        if (_toAdd.Count > 0 || _toRemove.Count > 0)
        {
            if (_player?.AmOwner == true)
            {
                HudManager.Instance?.SetHudActive(true);
            }
            _toAdd.Clear();
            _toRemove.Clear();
        }

        foreach (var modifier in Modifiers)
        {
            modifier.FixedUpdate();
        }
    }

    private void Update()
    {
        foreach (var modifier in Modifiers)
        {
            modifier.Update();
        }

        if (!_modifierText || _player?.AmOwner == false)
        {
            return;
        }

        var filteredModifiers = Modifiers.Where(mod => !mod.HideOnUi);

        var baseModifiers = filteredModifiers as BaseModifier[] ?? filteredModifiers.ToArray();

        if (baseModifiers.Length != 0)
        {
            var stringBuild = new StringBuilder();
            foreach (var mod in baseModifiers)
            {
                stringBuild.Append(CultureInfo.InvariantCulture, $"\n{mod.GetHudString()}");
            }
            _modifierText!.text = $"<b><size=130%>Modifiers:</b></size>{stringBuild}";
        }
        else if (_modifierText!.text != string.Empty)
        {
            _modifierText.text = string.Empty;
        }
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(Type type)
    {
        var modifier = Modifiers.Find(x => x.GetType() == type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <typeparam name="T">The modifier type.</typeparam>
    [HideFromIl2Cpp]
    public void RemoveModifier<T>() where T : BaseModifier
    {
        RemoveModifier(typeof(T));
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="modifierId">The modifier ID.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(uint modifierId)
    {
        var modifier = Modifiers.Find(x => x.ModifierId == modifierId);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with id {modifierId} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a modifier from the player.
    /// </summary>
    /// <param name="modifier">The modifier object.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(BaseModifier modifier)
    {
        if (!Modifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {modifier.ModifierName} because it is not active on this player.");
            return;
        }

        _toRemove.Add(modifier);
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The modifier that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(BaseModifier modifier)
    {
        if (Modifiers.Contains(modifier))
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {modifier.ModifierId}!");
            return null;
        }

        var modifierId = ModifierManager.GetModifierId(modifier.GetType());

        if (modifierId == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {modifier.GetType().Name} because it has no ID!");
            return null;
        }

        _toAdd.Add(modifier);
        modifier.Player = _player;
        modifier.ModifierId = modifierId.Value;
        return modifier;
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <returns>The modifier that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(Type type)
    {
        var modifierId = ModifierManager.GetModifierId(type);
        if (modifierId == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is not registered.");
            return null;
        }

        if (Modifiers.Find(x => x.ModifierId == modifierId) != null)
        {
            Logger<MiraApiPlugin>.Error($"Player already has modifier with id {modifierId}!");
            return null;
        }

        if (Activator.CreateInstance(type) is not BaseModifier modifier)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is null.");
            return null;
        }

        AddModifier(modifier);

        return modifier;
    }

    /// <summary>
    /// Adds a modifier to the player.
    /// </summary>
    /// <typeparam name="T">The Type of the modifier.</typeparam>
    /// <returns>The new modifier.</returns>
    [HideFromIl2Cpp]
    public T? AddModifier<T>() where T : BaseModifier
    {
        return AddModifier(typeof(T)) as T;
    }
}
