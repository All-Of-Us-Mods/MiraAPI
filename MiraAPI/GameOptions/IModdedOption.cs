using MiraAPI.PluginLoading;
using System;
using MiraAPI.Networking;
using UnityEngine;

namespace MiraAPI.GameOptions
{
    public interface IModdedOption
    {
        public uint Id { get; }
        public BaseGameSetting Data { get; }
        public ModdedOptionGroup Group { get; set; }
        public IMiraPlugin ParentMod { get; set; }
        public Type AdvancedRole { get; }
        public OptionBehaviour OptionBehaviour { get; protected set; }
        public string Title { get; }
        public StringNames StringName { get; }
        public Func<bool> Visible { get; set; }
        public void ValueChanged(OptionBehaviour optionBehaviour);
        public OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);
        public NetData GetNetData();
        public void HandleNetData(byte[] data);
    }
}
