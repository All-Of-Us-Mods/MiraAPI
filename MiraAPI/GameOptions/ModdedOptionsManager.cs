using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;

namespace MiraAPI.GameOptions;

public class ModdedOptionsManager
{
    public static readonly Dictionary<uint, IModdedOption> ModdedOptions = new();
    public static readonly List<IModdedOption> Options = [];
    public static readonly List<IModdedOptionGroup> Groups = [];
    private static readonly Dictionary<PropertyInfo, ModdedOptionAttribute> OptionAttributes = new();
    public static readonly Dictionary<Type, IModdedOptionGroup> OriginalTypes = new();
    public static uint NextId = 1;

    public static IModdedOption RegisterOption(Type type, ModdedOptionAttribute attribute, PropertyInfo property)
    {
        if (OptionAttributes.ContainsKey(property)) return null;
        object newObj = Activator.CreateInstance(type);
        IModdedOption result = attribute.CreateOption(property.GetValue(newObj), property);

        if (result != null)
        {
            var setterOriginal = property.GetSetMethod();
            var setterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertySetterPatch));
            PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: new HarmonyMethod(setterPatch));

            var getterOriginal = property.GetGetMethod();
            var getterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyGetterPatch));
            PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: new HarmonyMethod(getterPatch));

            attribute.HolderOption = result;
                
            OptionAttributes.Add(property, attribute);

            if (OriginalTypes.ContainsKey(type))
            {
                Logger<MiraApiPlugin>.Error($"Grouping {attribute.Title} with {OriginalTypes[type].GroupName}");
                result.Group = OriginalTypes[type];
            }
        }

        return result;
    }

    public static void SyncAllOptions(int targetId=-1)
    {
        List<NetData> data = [];
        int count = 0;
        foreach (var option in ModdedOptions.Values)
        {
            var netData = option.GetNetData();
            data.Add(netData);
            count += netData.GetLength();
                
            if (count > 1000)
            {
                Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.ToArray());
                data.Clear();
                count = 0;
            }
        }
        if (data.Count > 0)
        {
            Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.ToArray());
        }
    }
        
        
    public static void HandleSyncOptions(NetData[] data)
    {
        foreach (var netData in data)
        {
            if (ModdedOptions.TryGetValue(netData.Id, out var option))
            {
                Logger<MiraApiPlugin>.Error("Handling option " + option.Title);
                option.HandleNetData(netData.Data);
            }
        }
    }
        
        
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
    {
        ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetSetMethod().Equals(__originalMethod)).Value;

        if (attribute != null)
        {
            attribute.SetValue(value);
        }
    }

    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod().Equals(__originalMethod)).Value;

        if (attribute != null)
        {
            __result = attribute.GetValue();
            return false;
        }
        return true;
    }
}