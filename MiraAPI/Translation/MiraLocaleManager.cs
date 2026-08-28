using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraAPI.Translation;

public static class MiraLocaleManager
{
    private const string LangDirectory = "mira_languages";

    public static readonly Dictionary<string, Dictionary<MiraLanguage, Dictionary<string, string>>> Locale = [];

    public static readonly Dictionary<string, StringNames> RegisteredStringNames = [];
    internal static readonly Dictionary<StringNames, string> StringNamesLookup = [];

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
    /// Registers translations for a mod. Copies embedded XML to mira_languages/{modGuid}/ on first run,
    /// then loads all XML files from that directory into memory.
    /// Call once per mod during plugin Load().
    /// </summary>
    /// <param name="modGuid">The mod GUID (e.g., "mira.example").</param>
    public static void Register(string modGuid)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var dir = GetModLangDir(modGuid);

        LoadInternalStrings(callingAssembly, callingAssembly.GetName().Name!, modGuid, dir);
    }

    /// <summary>
    /// Registers translations for a mod. Copies embedded XML to mira_languages/{modGuid}/ on first run,
    /// then loads all XML files from that directory into memory.
    /// Call once per mod during plugin Load().
    /// </summary>
    /// <param name="modGuid">The mod GUID (e.g., "mira.example").</param>
    /// <param name="internalName">The mod's root namespace (e.g., "mira.api").</param>
    public static void Register(string modGuid, string internalName)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var dir = GetModLangDir(modGuid);

        LoadInternalStrings(callingAssembly, internalName, modGuid, dir);
    }

    /// <summary>
    /// Translates a key into the current language.
    /// Searches all mods, with reverse lookup fallback.
    /// </summary>
    /// <param name="key">The string id to find.</param>
    /// <param name="fallback">Fallback string to use if no translation is found.</param>
    /// <returns>A <see cref="string"/> based on the key provided.</returns>
    public static string Get(string key, string fallback = "")
    {
        var currentLang = CurrentLanguage;

        foreach (var (_, modLocale) in Locale)
        {
            if (modLocale.TryGetValue(currentLang, out var langDict) &&
                langDict.TryGetValue(key, out var translated))
            {
                return translated;
            }
        }

        foreach (var (_, modLocale) in Locale)
        {
            if (modLocale.TryGetValue(MiraLanguage.English, out var engDict) &&
                engDict.TryGetValue(key, out var english))
            {
                return english;
            }
        }

        string? bestStructuredKey = null;
        foreach (var (_, modLocale) in Locale)
        {
            if (modLocale.TryGetValue(MiraLanguage.English, out var engDict))
            {
                foreach (var (structuredKey, englishValue) in engDict)
                {
                    if (englishValue == key)
                    {
                        bestStructuredKey = structuredKey;
                        break;
                    }
                }
            }

            if (bestStructuredKey != null) break;
        }

        if (bestStructuredKey != null)
        {
            foreach (var (_, modLocale) in Locale)
            {
                if (modLocale.TryGetValue(currentLang, out var langDict) &&
                    langDict.TryGetValue(bestStructuredKey, out var revTranslated))
                {
                    return revTranslated;
                }
            }

            foreach (var (_, modLocale) in Locale)
            {
                if (modLocale.TryGetValue(MiraLanguage.English, out var engDict) &&
                    engDict.TryGetValue(bestStructuredKey, out var revEnglish))
                {
                    return revEnglish;
                }
            }
        }

        return fallback != string.Empty ? fallback : key;
    }

    public static string BuildTranslationId(string modId, string idPart, string suffix)
    {
        return idPart.StartsWith('#') ? idPart[1..] : $"{modId}.{idPart}.{suffix}";
    }

    public static string BuildTranslationId(string modId, string idPart)
    {
        return idPart.StartsWith('#') ? idPart[1..] : $"{modId}.{idPart}";
    }

    private static string GetModLangDir(string modGuid)
    {
        return Path.Combine(Application.persistentDataPath, LangDirectory, modGuid);
    }

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

    private static void LoadInternalStrings(Assembly assembly, string internalName, string modGuid, string dir)
    {
        Directory.CreateDirectory(dir);

        if (!Locale.ContainsKey(modGuid))
        {
            Locale[modGuid] = [];
        }

        var resourcePrefix = $"{internalName}.Resources.Locale.";

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
            string xmlContent = reader.ReadToEnd();
            try
            {
                var dict = ParseXmlFile(xmlContent);
                Locale[modGuid][locale.Key] = dict;
                Info($"Loaded {locale.Key.ToDisplayString()} translation for mod {modGuid} ({dict.Count} keys)");
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

        if (!Locale.TryGetValue(modGuid, out _))
        {
            Locale[modGuid] = [];
        }

        foreach (var filePath in Directory.GetFiles(dir, "*"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var lang = LangList.ContainsValue(fileName)
                ? LangList.First(x => x.Value == fileName).Key
                : (MiraLanguage)(-1);
            if ((int)lang == -1) continue;

            try
            {
                var dict = ParseXmlFile(filePath);
                Locale[modGuid][lang] = dict;
                Info($"Loaded external {lang.ToDisplayString()} translation for mod {modGuid} ({dict.Count} keys)");
            }
            catch (Exception e)
            {
                Error($"Failed to load external translation {filePath}: {e.Message}");
            }
        }
    }

    private static Dictionary<string, string> ParseXmlFile(string xmlContent)
    {
        var dict = new Dictionary<string, string>();
        XmlDocument xmlDoc = new();
        try
        {
            xmlDoc.LoadXml(xmlContent);
            XmlNodeList? stringNodes = xmlDoc.SelectNodes("/resources/string");

            if (stringNodes != null)
            {
                foreach (XmlNode node in stringNodes)
                {
                    if (node.Attributes?["name"] != null)
                    {
                        string name = node.Attributes["name"]!.Value;
                        string value = node.InnerText;

                        if (string.IsNullOrEmpty(name)) continue;

                        if (value.Contains('['))
                        {
                            value = value.Replace("[", "<");
                        }

                        if (value.Contains(']'))
                        {
                            value = value.Replace("]", ">");
                        }

                        value = value.Replace("<nl>", "\n").Replace("<and>", "&");

                        dict[name] = value;
                    }
                }
            }
        }
        catch (XmlException ex)
        {
            Error($"XML parsing error: {ex.Message}");
        }

        return dict;
    }
}
