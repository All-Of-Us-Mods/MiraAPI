global using static Reactor.Utilities.Logger<MiraAPI.MiraApiPlugin>;

using System;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.VanillaEvents;
using Reactor;
using Reactor.Localization;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// The main plugin class for Mira API.
/// </summary>
[BepInAutoPlugin("mira.api", "MiraAPI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class MiraApiPlugin : BasePlugin, IMiraPlugin
{
    /// <summary>
    /// Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; internal set; } = new("en-US");

    /// <inheritdoc />
    public string OptionsTitleText => "MiraAPI";

    /// <inheritdoc />
    public bool DisplayOnOptionsMenu => false;

    /// <summary>
    /// Gets a value indicating whether the current device is running Starlight (on mobile).
    /// </summary>
    public static bool IsMobile => Constants.GetPlatformType() is Platforms.Android or Platforms.IPhone;

    /// <summary>
    /// Gets the branding Mira API color.
    /// </summary>
    public static Color MiraColor { get; } = new Color32(238, 154, 112, 255);

    /// <summary>
    /// Gets the default color for headers in the options menu.
    /// </summary>
    public static Color DefaultHeaderColor { get; } = new Color32(77, 77, 77, 255);

    /// <summary>
    /// Gets a value indicating whether the current build is a development build or not. This mostly avoids confusion for users asking why the mod appears red.
    /// </summary>
    public static bool IsDevBuild => Version.Contains("ci", StringComparison.OrdinalIgnoreCase) || Version.Contains("dev", StringComparison.OrdinalIgnoreCase);

    private static MiraPluginManager? PluginManager { get; set; }
    internal Harmony Harmony { get; } = new(Id);

    /// <inheritdoc />
    public override void Load()
    {
        Harmony.PatchAll();
        UiResetEvents.Initialize();
        JudgeEvents.Initialize();

        ReactorCredits.Register("Mira API", Version, IsDevBuild, ReactorCredits.AlwaysShow);

        PluginManager = new MiraPluginManager();
        PluginManager.Initialize(this, this);

        MiraLocaleManager.Register("mira.api");
        LocalizationManager.Register(new MiraLocalizationProvider());

        IL2CPPChainloader.Instance.Finished +=
            ModCompatibility
                .Initialize; // Initialise AFTER the mods are loaded to ensure maximum parity (no need for the soft dependency either then)
    }

    /// <inheritdoc />
    public ConfigFile GetConfigFile()
    {
        return Config;
    }
}
