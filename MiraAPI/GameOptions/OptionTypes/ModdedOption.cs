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
    private IMiraPlugin _parentMod;

    public uint Id { get; }
    public BaseGameSetting Data { get; protected init; }
    public IMiraPlugin ParentMod
    {
        get => _parentMod;
        set
        {
            _parentMod = value;
            var entry = _parentMod.GetConfigFile().Bind(ConfigDefinition, DefaultValue);
            Value = entry.Value;
        }
    }
    public T Value { get; protected set; }
    public T DefaultValue { get; init; }
    public Action<T> ChangedEvent { get; set; }
    public string Title { get; }
    public StringNames StringName { get; }
    public Func<bool> Visible { get; set; }
    public Type AdvancedRole { get; set; }
    public OptionBehaviour OptionBehaviour { get; set; }
    public ConfigDefinition ConfigDefinition => new("Options", Title);

    protected ModdedOption(string title, T defaultValue, Type roleType)
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
            Rpc<SyncOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [GetNetData()], true);
        }

        if (ParentMod?.GetConfigFile().TryGetEntry<T>(ConfigDefinition, out var entry) == true)
        {
            entry.Value = Value;
        }

        OnValueChanged(newValue);
    }

    public abstract NetData GetNetData();

    public abstract void HandleNetData(byte[] data);

    public abstract void OnValueChanged(T newValue);

    public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

    public abstract OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);

    public abstract string GetHudStringText();
}