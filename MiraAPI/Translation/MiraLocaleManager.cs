using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using AmongUs.Data;
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
    private const string RefPrefix = "@ref:";
    private const int MaxDepth = 10;

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
        // Vanilla
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

        // Custom
        { MiraLanguage.Polish, "pl_PL" },
        { MiraLanguage.Turkish, "tr_TR" },
        { MiraLanguage.Swedish, "sv_SE" },
        { MiraLanguage.Lithuanian, "lt_LT" },
        { MiraLanguage.Czech, "cs_CZ" },
        { MiraLanguage.LChinese, "lzh_CN" },
        { MiraLanguage.Greek, "el_GR" },
        { MiraLanguage.Arabic, "arb" },
        { MiraLanguage.Hebrew, "heb" },
    };
    private static readonly Dictionary<string, MiraLanguage> ReverseLangLookup =
        LangList.ToDictionary(x => x.Value, x => x.Key);

    /// <summary>
    /// Gets a dictionary mapping <see cref="MiraLanguage"/> values to their standard language codes.
    /// </summary>
    public static Dictionary<MiraLanguage, string> LangCultureList { get; } = new()
    {
        // Vanilla
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

        // Custom
        { MiraLanguage.Polish, "pl-PL" },
        { MiraLanguage.Turkish, "tr-TR" },
        { MiraLanguage.Swedish, "sv-SE" },
        { MiraLanguage.Lithuanian, "lt-LT" },
        { MiraLanguage.Czech, "cs-CZ" },
        { MiraLanguage.LChinese, "lzh-CN" },
        { MiraLanguage.Greek, "el-GR" },
        { MiraLanguage.Arabic, "arb" },
        { MiraLanguage.Hebrew, "heb" },
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
                var langName = DataManager.Settings.Language.CurrentLanguage;
                return (MiraLanguage)langName;
            }
            catch
            {
                return MiraLanguage.English;
            }
        }
    }

    /// <inheritdoc cref="Register(string, string, Assembly)"/>
    [Obsolete("Pass typeof(YourPlugin).Assembly directly to Register for better performance.")]
    public static void Register(string modGuid)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        LoadInternalStrings(callingAssembly, callingAssembly.GetName().Name!, modGuid);
    }

    /// <inheritdoc cref="Register(string, string, Assembly)"/>
    public static void Register(string modGuid, Assembly assembly)
    {
        LoadInternalStrings(assembly, assembly.GetName().Name!, modGuid);
    }

    /// <inheritdoc cref="Register(string, string, Assembly)"/>
    [Obsolete("Pass typeof(YourPlugin).Assembly directly to Register for better performance.")]
    public static void Register(string modGuid, string internalName)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        LoadInternalStrings(callingAssembly, internalName, modGuid);
    }

    /// <summary>
    /// Registers translations for a mod.
    /// Call once per mod during plugin Load().
    /// </summary>
    /// <param name="modGuid">The mod GUID (e.g., "mira.example").</param>
    /// <param name="internalName">The mod's root namespace (e.g., "mira.api").</param>
    /// <param name="assembly">The <see cref="Assembly"/> to load the strings from.</param>
    public static void Register(string modGuid, string internalName, Assembly assembly)
    {
        LoadInternalStrings(assembly, internalName, modGuid);
    }

    /// <inheritdoc cref="Get(MiraLanguage, string, string)"/>
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

    /// <inheritdoc cref="GetParsed(MiraLanguage, string, Dictionary{string, string}, string)"/>
    public static string GetParsed(
        string key,
        Dictionary<string, string> parseList,
        string fallback = "")
    {
        return GetParsed(CurrentLanguage, key, parseList, fallback != string.Empty ? fallback : key);
    }

    /// <inheritdoc cref="GetParsed(MiraLanguage, string, ValueTuple{string, string}[], string)"/>
    public static string GetParsed(
        string key,
        (string, string)[] parseList,
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
        var text = Locale.GetValueOrDefault(language)?.GetValueOrDefault(key);

        if (language != MiraLanguage.English)
            text ??= Locale.GetValueOrDefault(MiraLanguage.English)?.GetValueOrDefault(key);

        text ??= fallback;

        foreach (var (toReplace, value) in parseList)
        {
            text = text.Replace(toReplace, value);
        }

        return text;
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
        (string, string)[] parseList,
        string fallback = "")
    {
        var text = Locale.GetValueOrDefault(language)?.GetValueOrDefault(key);

        if (language != MiraLanguage.English)
            text ??= Locale.GetValueOrDefault(MiraLanguage.English)?.GetValueOrDefault(key);

        text ??= fallback;

        foreach (var (toReplace, value) in parseList)
        {
            text = text.Replace(toReplace, value);
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

    /// <inheritdoc cref="BuildTranslationId(string, string, string)"/>
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

            if (!ReverseLangLookup.TryGetValue(fileName, out var lang))
            {
                continue;
            }

            try
            {
                Locale.TryAdd(lang, []);
                using var fileStream = File.OpenRead(filePath);
                ParseXmlFile(fileStream, lang, false);
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

        foreach (var (lang, name) in LangList)
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourcePrefix + name + ".xml");

            if (resourceStream == null)
            {
                // Silently skipped
                continue;
            }

            try
            {
                Locale.TryAdd(lang, []);
                ParseXmlFile(resourceStream, lang, true);
                atLeastOneLoaded = true;
            }
            catch (Exception e)
            {
                Error($"Failed to load translation {resourcePrefix}{name}: {e.Message}");
            }
        }

        if (!atLeastOneLoaded)
        {
            Error($"No internal strings were found for {internalName}!");
        }
    }

    /// <summary>
    /// Parses the provided xml file into translation keys and values and registers them to the locale manager.
    /// </summary>
    /// <param name="xmlContent">The xml file being parsed.</param>
    /// <param name="language">The language that the xml file corresponds to.</param>
    /// <param name="loadingInternal">Flag that indicates if the xml being parsed is internal or not.</param>
    public static void ParseXmlFile(string xmlContent, MiraLanguage language, bool loadingInternal)
    {
        using var stringReader = new StringReader(xmlContent);
        using var xmlReader = XmlReader.Create(stringReader);
        ParseXml(xmlReader, language, loadingInternal);
    }

    /// <summary>
    /// Parses the provided xml stream into translation keys and values and registers them to the locale manager.
    /// </summary>
    /// <param name="xmlStream">The stream to the xml content.</param>
    /// <param name="language">The language that the xml file corresponds to.</param>
    /// <param name="loadingInternal">Flag that indicates if the xml being parsed is internal or not.</param>
    public static void ParseXmlFile(Stream xmlStream, MiraLanguage language, bool loadingInternal)
    {
        using var reader = XmlReader.Create(xmlStream);
        ParseXml(reader, language, loadingInternal);
    }

    private static void ParseXml(XmlReader reader, MiraLanguage language, bool loadingInternal)
    {
        var dict = Locale[language];
        var total = 0;

        var pendingRefs = new Dictionary<string, string>();

        try
        {
            while (reader.ReadToFollowing("string"))
            {
                var name = reader.GetAttribute("name");

                if (string.IsNullOrEmpty(name))
                    continue;

                if (loadingInternal && dict.ContainsKey(name))
                {
                    Error($"String for \"{name}\" in {language} was overwritten by a duplicate!");
                }

                var value = reader.ReadElementContentAsString();

                if (value.StartsWith(RefPrefix, StringComparison.Ordinal))
                {
                    pendingRefs[name] = value[RefPrefix.Length..];
                }
                else
                {
                    value = value.Replace('[', '<')
                        .Replace(']', '>')
                        .Replace("<nl>", "\n")
                        .Replace("<and>", "&");

                    dict[name] = value;
                    total++;
                }
            }

            foreach (var unresolved in pendingRefs)
            {
                var originalKey = unresolved.Key;
                var currentTarget = unresolved.Value;
                var depth = 0;
                var resolved = false;

                while (depth < MaxDepth)
                {
                    if (dict.TryGetValue(currentTarget, out var resolvedValue))
                    {
                        dict[originalKey] = resolvedValue;
                        total++;
                        resolved = true;
                        break;
                    }

                    if (pendingRefs.TryGetValue(currentTarget, out var nextTarget))
                    {
                        currentTarget = nextTarget;
                        depth++;
                    }
                    else
                    {
                        Error($"Translation reference '{unresolved.Value}' for '{originalKey}' not found.");
                        break;
                    }
                }

                if (!resolved && depth >= MaxDepth)
                {
                    Error($"Circular translation reference or max depth exceeded for '{originalKey}'.");
                }
            }

            Info($"Loaded {language.ToDisplayString()} translation with ({total} keys)");
        }
        catch (XmlException ex)
        {
            Error($"XML parsing error: {ex.Message}");
        }
    }
}
