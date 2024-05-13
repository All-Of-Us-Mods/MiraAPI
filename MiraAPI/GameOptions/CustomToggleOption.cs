using BepInEx.Configuration;
using Reactor.Utilities;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.API.GameOptions;

public class CustomToggleOption : AbstractGameOption
{
    public bool Value { get; private set; }
    public bool Default { get; }
    public ConfigEntry<bool> Config { get; }
    public Action<bool> ChangedEvent { get; init; }
    public CustomToggleOption(string title, bool defaultValue, Type role = null, bool save = true) : base(title, role, save)
    {
        Default = defaultValue;
        if (Save)
        {
            try
            {
                Config = MiraAPIPlugin.Instance.Config.Bind("Toggle Options", title, defaultValue);
                SetValue(Save ? Config.Value : defaultValue);
            }
            catch (Exception e)
            {
                Logger<MiraAPIPlugin>.Warning(e.ToString());
            }
        }
        CustomOptionsManager.CustomToggleOptions.Add(this);
    }

    public void SetValue(bool newValue)
    {
        if (Save && (!AmongUsClient.Instance || AmongUsClient.Instance.AmHost))
        {
            try
            {
                Config.Value = newValue;
            }
            catch (Exception e)
            {
                Logger<MiraAPIPlugin>.Warning(e.ToString());
            }
        }

        var oldValue = Value;
        Value = newValue;

        var behaviour = (ToggleOption)OptionBehaviour;
        if (behaviour)
        {
            behaviour.CheckMark.enabled = newValue;
        }

        if (newValue != oldValue)
        {
            ChangedEvent?.Invoke(newValue);
        }
    }

    protected override void OnValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(optionBehaviour.GetBool());
    }

    public ToggleOption CreateToggleOption(ToggleOption original, Transform container)
    {
        var toggleOption = Object.Instantiate(original, container);

        toggleOption.name = Title;
        toggleOption.Title = StringName;
        toggleOption.CheckMark.enabled = Value;
        toggleOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
        toggleOption.OnEnable();
        OptionBehaviour = toggleOption;
        return toggleOption;
    }
}