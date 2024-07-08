using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using System;
using MiraAPI.Networking;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public abstract class ModdedOption<T> : IModdedOption
    {
        public uint Id { get; }
        public T Value { get; protected set; }
        public T DefaultValue { get; init; }
        public Action<T> ChangedEvent { get; set; }
        public string Title { get; }
        public StringNames StringName { get; }
        public Func<bool> Visible { get; set; }
        public Type AdvancedRole { get; }
        public OptionBehaviour OptionBehaviour { get; set; }
        public ModdedOptionGroup Group { get; set; } = null;
        public IMiraPlugin ParentMod { get; set; } = null;

        public ModdedOption(string title, T defaultValue, Type roleType) : this()
        {
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
        
        public ModdedOption()
        {
            Logger<MiraApiPlugin>.Error("created modded option");
            Id = ModdedOptionsManager.NextId++;
            ModdedOptionsManager.Options.Add(this);
            ModdedOptionsManager.ModdedOptions.Add(Id, this);
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

            OnValueChanged(newValue);
        }

        public abstract NetData GetNetData();
        public abstract void HandleNetData(byte[] data);

        public abstract void OnValueChanged(T newValue);

        public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

        public abstract OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);

        public abstract string GetHudStringText();
    }
}
