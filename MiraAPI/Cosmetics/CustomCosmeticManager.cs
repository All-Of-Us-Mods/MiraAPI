using HarmonyLib;
using MiraAPI.PluginLoading;
using MonoMod.Utils;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiraAPI.Cosmetics;
public static class CustomCosmeticManager
{
    private static readonly Dictionary<PropertyInfo, RegisterCustomCosmeticAttribute> OptionAttributes = new();
    private static readonly Dictionary<Type, AbstractCosmeticsGroup> TypeToGroup = new();
    internal static readonly Dictionary<CosmeticData, int> _cosmeticToPriority = new();
    internal static readonly Dictionary<CosmeticData, AbstractCosmeticsGroup> _cosmeticToGroup = new();

    internal static readonly List<AbstractCosmeticsGroup> Groups = [];

    internal static void RegisterVanilla()
    {
        if (Activator.CreateInstance(typeof(VanillaCosmeticsGroup)) is not AbstractCosmeticsGroup group)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create group from Vanilla");
            return;
        }
        if (TypeToGroup.ContainsKey(typeof(VanillaCosmeticsGroup)))
        {
            Logger<MiraApiPlugin>.Error($"Vanilla group already exists.");
            return;
        }

        Groups.Add(group);
        TypeToGroup.Add(typeof(VanillaCosmeticsGroup), group);
    }

    internal static bool RegisterGroup(Type type, MiraPluginInfo pluginInfo)
    {
        if (Activator.CreateInstance(type) is not AbstractCosmeticsGroup group)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create group from {type.Name}");
            return false;
        }

        if (TypeToGroup.ContainsKey(type))
        {
            Logger<MiraApiPlugin>.Error($"Group {type.Name} already exists.");
            return false;
        }

        Groups.Add(group);
        TypeToGroup.Add(type, group);
        pluginInfo.CosmeticGroups.Add(group);

        return true;
    }

    internal static void RegisterAttributeOption(Type type, RegisterCustomCosmeticAttribute attribute, PropertyInfo property, MiraPluginInfo pluginInfo)
    {
        if (OptionAttributes.ContainsKey(property))
        {
            Logger<MiraApiPlugin>.Error($"Property {property.Name} already has an attribute registered.");
            return;
        }

        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Logger<MiraApiPlugin>.Error($"Failed to get group for {type.Name}");
            return;
        } 

        OptionAttributes.Add(property, attribute);
    }

    internal static void LoadCosmetic(PropertyInfo property, RegisterCustomCosmeticAttribute attribute)
    {

        if (!TypeToGroup.TryGetValue(property.DeclaringType, out var group))
        {
            Logger<MiraApiPlugin>.Error($"Failed to get group for {property.DeclaringType.Name}");
            return;
        }

        if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<HatData>)))
        {
            group.Hats.AddRange((IEnumerable<HatData>)property.GetValue(group));
        }
        else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<SkinData>)))
        {
            group.Skins.AddRange((IEnumerable<SkinData>)property.GetValue(group));
        }
        else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<VisorData>)))
        {
            group.Visors.AddRange((IEnumerable<VisorData>)property.GetValue(group));
        }
        else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<NamePlateData>)))
        {
            group.Nameplates.AddRange((IEnumerable<NamePlateData>)property.GetValue(group));
        }
        else
        {
            Logger<MiraApiPlugin>.Error($"Failed to get enumerable for {property.Name}");
            return;
        }

        _cosmeticToPriority.AddRange(((IEnumerable<CosmeticData>)property.GetValue(group)).ToDictionary(y => y, x => attribute.Priority));
        _cosmeticToGroup.AddRange(((IEnumerable<CosmeticData>)property.GetValue(group)).ToDictionary(y => y, x => group));
    }

    internal static bool Registered { get; set; }
    internal static void LoadAll()
    {
        CosmeticGroupSingleton<VanillaCosmeticsGroup>.Instance.runtimeRegister();
        if (Registered || !CosmeticGroupSingleton<VanillaCosmeticsGroup>.Instance.registered) return;
        Registered = true;
        _cosmeticToPriority.AddRange(CosmeticGroupSingleton<VanillaCosmeticsGroup>.Instance.Cosmetics.ToDictionary(y => y, x => 0));
        _cosmeticToGroup.AddRange(CosmeticGroupSingleton<VanillaCosmeticsGroup>.Instance.Cosmetics.ToDictionary(y => y, x => CosmeticGroupSingleton<VanillaCosmeticsGroup>.Instance));
        OptionAttributes.Do(x => LoadCosmetic(x.Key,x.Value));
        DestroyableSingleton<HatManager>.Instance.allHats = _cosmeticToGroup.Keys.Where(x=>x is HatData).Select(x=>(HatData)x).ToArray();
        DestroyableSingleton<HatManager>.Instance.allVisors = _cosmeticToGroup.Keys.Where(x=>x is VisorData).Select(x => (VisorData)x).ToArray();
        DestroyableSingleton<HatManager>.Instance.allSkins = _cosmeticToGroup.Keys.Where(x=>x is SkinData).Select(x => (SkinData)x).ToArray();
        DestroyableSingleton<HatManager>.Instance.allNamePlates = _cosmeticToGroup.Keys.Where(x=>x is NamePlateData).Select(x => (NamePlateData)x).ToArray();
    }
}
