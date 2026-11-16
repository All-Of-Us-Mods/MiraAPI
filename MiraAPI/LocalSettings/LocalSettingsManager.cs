using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Manages <see cref="LocalSettingsTab"/>s.
/// </summary>
public static class LocalSettingsManager
{
    internal static readonly Dictionary<Type, LocalSettingsTab> TypeToTab = [];
    internal static readonly List<LocalSettingsTab> Tabs = [];
    internal static readonly List<LocalSettingsTab> AvailableTabs = [];

    internal static bool RegisterLocalTabs(Type type, in MiraPluginManager.RegistrationContext context)
    {
        if (!type.IsAssignableTo(typeof(LocalSettingsTab)))
        {
            return false;
        }

        if (TypeToTab.ContainsKey(type))
        {
            Error($"Local settings tab {type.Name} already exists.");
            return false;
        }

        if (Activator.CreateInstance(type, context.BasePlugin.Config) is not LocalSettingsTab tabInstance)
        {
            return false;
        }

        Tabs.Add(tabInstance);

        if (tabInstance.ShouldCreateButton)
        {
            AvailableTabs.Add(tabInstance);
        }

        TypeToTab.Add(type, tabInstance);

        Helpers.SetInstance(typeof(LocalSettingsTabSingleton<>), type, tabInstance);

        foreach (var property in type.GetProperties(MiraPluginManager.DeclaredInstanceMembers))
        {
            if (property.IsDefined<LocalSettingsButtonAttribute>())
            {
                if (property.GetValue(tabInstance) is not LocalSettingsButton button)
                {
                    Error($"Option property {property.Name} in {type.Name} has to be a settings button.");
                    continue;
                }

                button.Tab = tabInstance;
                tabInstance.Buttons.Add(button);
                continue;
            }

            if (!property.TryGetCustomAttribute<LocalSettingAttribute>(out var attribute))
            {
                continue;
            }

            if (!typeof(ConfigEntryBase).IsAssignableFrom(property.PropertyType))
            {
                continue;
            }

            if (property.GetValue(tabInstance) is not ConfigEntryBase configEntry)
            {
                Error($"Option property {property.Name} in {type.Name} has to be a config entry.");
                continue;
            }

            attribute.CreateSetting(type, configEntry);
        }

        if (!context.MiraPlugin.DebugMode)
            return true;

        foreach (var field in type.GetFields(MiraPluginManager.DeclaredInstanceMembers)
            .Where(f => !f.IsAutoPropertyBackingField()
                && (f.FieldType.IsAssignableTo(typeof(ConfigEntryBase)) || f.FieldType.IsAssignableTo(typeof(LocalSettingsButton)))))
        {
            Error($"{field.Name} is a field, not a property. Use properties for local settings or buttons.");
        }

        return true;
    }
}
