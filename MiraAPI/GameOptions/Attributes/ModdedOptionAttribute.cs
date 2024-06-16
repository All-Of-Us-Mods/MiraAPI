using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ModdedOptionAttribute : Attribute
    {
        public IModdedOption HolderOption { get; set; }
        public string Title { get; private set; }
        public Type RoleType { get; private set; }
        public ModdedOptionAttribute(string title, Type roleType = null)
        {
            Title = title;
            RoleType = roleType;
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (PropertyInfo property in type.GetProperties())
                {
                    foreach (var attribute in property.GetCustomAttributes())
                    {
                        if (attribute.GetType().BaseType == typeof(ModdedOptionAttribute))
                        {
                            ModdedOptionsManager.RegisterOption(assembly, type, (ModdedOptionAttribute)attribute, property);
                        }
                    }
                }
            }
        }

        public abstract void SetValue(object val);
        public abstract object GetValue();
        public abstract IModdedOption CreateOption(object value, PropertyInfo property);
    }
}
