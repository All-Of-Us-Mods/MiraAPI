using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using System;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public abstract class ModdedOption<T> : IModdedOption
    {
        public T Value { get; set; }
        public T DefaultValue { get; set; }
        public Action<T> ChangedEvent { get; private set; }
        public string Title { get; }
        public StringNames StringName { get; }
        public Func<bool> Visible { get; set; }
        public Type AdvancedRole { get; }
        public OptionBehaviour OptionBehaviour { get; set; }
        public ModdedOptionGroup Group { get; set; } = null;
        public IMiraConfig ParentMod { get; set; } = null;

        public ModdedOption(string title, T defaultValue, Type roleType)
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

        public void ValueChanged(OptionBehaviour optionBehaviour)
        {
            SetValue(GetValueFromOptionBehaviour(optionBehaviour));
        }

        public void SetValue(T newValue)
        {
            T oldVal = Value;
            Value = newValue;
            if (!Value.Equals(oldVal) && ChangedEvent != null)
            {
                ChangedEvent.Invoke(Value);
            }

            OnValueChanged(newValue);
        }

        public abstract void OnValueChanged(T newValue);

        public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);

        public abstract OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);

        public abstract string GetHudStringText();
    }
}
