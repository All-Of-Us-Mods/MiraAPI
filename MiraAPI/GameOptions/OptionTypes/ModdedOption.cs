using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
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
    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public StringNames StringName { get; }

    /// <inheritdoc />
    public BaseGameSetting Data { get; protected set; } = null!;

    /// <inheritdoc />
    public IMiraPlugin? ParentMod
    {
        get;
        set
        {
            if (field != null || value == null) return;
            field = value;

            var entry = field.GetConfigFile().Bind(ConfigDefinition, DefaultValue);
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

    /// <inheritdoc />
    public Func<bool> Visible { get; set; }

    /// <inheritdoc />
    public bool IncludeInPreset { get; set; }

    /// <inheritdoc />
    public OptionBehaviour? OptionBehaviour { get; protected set; }

    /// <inheritdoc />
    public ConfigDefinition? ConfigDefinition { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedOption{T}"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="includeInPreset">Whether to include the option in the preset.</param>
    protected ModdedOption(string title, T defaultValue, bool includeInPreset = true)
    {
        Id = ModdedOptionsManager.NextId;
        Title = title.Translate();
        DefaultValue = defaultValue;
        Value = defaultValue;
        StringName = MiraLocaleManager.GetOrCreateLocaleString(Title);
        Visible = () => true;
        IncludeInPreset = includeInPreset;
    }

    internal void ValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(GetValueFromOptionBehaviour(optionBehaviour));
    }

    /// <summary>
    /// Sets the value of the option.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    /// <param name="sendRpc">Whether to send the value to other players.</param>
    public void SetValue(T newValue, bool sendRpc = true)
    {
        var oldVal = Value;
        Value = newValue;

        if (Value?.Equals(oldVal) == false)
        {
            ChangedEvent?.Invoke(Value);
        }

        if (sendRpc && AmongUsClient.Instance.AmHost)
        {
            if (ParentMod?.GetConfigFile().TryGetEntry<T>(ConfigDefinition, out var entry) == true)
            {
                entry.Value = Value;
            }

            Rpc<SyncOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [GetNetData()], true);
        }

        OnValueChanged(newValue);
    }

    /// <inheritdoc />
    public void SaveToPreset(ConfigFile presetConfig, bool saveDefault=false)
    {
        if (ConfigDefinition is null)
        {
            Error($"Attempted to save {Title} to preset, but ConfigDefinition is null.");
            return;
        }
        Bind(presetConfig);
        presetConfig[ConfigDefinition].BoxedValue = saveDefault ? DefaultValue : Value;
    }

    /// <inheritdoc />
    public void Bind(ConfigFile config)
    {
        config.Bind(ConfigDefinition, DefaultValue);
    }

    /// <inheritdoc />
    public void LoadFromPreset(ConfigFile presetConfig)
    {
        if (presetConfig.TryGetEntry(ConfigDefinition, out ConfigEntry<T> entry))
        {
            SetValue(entry.Value, false);
        }
        else
        {
            Error($"Attempted to load {Title} from preset, but ConfigDefinition is not found in preset.");
        }
    }

    /// <inheritdoc />
    public abstract float GetFloatData();

    /// <inheritdoc />
    public abstract NetData GetNetData();

    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        PlayerOption playerOpt,
        Transform container);

    /// <summary>
    /// Implicitly converts the option to type of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>Value of type <typeparamref name="T"/>.</returns>
    public static implicit operator T(ModdedOption<T> option)
    {
        return option.Value;
    }

    /// <inheritdoc />
    public AbstractOptionGroup ParentGroup { get; set; }

    /// <inheritdoc />
    public virtual OptionNotifConfiguration Configuration => new(ParentGroup);
}
