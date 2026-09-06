using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraAPI.Translation;

/// <summary>
/// The locale manager to handle translations and more.
/// </summary>
public static class MiraLocaleManager
{
    private const string LangDirectory = "mira_languages";

    // Language, XML Name, then Value

    /// <summary>
    /// Gets the dictionary containing all loaded localization strings, organized by language and XML name.
    /// </summary>
    public static Dictionary<MiraLanguage, Dictionary<string, string>> Locale { get; } = [];

    /// <summary>
    /// Gets the list of mod GUIDs that have been registered with the localization manager.
    /// </summary>
    public static readonly List<string> RegisteredModIds = [];

    /// <summary>
    /// Gets a dictionary mapping registered string keys to their corresponding <see cref="StringNames"/> enum values.
    /// </summary>
    public static readonly Dictionary<string, StringNames> RegisteredStringNames = [];
    internal static readonly Dictionary<StringNames, string> StringNamesLookup = [];

    /// <summary>
    /// Gets a dictionary mapping <see cref="MiraLanguage"/> values to their standard language codes.
    /// </summary>
    public static Dictionary<MiraLanguage, string> LangList { get; } = new()
    {
        { MiraLanguage.English, "en_US" },
        { MiraLanguage.Latam, "es_419" },
        { MiraLanguage.Brazilian, "pt_BR" },
        { MiraLanguage.Portuguese, "pt_PT" },
        { MiraLanguage.Korean, "ko_KR" },
        { MiraLanguage.Russian, "ru_RU" },
        { MiraLanguage.Dutch, "nl_NL" },
        { MiraLanguage.Filipino, "fil_PH" },
        { MiraLanguage.French, "fr_FR" },
        { MiraLanguage.German, "de_DE" },
        { MiraLanguage.Italian, "it_IT" },
        { MiraLanguage.Japanese, "ja_JP" },
        { MiraLanguage.Spanish, "es_ES" },
        { MiraLanguage.SChinese, "zh_CN" },
        { MiraLanguage.TChinese, "zh_TW" },
        { MiraLanguage.Irish, "ga_IE" },
        { MiraLanguage.Polish, "pl_PL" }, // Custom
        { MiraLanguage.Turkish, "tr_TR" }, // Custom
        { MiraLanguage.Swedish, "sv_SE" }, // Custom
        { MiraLanguage.Lithuanian, "lt_LT" }, // Custom
        { MiraLanguage.Czech, "cs_CZ" }, // Custom
    };

    /// <summary>
    /// Gets a dictionary mapping <see cref="MiraLanguage"/> values to their standard language codes.
    /// </summary>
    public static Dictionary<MiraLanguage, string> LangCultureList { get; } = new()
    {
        { MiraLanguage.English, "en-US" },
        { MiraLanguage.Latam, "es-419" },
        { MiraLanguage.Brazilian, "pt-BR" },
        { MiraLanguage.Portuguese, "pt-PT" },
        { MiraLanguage.Korean, "ko-KR" },
        { MiraLanguage.Russian, "ru-RU" },
        { MiraLanguage.Dutch, "nl-NL" },
        { MiraLanguage.Filipino, "fil-PH" },
        { MiraLanguage.French, "fr-FR" },
        { MiraLanguage.German, "de-DE" },
        { MiraLanguage.Italian, "it-IT" },
        { MiraLanguage.Japanese, "ja-JP" },
        { MiraLanguage.Spanish, "es-ES" },
        { MiraLanguage.SChinese, "zh-CN" },
        { MiraLanguage.TChinese, "zh-TW" },
        { MiraLanguage.Irish, "ga-IE" },
        { MiraLanguage.Polish, "pl-PL" }, // Custom
        { MiraLanguage.Turkish, "tr-TR" }, // Custom
        { MiraLanguage.Swedish, "sv-SE" }, // Custom
        { MiraLanguage.Lithuanian, "lt-LT" }, // Custom
        { MiraLanguage.Czech, "cs-CZ" }, // Custom
    };

    /// <summary>
    /// Gets the current language from Among Us settings.
    /// </summary>
    public static MiraLanguage CurrentLanguage
    {
        get
        {
            try
            {
                var langName = AmongUs.Data.DataManager.Settings.Language.CurrentLanguage;
                return (MiraLanguage)langName;
            }
            catch
            {
                return MiraLanguage.English;
            }
        }
    }

    /// <summary>
    /// Registers translations for a mod.
    /// Call once per mod during plugin Load().
    /// </summary>
    /// <param name="modGuid">The mod GUID (e.g., "mira.example").</param>
    public static void Register(string modGuid)
    {
        var callingAssembly = Assembly.GetCallingAssembly();

        LoadInternalStrings(callingAssembly, callingAssembly.GetName().Name!, modGuid);
    }

    /// <summary>
    /// Registers translations for a mod.
    /// Call once per mod during plugin Load().
    /// </summary>
    /// <param name="modGuid">The mod GUID (e.g., "mira.example").</param>
    /// <param name="internalName">The mod's root namespace (e.g., "mira.api").</param>
    public static void Register(string modGuid, string internalName)
    {
        var callingAssembly = Assembly.GetCallingAssembly();

        LoadInternalStrings(callingAssembly, internalName, modGuid);
    }

    /// <summary>
    /// Translates a key into the current language.
    /// </summary>
    /// <param name="key">The string id to find.</param>
    /// <param name="fallback">Fallback string to use if no translation is found.</param>
    /// <returns>A <see cref="string"/> based on the key provided.</returns>
    public static string Get(string key, string fallback = "")
    {
        return Get(CurrentLanguage, key, fallback != string.Empty ? fallback : key);
    }

    /// <summary>
    /// Translates a key into the current language.
    /// </summary>
    /// <param name="language">The specific language to prioritize.</param>
    /// <param name="key">The string id to find.</param>
    /// <param name="fallback">Fallback string to use if no translation is found.</param>
    /// <returns>A <see cref="string"/> based on the key provided.</returns>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Warning cascades into forcing the entire tree to be ternary operators.")]
    public static string Get(MiraLanguage language, string key, string fallback = "")
    {
        if (Locale.TryGetValue(language, out var translations) &&
            translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        if (Locale.TryGetValue(MiraLanguage.English, out var translationsEng) &&
            translationsEng.TryGetValue(key, out var translationEng))
        {
            return translationEng;
        }

        return fallback;
    }

    /// <summary>
    /// Translates a key into the current language.
    /// </summary>
    /// <param name="key">The string id to find.</param>
    /// <param name="parseList">List of keys to change into other text.</param>
    /// <param name="fallback">Fallback string to use if no translation is found.</param>
    /// <returns>A <see cref="string"/> based on the key provided.</returns>
    public static string GetParsed(
        string key,
        Dictionary<string, string> parseList,
        string fallback = "")
    {
        return GetParsed(CurrentLanguage, key, parseList, fallback != string.Empty ? fallback : key);
    }

    /// <summary>
    /// Translates a key into the current language.
    /// </summary>
    /// <param name="language">The specific language to prioritize.</param>
    /// <param name="key">The string id to find.</param>
    /// <param name="parseList">List of keys to change into other text.</param>
    /// <param name="fallback">Fallback string to use if no translation is found.</param>
    /// <returns>A <see cref="string"/> based on the key provided.</returns>
    public static string GetParsed(
        MiraLanguage language,
        string key,
        Dictionary<string, string> parseList,
        string fallback = "")
    {
        var text = fallback;

        if (Locale.TryGetValue(MiraLanguage.English, out var translationsEng) &&
            translationsEng.TryGetValue(key, out var translationEng))
        {
            text = translationEng;
        }

        if (language is not MiraLanguage.English &&
            Locale.TryGetValue(language, out var translations) &&
            translations.TryGetValue(key, out var translation))
        {
            text = translation;
        }

        foreach (var tmpText in parseList.Where(x => text.Contains(x.Key)))
        {
            text = text.Replace(tmpText.Key, tmpText.Value);
        }

        return text;
    }

    /// <summary>
    /// Builds a formatted translation ID string using a mod ID, a specific ID part, and a suffix.
    /// </summary>
    /// <param name="modId">The mod GUID.</param>
    /// <param name="idPart">The core part of the ID. If it starts with '#', the character is stripped.</param>
    /// <param name="suffix">The suffix to append to the ID.</param>
    /// <returns>A formatted translation ID <see cref="string"/>.</returns>
    public static string BuildTranslationId(string modId, string idPart, string suffix)
    {
        return idPart.StartsWith('#') ? idPart[1..] : $"{modId}.{idPart}.{suffix}";
    }

    /// <summary>
    /// Builds a formatted translation ID string using a mod ID and a specific ID part.
    /// </summary>
    /// <param name="modId">The mod GUID.</param>
    /// <param name="idPart">The core part of the ID. If it starts with '#', the character is stripped.</param>
    /// <returns>A formatted translation ID <see cref="string"/>.</returns>
    public static string BuildTranslationId(string modId, string idPart)
    {
        return idPart.StartsWith('#') ? idPart[1..] : $"{modId}.{idPart}";
    }

    private static string GetModLangDir(string modGuid)
    {
        return Path.Combine(Application.persistentDataPath, LangDirectory, modGuid);
    }

    /// <summary>
    /// Retrieves the <see cref="StringNames"/> value for a given locale string name, creating and registering a new one if it does not already exist.
    /// </summary>
    /// <param name="name">The name of the locale string.</param>
    /// <returns>The corresponding <see cref="StringNames"/> value.</returns>
    public static StringNames GetOrCreateLocaleString(string name)
    {
        if (RegisteredStringNames.TryGetValue(name, out var stringName))
        {
            return stringName;
        }

        var newString = CustomStringName.CreateAndRegister(name);
        RegisteredStringNames.Add(name, newString);
        StringNamesLookup.Add(newString, name);
        return newString;
    }

    /// <summary>
    /// Loads external locale XML files by checking predefined directories, including the plugin path, BepInEx root, game root, and registered mod directories.
    /// </summary>
    public static void LoadExternalLocale()
    {
        CheckExternalDirectory("mira.api", BepInEx.Paths.PluginPath);
        CheckExternalDirectory("mira.api", BepInEx.Paths.BepInExRootPath);
        CheckExternalDirectory("mira.api", BepInEx.Paths.GameRootPath);
        foreach (var mod in RegisteredModIds)
        {
            CheckExternalDirectory(mod, GetModLangDir(mod));
        }
    }

    /// <summary>
    /// Checks a specific directory for XML language files and parses their contents into the <see cref="Locale"/> dictionary.
    /// </summary>
    /// <param name="modGuid">The mod GUID associated with the directory.</param>
    /// <param name="dir">The directory path to search for XML files.</param>
    public static void CheckExternalDirectory(string modGuid, string dir)
    {
        Directory.CreateDirectory(dir);

        foreach (var filePath in Directory.GetFiles(dir, "*.xml"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var lang = LangList.ContainsValue(fileName)
                ? LangList.First(x => x.Value == fileName).Key
                : (MiraLanguage)(-1);
            if ((int)lang == -1) continue;

            try
            {
                Locale.TryAdd(lang, []);
                var xmlContent = File.ReadAllText(filePath);
                ParseXmlFile(xmlContent, lang, false);
            }
            catch (Exception e)
            {
                Error($"Failed to load external translation {filePath} for mod {modGuid}: {e.Message}");
            }
        }
    }

    private static void LoadInternalStrings(Assembly assembly, string internalName, string modGuid)
    {
        var resourcePrefix = $"{internalName}.Resources.Locale.";

        RegisteredModIds.Add(modGuid);

        var atLeastOneLoaded = false;
        foreach (var locale in LangList)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream(resourcePrefix + locale.Value + ".xml");
            if (resourceStream == null)
            {
                // Silently skipped
                continue;
            }

            using StreamReader reader = new(resourceStream);
            var xmlContent = reader.ReadToEnd();
            try
            {
                Locale.TryAdd(locale.Key, []);
                ParseXmlFile(xmlContent, locale.Key, true);
                atLeastOneLoaded = true;
            }
            catch (Exception e)
            {
                Error($"Failed to load translation {resourcePrefix}{locale.Value}: {e.Message}");
            }
        }

        if (!atLeastOneLoaded)
        {
            Error($"No internal strings were found for {internalName}!");
        }
    }

    private static void ParseXmlFile(string xmlContent, MiraLanguage language, bool loadingInternal)
    {
        var dict = Locale[language];
        XmlDocument xmlDoc = new();
        try
        {
            xmlDoc.LoadXml(xmlContent);
            var stringNodes = xmlDoc.SelectNodes("/resources/string");

            if (stringNodes == null)
            {
                return;
            }

            var total = 0;

            foreach (XmlNode node in stringNodes)
            {
                if (node.Attributes?["name"] == null)
                {
                    continue;
                }

                var name = node.Attributes["name"]!.Value;
                var value = node.InnerText;

                if (string.IsNullOrEmpty(name)) continue;

                if (value.Contains('['))
                {
                    value = value.Replace('[', '<');
                }

                if (value.Contains(']'))
                {
                    value = value.Replace(']', '>');
                }

                value = value.Replace("<nl>", "\n").Replace("<and>", "&");

                if (loadingInternal && Locale[language].ContainsKey(name))
                {
                    Error($"String for \"{name}\" in {language} was overwritten by duplicate!");
                }
                dict[name] = value;
                total++;
            }
            Info($"Loaded {language.ToDisplayString()} translation with ({total} keys)");
        }
        catch (XmlException ex)
        {
            Error($"XML parsing error: {ex.Message}");
        }
    }
}
