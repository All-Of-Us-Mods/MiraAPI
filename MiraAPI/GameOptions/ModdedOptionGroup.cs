using System;
using UnityEngine;

namespace MiraAPI.GameOptions
{
    public class ModdedOptionGroup
    {
        public IMiraConfig ParentMod;
        public GameObject Header;
        public string GroupName;
        public Func<bool> GroupVisible;
        public Color GroupColor;
        public Type AdvancedRole;
    }
}
