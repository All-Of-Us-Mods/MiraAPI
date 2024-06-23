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

        public abstract void SetValue(object val);
        public abstract object GetValue();
        public abstract IModdedOption CreateOption(object value, PropertyInfo property);
    }
}
