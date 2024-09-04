using System;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using UnityEngine;

namespace MiraAPI.GameOptions;

internal interface IModdedOption
{
    uint Id { get; }
    string Title { get; }
    StringNames StringName { get; }
    IMiraPlugin? ParentMod { get; set; }
    BaseGameSetting? Data { get; }
    Type? AdvancedRole { get; set; }
    OptionBehaviour? OptionBehaviour { get; }
    Func<bool> Visible { get; set; }
    ConfigDefinition? ConfigDefinition { get; set; }
    OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container);
    float GetFloatData();
    NetData GetNetData();
    void HandleNetData(byte[] data);
}
