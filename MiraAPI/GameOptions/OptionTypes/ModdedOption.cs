using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// Represents a modded option.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public abstract class ModdedOption<T> : IModdedOption
{
    private IMiraPlugin? _parentMod;

    /// <summary>
    /// Gets the unique identifier of the option.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the title of the option.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the StringName object of the option.
    /// </summary>
    public StringNames StringName { get; }

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
            if (_parentMod != null || value == null)
            {
                return;
            }

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
    public Action<T>? ChangedEvent { get; set; }

    /// <summary>
    /// Gets or sets the visibility of the option.
    /// </summary>
    public Func<bool> Visible { get; set; }

    /// <summary>
    /// Gets or sets the advanced role of the option.
    /// </summary>
    public Type? AdvancedRole { get; set; }

    /// <summary>
    /// Gets or sets the option behaviour of the option.
    /// </summary>
    public OptionBehaviour? OptionBehaviour { get; protected set; }

    /// <summary>
    /// Gets or sets the config definition of the option.
    /// </summary>
    public ConfigDefinition? ConfigDefinition
    {
        get => _configDefinition;
        set
        {
            if (_configDefinition is not null)
            {
                return;
            }

            _configDefinition = value;
        }
    }

    private ConfigDefinition? _configDefinition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedOption{T}"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="roleType">The Role Type or null if it doesn't belong to a role.</param>
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

    internal void ValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(GetValueFromOptionBehaviour(optionBehaviour));
    }

    /// <summary>
    /// Sets the value of the option.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    public void SetValue(T newValue)
    {
        var oldVal = Value;
        Value = newValue;

        if (Value?.Equals(oldVal) == false)
        {
            ChangedEvent?.Invoke(Value);
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

    /// <summary>
    /// Gets the float data of the option.
    /// </summary>
    /// <returns>A float object representing the option's value.</returns>
    public abstract float GetFloatData();

    /// <summary>
    /// Gets the net data of the option.
    /// </summary>
    /// <returns>A NetData object representing this option's data.</returns>
    public abstract NetData GetNetData();

    /// <summary>
    /// Handles incoming net data.
    /// </summary>
    /// <param name="data">The NetData's byte array.</param>
    public abstract void HandleNetData(byte[] data);

    /// <summary>
    /// Handles the value changed event.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    protected abstract void OnValueChanged(T newValue);

    /// <summary>
    /// Gets the value from the option behaviour.
    /// </summary>
    /// <param name="optionBehaviour">The OptionBehaviour.</param>
    /// <returns>The value.</returns>
    public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

    /// <summary>
    /// Creates the option behaviour.
    /// </summary>
    /// <param name="toggleOpt">The ToggleOption prefab.</param>
    /// <param name="numberOpt">The NumberOption prefab.</param>
    /// <param name="stringOpt">The StringOption prefab.</param>
    /// <param name="container">The options container.</param>
    /// <returns>A new OptionBehaviour for this modded option.</returns>
    public abstract OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        Transform container);
}
