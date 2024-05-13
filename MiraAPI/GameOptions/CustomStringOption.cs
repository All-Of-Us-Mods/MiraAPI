using BepInEx.Configuration;
using Reactor.Localization.Utilities;
using Reactor.Utilities;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.API.GameOptions;

public class CustomStringOption : AbstractGameOption
{
    public int IndexValue { get; private set; }
    public string Value => Options[IndexValue];
    public int Default { get; }
    public string[] Options { get; private set; }
    public ConfigEntry<int> Config { get; }
    public Action<int, string> ChangedEvent { get; init; }
    public CustomStringOption(string title, int defaultValue, string[] options, Type role = null, bool save = true) : base(title, role, save)
    {
        IndexValue = defaultValue;
        Options = options;
        Default = defaultValue;

        CustomOptionsManager.CustomStringOptions.Add(this);
        if (Save)
        {
            try
            {
                Config = MiraAPIPlugin.Instance.Config.Bind("String Options", title, defaultValue);
                SetValue(Save ? Config.Value : defaultValue);
            }
            catch (Exception e)
            {
                Logger<MiraAPIPlugin>.Warning(e.ToString());
            }
        }

    }

    public void SetValue(int newValue)
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

        var oldValue = IndexValue;
        IndexValue = newValue;

        var behaviour = (StringOption)OptionBehaviour;
        if (behaviour)
        {
            behaviour.Value = newValue;
        }

        if (oldValue != newValue)
        {
            ChangedEvent?.Invoke(newValue, Value);
        }
    }

    public void SetValue(string newValue) => SetValue(Options.ToList().IndexOf(newValue));

    protected override void OnValueChanged(OptionBehaviour optionBehaviour)
    {
        SetValue(optionBehaviour.GetInt());
    }

    public StringOption CreateStringOption(StringOption original, Transform container)
    {
        var stringOption = Object.Instantiate(original, container);

        stringOption.name = Title;
        stringOption.Title = StringName;
        stringOption.Value = Options.ToList().IndexOf(Value);
        stringOption.Values = Options.Select(CustomStringName.CreateAndRegister).ToArray();
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
        stringOption.OnEnable();

        OptionBehaviour = stringOption;

        return stringOption;
    }
}