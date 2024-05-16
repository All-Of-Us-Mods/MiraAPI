using Reactor.Localization.Utilities;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.OptionTypes
{
    public abstract class ModdedOption<T> : IModdedOption
    {
        public T Value { get; set; }
        public T DefaultValue { get; set; }
        public Action<T> ChangedEvent { get; private set; }

        public PropertyInfo PropertyInfo;
        public string Title { get; }
        public StringNames StringName { get; }
        public Func<bool> Hidden { get; set; }
        public OptionBehaviour OptionBehaviour { get; protected set; }

        public ModdedOption(string title, T defaultValue)
        {
            Title = title;
            DefaultValue = defaultValue;
            Value = defaultValue;
            StringName = CustomStringName.CreateAndRegister(Title);
            Hidden = () => false;
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

            PropertyInfo.SetValue(null, newValue);
        }

        public abstract T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour);
    }
}
