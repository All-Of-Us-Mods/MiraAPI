using BepInEx.Unity.IL2CPP;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RegisterModdedOptionAttribute : Attribute
    {
        private static readonly HashSet<Assembly> RegisteredAssemblies = [];

        public string Title { get; private set; }
        public RegisterModdedOptionAttribute(string title)
        {
            Title = title;
        }

        public static void Initialize()
        {
            IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly);
        }

        public static void Register(Assembly assembly)
        {
            if (!RegisteredAssemblies.Add(assembly)) return;

            foreach (var type in assembly.GetTypes())
            {
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public))
                {
                    var attribute = property.GetCustomAttribute<RegisterModdedOptionAttribute>();
                    if (attribute != null)
                    {
                        ModdedOptionsManager.Instance.CreateOption(type, attribute, property);
                    }
                }
            }
        }
    }
}
