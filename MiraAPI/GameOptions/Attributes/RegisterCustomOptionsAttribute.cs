using BepInEx.Unity.IL2CPP;
using MiraAPI.API.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterCustomOptionsAttribute : Attribute
    {
        private static readonly HashSet<Assembly> RegisteredAssemblies = [];
        public string ModId { get; }
        public List<FieldInfo> Options;

        public RegisterCustomOptionsAttribute(string modId)
        {
            ModId = modId;
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
                var attribute = type.GetCustomAttribute<RegisterCustomOptionsAttribute>();
                if (attribute != null)
                {
                    attribute.Options = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(fi => fi.FieldType.BaseType == typeof(AbstractGameOption) || fi.FieldType.BaseType == typeof(CustomOptionGroup)).ToList();
                    CustomOptionsManager.RegisterOptionsForMod(type, attribute.Options, attribute.ModId);
                }
            }
        }
    }
}
