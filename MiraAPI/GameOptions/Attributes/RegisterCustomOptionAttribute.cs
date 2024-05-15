using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RegisterCustomOptionAttribute : Attribute
    {
        private static readonly HashSet<Assembly> RegisteredAssemblies = [];
        public string ModId { get; }

        public RegisterCustomOptionAttribute(string modId)
        {
            ModId = modId;
        }
    }
}
