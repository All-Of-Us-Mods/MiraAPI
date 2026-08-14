using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx.Unity.IL2CPP;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Manages <see cref="LocalSettingsTab"/>s.
/// </summary>
public static class LocalSettingsManager
{
    internal static readonly Dictionary<Type, LocalSettingsTab> TypeToTab = [];
    internal static readonly List<LocalSettingsTab> Tabs = [];
    internal static readonly List<LocalSettingsTab> AvailableTabs = [];

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime."
    )]
    internal static bool RegisterTab(Type type, BasePlugin pluginInfo)
    {
        if (Activator.CreateInstance(type, pluginInfo.Config) is not LocalSettingsTab tab)
        {
            return false;
        }

        if (TypeToTab.ContainsKey(type))
        {
            Error($"Local settings tab {type.Name} already exists.");
            return false;
        }

        Tabs.Add(tab);
        if (tab.ShouldCreateButton)
        {
            AvailableTabs.Add(tab);
        }
        TypeToTab.Add(type, tab);

        typeof(LocalSettingsTabSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)! // Suppression message refers to this
            .SetValue(null, tab);

        return true;
    }
}
