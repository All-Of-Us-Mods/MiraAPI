using MiraAPI.PluginLoading;
using System;
using UnityEngine;

namespace MiraAPI.GameOptions
{
    public class ModdedOptionGroup
    {
        public IMiraPlugin ParentMod;
        public GameObject Header;
        public string GroupName;
        public Func<bool> GroupVisible;
        public Color GroupColor;
        public Type AdvancedRole;
    }
}
