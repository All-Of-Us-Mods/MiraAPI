using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using Reactor.Utilities;

namespace MiraAPI.Utilities;

internal sealed record GuideOptionEntry(string Title, string Value, string GroupName, string ModAbbreviation);

internal sealed class GuideOptionPage(AbstractOptionGroup group, string modAbbreviation, IReadOnlyList<GuideOptionEntry> entries)
{
    public AbstractOptionGroup Group { get; } = group;

    public string Title { get; } = group.GroupName.Translate();

    public string ModAbbreviation { get; } = modAbbreviation;

    public IReadOnlyList<GuideOptionEntry> Entries { get; } = entries;
}

internal static class GuideOptionPages
{
    private static readonly Regex TrailingZeroRegex = new(@"\.0+(?!\d)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    public static List<GuideOptionPage> BuildPages(Func<bool, string> formatBool)
    {
        var pages = new List<GuideOptionPage>();
        var mode = CustomGameModeManager.ActiveMode;

        // Same plugin set the in-game options menu lists (skips mods that hide themselves from it).
        foreach (var plugin in MiraPluginManager.Instance.PluginsWithOptionsOrGameModes
                     .OrderBy(p => p.MiraPlugin.GetAbbreviatedModName(), StringComparer.OrdinalIgnoreCase))
        {
            var modAbbreviation = plugin.MiraPlugin.GetAbbreviatedModName();

            var groups = plugin.InternalOptionGroups
                .Where(g => IsGeneralGroup(g, mode) && SafeGroupVisible(g))
                .OrderBy(g => g.GroupPriority)
                .ThenBy(g => g.GroupName, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                var groupName = group.GroupName.Translate();
                var entries = new List<GuideOptionEntry>();

                foreach (var option in group.Options)
                {
                    if (!SafeOptionVisible(option))
                    {
                        continue;
                    }

                    var entry = TryCreateEntry(option, groupName, modAbbreviation, formatBool);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }

                if (entries.Count > 0)
                {
                    pages.Add(new GuideOptionPage(group, modAbbreviation, entries));
                }
            }
        }

        return pages;
    }
    public static List<GuideOptionEntry> Search(IEnumerable<GuideOptionPage> pages, string query)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return [];
        }

        const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
        return pages
            .SelectMany(p => p.Entries)
            .Select(e => (Entry: e, Rank: Rank(e, query, cmp)))
            .Where(x => x.Rank < int.MaxValue)
            .OrderBy(x => x.Rank)
            .Select(x => x.Entry)
            .ToList();
    }

    private static int Rank(GuideOptionEntry entry, string query, StringComparison cmp)
    {
        if (entry.Title.Equals(query, cmp))
        {
            return 0;
        }

        if (entry.Title.Contains(query, cmp))
        {
            return 1;
        }

        if (entry.GroupName.Contains(query, cmp) || entry.ModAbbreviation.Contains(query, cmp))
        {
            return 2;
        }

        return entry.Value.Contains(query, cmp) ? 3 : int.MaxValue;
    }
    private static bool IsGeneralGroup(AbstractOptionGroup group, AbstractGameMode? mode)
    {
        if (group.ParentMenu is not (MenuCategory.Game or MenuCategory.CustomOne or MenuCategory.CustomTwo))
        {
            return false;
        }

        if (group.OptionableType == null)
        {
            return true;
        }

        return mode != null && typeof(AbstractGameMode).IsAssignableFrom(group.OptionableType) &&
               mode.GetType().IsAssignableTo(group.OptionableType);
    }

    private static bool SafeGroupVisible(AbstractOptionGroup group)
    {
        try
        {
            return group.GroupVisible();
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Warning($"GroupVisible threw for {group.GetType().Name}: {e.Message}");
            return false;
        }
    }

    private static bool SafeOptionVisible(IModdedOption option)
    {
        try
        {
            return option.Visible();
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Warning($"Visible threw for option '{option.Title}': {e.Message}");
            return false;
        }
    }

    private static GuideOptionEntry? TryCreateEntry(
        IModdedOption option,
        string groupName,
        string modAbbreviation,
        Func<bool, string> formatBool)
    {
        try
        {
            var title = TranslationController.Instance.GetString(option.StringName);
            var value = FormatValue(option, formatBool);
            return new GuideOptionEntry(title, value, groupName, modAbbreviation);
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Warning($"Could not format option '{option.Title}' for the guide: {e.Message}");
            return null;
        }
    }

    private static string FormatValue(IModdedOption option, Func<bool, string> formatBool)
    {
        switch (option)
        {
            case ModdedToggleOption toggle:
                return formatBool(toggle.Value);

            case ModdedEnumOption enumOption:
                return MiraLocaleManager.Get(
                    enumOption.Values[enumOption.Value],
                    enumOption.Values[enumOption.Value]);

            case ModdedNumberOption number:
                if (number is { NegativeWordValue: not "#", Value: -1 })
                {
                    return number.NegativeWordValue;
                }

                if (number is { ZeroWordValue: not "#", Value: 0 })
                {
                    return number.ZeroWordValue;
                }

                return TrailingZeroRegex.Replace(number.Data.GetValueString(number.Value), string.Empty);

            default:
                return option.Data.Type == global::OptionTypes.Checkbox
                    ? formatBool(Math.Abs(option.GetFloatData() - 1f) < 0.0001f)
                    : option.Data.GetValueString(option.GetFloatData());
        }
    }
}