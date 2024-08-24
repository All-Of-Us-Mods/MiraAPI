using MiraAPI.PluginLoading;
using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using UnityEngine;

namespace MiraAPI.GameOptions;

public interface IModdedOption
{
    public uint Id { get; }
    public BaseGameSetting Data { get; }
    public IMiraPlugin ParentMod { get; set; }
    public Type AdvancedRole { get; set; }
    public OptionBehaviour OptionBehaviour { get; protected set; }
    public string Title { get; }
    public StringNames StringName { get; }
    public Func<bool> Visible { get; set; }
    public ConfigDefinition ConfigDefinition { get; }
    public void ValueChanged(OptionBehaviour optionBehaviour);
    public OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);
    public NetData GetNetData();
    public void HandleNetData(byte[] data);
}