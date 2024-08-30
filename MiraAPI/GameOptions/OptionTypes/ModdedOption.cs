using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using System;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes;

public abstract class ModdedOption<T> : IModdedOption
{
    private IMiraPlugin? _parentMod;

    /// <summary>
    /// Gets the unique identifier of the option.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the BaseGameSetting data of the option.
    /// </summary>
    public BaseGameSetting Data { get; protected init; }

    /// <summary>
    /// Gets or sets the parent mod of the option.
    /// </summary>
    public IMiraPlugin? ParentMod
    {
        get => _parentMod;
        set
        {
            if (_parentMod != null || value == null) return;
            _parentMod = value;
            var entry = _parentMod.GetConfigFile().Bind(ConfigDefinition, DefaultValue);
            Value = entry.Value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the option.
    /// </summary>
    public T Value { get; protected set; }

    /// <summary>
    /// Gets the default value of the option.
    /// </summary>
    public T DefaultValue { get; }

    /// <summary>
    /// Gets or sets the event that is invoked when the value of the option changes.
    /// </summary>
    public Action<T> ChangedEvent { get; set; }

    /// <summary>
    /// Gets the title of the option.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the StringName object of the option.
    /// </summary>
    public StringNames StringName { get; }

    /// <summary>
    /// Gets or sets the visibility of the option.
    /// </summary>
    public Func<bool> Visible { get; set; }

    public Type AdvancedRole { get; set; }

    public OptionBehaviour OptionBehaviour { get; set; }

    /// <summary>
    /// Gets or sets the config definition of the option.
    /// </summary>
    public ConfigDefinition? ConfigDefinition
    {
        get => _configDefinition;
        set
        {
            if (_configDefinition is not null) return;
            _configDefinition = value;
        }
    }

    private ConfigDefinition? _configDefinition;

    protected ModdedOption(string title, T defaultValue, Type? roleType)
    {
        Id = ModdedOptionsManager.NextId;
        Title = title;
        DefaultValue = defaultValue;
        Value = defaultValue;
        StringName = CustomStringName.CreateAndRegister(Title);
        Visible = () => true;

        if (roleType is not null && roleType.IsAssignableTo(typeof(ICustomRole)))
        {
            AdvancedRole = roleType;
        }
    }

    public void ValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(GetValueFromOptionBehaviour(optionBehaviour));
    }

    public void SetValue(T newValue)
    {
        var oldVal = Value;
        Value = newValue;
        if (!Value.Equals(oldVal) && ChangedEvent != null)
        {
            ChangedEvent.Invoke(Value);
        }

        if (AmongUsClient.Instance.AmHost)
        {
            if (ParentMod?.GetConfigFile().TryGetEntry<T>(ConfigDefinition, out var entry) == true)
            {
                entry.Value = Value;
            }
            
            Rpc<SyncOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [GetNetData()], true);
        }

        OnValueChanged(newValue);
    }

    public abstract float GetFloatData();
    
    public abstract NetData GetNetData();

    public abstract void HandleNetData(byte[] data);

    public abstract void OnValueChanged(T newValue);

    public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

    public abstract OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);

    public abstract string GetHudStringText();
}