using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.GameOptions
{
    public class ModdedOptionsManager
    {
        public static ModdedOptionsManager Instance;
        public List<IModdedOption> Options = new List<IModdedOption>();

        private Dictionary<PropertyInfo, IModdedOption> PatchedOptions = new Dictionary<PropertyInfo, IModdedOption>();

        public void CreateOption(Type type, RegisterModdedOptionAttribute attribute, PropertyInfo property)
        {
            Debug.LogError(property.GetConstantValue().GetType().ToString());

            switch (property.GetConstantValue())
            {
                case bool:
                    CreateToggleOption(type, attribute, property);
                    break;
                default:
                    Debug.LogError($"Mira API does not support type {property.GetConstantValue().GetType()}");
                    break;
            }
        }

        public static void PropertySetterPatch(MethodBase __originalMethod, object[] args)
        {
            IModdedOption opt = Instance.PatchedOptions.First(pair => pair.Key.GetSetMethod().Equals(__originalMethod)).Value;

            if (opt != null)
            {
                object value = args[0];
                switch (value)
                {
                    case bool:
                        ModdedToggleOption toggleOpt = opt as ModdedToggleOption;
                        toggleOpt.SetValue((bool)value);
                        break;
                }
            }
        }

        public ModdedToggleOption CreateToggleOption(Type type, RegisterModdedOptionAttribute attribute, PropertyInfo property)
        {
            var toggleOpt = new ModdedToggleOption(attribute.Title, (bool)property.GetValue(type));
            Options.Add(toggleOpt);

            var setterOriginal = property.GetSetMethod();
            var setterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertySetterPatch));
            PluginSingleton<MiraAPIPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: new HarmonyMethod(setterPatch));
            /*
                        var getterOriginal = property.GetGetMethod();
                        var getterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyGetterPatch));
                        PluginSingleton<MiraAPIPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: new HarmonyMethod(getterPatch));*/

            toggleOpt.PropertyInfo = property;

            if (!PatchedOptions.ContainsKey(property))
                PatchedOptions.Add(property, toggleOpt);

            return toggleOpt;
        }
    }
}
