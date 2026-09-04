using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.LocalSettings;
using Reactor.Localization;
using Reactor.Localization.Providers;
using Reactor.Utilities;

namespace MiraAPI.Translation;

public class MiraLocalizationProvider : LocalizationProvider
{
    public override int Priority => ReactorPriority.Normal;
    private static LocalizationProvider? _reactorProvider;
    private static bool _loadedStrings;

    public override bool TryGetText(StringNames stringName, out string? result)
    {
        if (MiraLocaleManager.StringNamesLookup.TryGetValue(stringName, out var key))
        {
            result = MiraLocaleManager.Get(key);
            return true;
        }
        result = null;
        return false;
    }

    public override bool TryGetTextFormatted(StringNames stringName, Il2CppReferenceArray<Il2CppSystem.Object> parts, out string? result)
    {
        if (!TryGetText(stringName, out result)) return false;

        result = Il2CppSystem.String.Format(result, parts);
        return true;
    }

    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        _reactorProvider ??= LocalizationManager.Providers.First(x => x is HardCodedLocalizationProvider);

        if (MiraLocaleManager.LangCultureList.TryGetValue((MiraLanguage)newLanguage, out var culture))
        {
            MiraApiPlugin.Culture = new(culture);
        }
        if (!_loadedStrings)
        {
            MiraLocaleManager.LoadExternalLocale();
            _loadedStrings = true;
        }

        foreach (var tab in LocalSettingsTab.TabGroups)
        {
            if (tab.Key == null)
            {
                break;
            }

            tab.Key.text = $"<b>{LocalSettingsTab.GetShortName(tab.Value.Translate())}</b>";
        }
        /*Warning($"<?xml version='1.0' encoding='UTF-8'?>");
        Warning($"<resources>");
        foreach (var stringName in TranslationController.Instance.currentLanguage.AllStrings)
        {
            var value = stringName.Value.Replace("\n", "\\%nl\\%");
            value = value.Replace("{", "[");
            value = value.Replace("}", "]");
            Warning($"<string name=\"{stringName.Key}\">{value}</string>");
        }
        Warning($"</resources>");*/
    }
}
