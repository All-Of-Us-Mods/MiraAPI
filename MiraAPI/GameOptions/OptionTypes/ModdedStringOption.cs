﻿using Reactor.Localization.Utilities;
using System.Linq;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedStringOption : ModdedOption<int>
    {
        public string[] Values { get; private set; }
        public ModdedStringOption(string title, int defaultValue, string[] values, System.Type roleType) : base(title, defaultValue, roleType)
        {
            Values = values;
        }

        public override OptionBehaviour CreateOption(OptionBehaviour optionBehaviour, Transform container)
        {
            var stringOption = (StringOption)Object.Instantiate(optionBehaviour, container);

            stringOption.name = Title;
            stringOption.Title = StringName;
            stringOption.Value = Value;
            stringOption.Values = Values.Select(CustomStringName.CreateAndRegister).ToArray();
            stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            stringOption.Initialize();

            OptionBehaviour = stringOption;

            return stringOption;
        }

        public override string GetHudStringText()
        {
            return Title + ": " + Values[Value];
        }

        public override int GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
        {
            return optionBehaviour.GetInt();
        }

        public override void OnValueChanged(int newValue)
        {
            if (OptionBehaviour is null) return;

            var opt = OptionBehaviour as StringOption;
            opt.Value = newValue;
        }
    }
}