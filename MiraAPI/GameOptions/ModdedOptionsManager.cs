using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
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
        public static List<IModdedOption> Options = new();
        public static List<ModdedOptionGroup> Groups = new();
        private static Dictionary<PropertyInfo, ModdedOptionAttribute> OptionAttributes = new();
        private static Dictionary<Type, ModdedOptionGroup> OriginalTypes = new();
        public static Dictionary<Assembly, IMiraConfig> RegisteredMods = new();

        public static void Initialize()
        {
            IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) =>
            {
                if (plugin.GetType().GetInterfaces().Contains(typeof(IMiraConfig)))
                {
                    IMiraConfig config = (IMiraConfig)Activator.CreateInstance(plugin.GetType());
                    RegisteredMods.Add(assembly, config);
                }

                RegisterOptionGroups(assembly);

                ModdedOptionAttribute.Register(assembly);
            };
        }

        private static void RegisterOptionGroups(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IModdedOptionGroup).IsAssignableFrom(type))
                {
                    IModdedOptionGroup group = (IModdedOptionGroup)Activator.CreateInstance(type);
                    ModdedOptionGroup newGroup = new ModdedOptionGroup()
                    {
                        AdvancedRole = group.AdvancedRole,
                        GroupColor = group.GroupColor,
                        GroupName = group.GroupName,
                        GroupVisible = group.GroupVisible
                    };

                    Groups.Add(newGroup);
                    OriginalTypes.Add(type, newGroup);

                    newGroup.ParentMod = RegisteredMods[assembly];
                }
            }
        }

        public static void RegisterOption(Assembly assembly, Type type, ModdedOptionAttribute attribute, PropertyInfo property)
        {
            if (OptionAttributes.ContainsKey(property)) return;
            object newObj = Activator.CreateInstance(type);
            IModdedOption result = attribute.CreateOption(property.GetValue(newObj), property);

            if (result != null)
            {
                var setterOriginal = property.GetSetMethod();
                var setterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertySetterPatch));
                PluginSingleton<MiraAPIPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: new HarmonyMethod(setterPatch));

                var getterOriginal = property.GetGetMethod();
                var getterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyGetterPatch));
                PluginSingleton<MiraAPIPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: new HarmonyMethod(getterPatch));

                attribute.HolderOption = result;
                result.ParentMod = RegisteredMods[assembly];

                Options.Add(result);
                OptionAttributes.Add(property, attribute);

                if (OriginalTypes.ContainsKey(type))
                {
                    Debug.LogError($"grouping {attribute.Title} with {OriginalTypes[type].GroupName}");
                    result.Group = OriginalTypes[type];
                }
            }
        }

        public static void PropertySetterPatch(MethodBase __originalMethod, object value)
        {
            ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetSetMethod().Equals(__originalMethod)).Value;

            if (attribute != null)
            {
                attribute.SetValue(value);
            }
        }

        public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
        {
            ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod().Equals(__originalMethod)).Value;

            if (attribute != null)
            {
                __result = attribute.GetValue();
                return false;
            }
            return true;
        }
    }
}
