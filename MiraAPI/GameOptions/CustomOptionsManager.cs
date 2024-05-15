using BepInEx;
using BepInEx.Unity.IL2CPP;
using MiraAPI.GameOptions;
using MiraAPI.Networking.Options;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.API.GameOptions;

public static class CustomOptionsManager
{
    public static readonly List<AbstractGameOption> CustomOptions = [];
    public static readonly List<CustomNumberOption> CustomNumberOptions = [];
    public static readonly List<CustomToggleOption> CustomToggleOptions = [];
    public static readonly List<CustomStringOption> CustomStringOptions = [];
    public static readonly List<CustomOptionGroup> CustomGroups = [];
    public static readonly List<PluginInfo> RegisteredMods = [];

    public static void RegisterOptionsForMod(Type type, List<FieldInfo> fields, string modId)
    {
        if (!typeof(IModOptions).IsAssignableFrom(type))
        {
            Debug.LogError($"Cannot register options for {modId}! Missing IModOptions interface.");
            return;
        }
        PluginInfo info = IL2CPPChainloader.Instance.Plugins[modId];

        if (info == null)
        {
            return;
        }

        Debug.Log($"Registering Options for {modId}");

        RegisteredMods.Add(info);
        foreach (FieldInfo field in fields)
        {
            AbstractGameOption option = (AbstractGameOption)field.GetValue(null);
            if (option == null)
            {
                CustomOptionGroup group = (CustomOptionGroup)field.GetValue(null);
                group.ParentMod = info;
                CustomGroups.Add(group);
            }

            option.ParentMod = info;

            CustomOptions.Add(option);
            switch (field.FieldType.Name)
            {
                case nameof(CustomNumberOption):
                    CustomNumberOptions.Add((CustomNumberOption)option);
                    break;

                case nameof(CustomToggleOption):
                    CustomToggleOptions.Add((CustomToggleOption)option);
                    break;

                case nameof(CustomStringOption):
                    CustomStringOptions.Add((CustomStringOption)option);
                    break;

            }
        }
    }

    public static void SyncOptions()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var toggles = CustomToggleOptions.Select(x => x.Value).ToArray();
        var numbers = CustomNumberOptions.Select(x => x.Value).ToArray();
        var strings = CustomStringOptions.Select(x => x.IndexValue).ToArray();

        Rpc<SyncOptionsRpc>.Instance.Send(new SyncOptionsRpc.Data(toggles, numbers, strings));
    }

    public static void SyncOptions(int targetId)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var toggles = CustomToggleOptions.Select(x => x.Value).ToArray();
        var numbers = CustomNumberOptions.Select(x => x.Value).ToArray();
        var strings = CustomStringOptions.Select(x => x.IndexValue).ToArray();

        Rpc<SyncOptionsRpc>.Instance.SendTo(targetId, new SyncOptionsRpc.Data(toggles, numbers, strings));
    }

    public static void UpdateToConfig()
    {
        foreach (var numberOpt in CustomNumberOptions)
        {
            numberOpt.SetValue(numberOpt.Save ? numberOpt.Config.Value : numberOpt.Default);
        }

        foreach (var toggleOpt in CustomToggleOptions)
        {
            toggleOpt.SetValue(toggleOpt.Save ? toggleOpt.Config.Value : toggleOpt.Default);
        }

        foreach (var stringOpt in CustomStringOptions)
        {
            stringOpt.SetValue(stringOpt.Save ? stringOpt.Config.Value : stringOpt.Default);
        }
    }


    public static void ResetToDefault()
    {
        foreach (var numberOpt in CustomNumberOptions)
        {
            numberOpt.SetValue(numberOpt.Default);
        }

        foreach (var stringOpt in CustomStringOptions)
        {
            stringOpt.SetValue(stringOpt.Default);
        }

        foreach (var toggleOpt in CustomToggleOptions)
        {
            toggleOpt.SetValue(toggleOpt.Default);
        }

        foreach (var option in CustomOptions)
        {
            if (!option.OptionBehaviour)
            {
                continue;
            }

            option.ValueChanged(option.OptionBehaviour);
        }
    }

    public static void HandleOptionsSync(bool[] toggles, float[] numbers, int[] strings)
    {
        Logger<MiraAPIPlugin>.Warning("syncing options!");
        for (var i = 0; i < toggles.Length; i++)
        {
            CustomToggleOptions[i].SetValue(toggles[i]);
        }
        for (var i = 0; i < numbers.Length; i++)
        {
            CustomNumberOptions[i].SetValue(numbers[i]);
        }
        for (var i = 0; i < strings.Length; i++)
        {
            CustomStringOptions[i].SetValue(strings[i]);
        }
    }

}