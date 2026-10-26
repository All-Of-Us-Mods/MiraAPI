using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Utilities;

internal sealed record GuideOptionEntry(
    string Title,
    string Value,
    AbstractOptionGroup Group,
    string GroupName,
    string ModAbbreviation,
    IReadOnlyList<string> ModSearchTerms);

internal sealed class GuideOptionPage(
    AbstractOptionGroup group,
    string title,
    int order,
    int menuOrder,
    uint groupPriority,
    int columns,
    string modAbbreviation,
    IReadOnlyList<string> modSearchTerms,
    List<GuideOptionEntry> entries)
{
    public AbstractOptionGroup Group { get; } = group;

    public List<AbstractOptionGroup> Groups { get; } = [group];

    public string Title { get; } = title;

    public LoadableAsset<Sprite> Sprite { get; } = group.Sprite;

    public int Order { get; set; } = order;

    public int MenuOrder { get; set; } = menuOrder;

    public uint GroupPriority { get; set; } = groupPriority;

    public int Columns { get; } = columns;

    public string ModAbbreviation { get; } = modAbbreviation;

    public HashSet<string> ModSearchTerms { get; } = new(modSearchTerms, StringComparer.OrdinalIgnoreCase);

    public List<GuideOptionEntry> Entries { get; } = entries;
}

internal static class GuideOptionPages
{
    private static readonly Regex TrailingZeroRegex = new(@"\.0+(?!\d)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public static List<GuideOptionPage> BuildPages(Func<bool, string> formatBool)
    {
        var pages = new List<GuideOptionPage>();
        var combinedPages = new Dictionary<string, GuideOptionPage>(StringComparer.OrdinalIgnoreCase);
        var mode = CustomGameModeManager.ActiveMode;

        var groups = MiraPluginManager.Instance.PluginsWithOptionsOrGameModes
            .SelectMany(plugin => plugin.InternalOptionGroups
                .Where(group => group.ShowInMatchInfoGuide && IsSettingsGroup(group, mode) && SafeGroupVisible(group))
                .Select(group => (
                    Plugin: plugin,
                    Group: group,
                    ModAbbreviation: plugin.MiraPlugin.GetAbbreviatedModName())))
            .OrderBy(item => GetMenuOrder(item.Group.ParentMenu))
            .ThenBy(item => item.Group.GroupPriority)
            .ThenBy(item => item.Group.GroupName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ModAbbreviation, StringComparer.OrdinalIgnoreCase);

        foreach (var item in groups)
        {
            var groupName = item.Group.GroupName.Translate();
            var modSearchTerms = GetModSearchTerms(item.Plugin, item.Group);
            var entries = new List<GuideOptionEntry>();

            foreach (var option in item.Group.Options)
            {
                if (!SafeOptionVisible(option))
                {
                    continue;
                }

                var entry = TryCreateEntry(
                    option,
                    item.Group,
                    groupName,
                    item.ModAbbreviation,
                    modSearchTerms,
                    formatBool);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            if (entries.Count <= 0)
            {
                continue;
            }

            var pageKey = item.Group.MatchInfoGuidePageKey?.Trim();
            var combinedKey = string.IsNullOrEmpty(pageKey) ? null : $"{item.Plugin.PluginId}\0{pageKey}";
            if (combinedKey != null && combinedPages.TryGetValue(combinedKey, out var combinedPage))
            {
                combinedPage.Groups.Add(item.Group);
                combinedPage.Entries.AddRange(entries);
                combinedPage.ModSearchTerms.UnionWith(modSearchTerms);
                combinedPage.Order = Math.Min(combinedPage.Order, item.Group.MatchInfoGuidePageOrder);
                combinedPage.MenuOrder = Math.Min(
                    combinedPage.MenuOrder,
                    GetMenuOrder(item.Group.ParentMenu));
                combinedPage.GroupPriority = Math.Min(
                    combinedPage.GroupPriority,
                    item.Group.GroupPriority);
            }
            else
            {
                var page = new GuideOptionPage(
                    item.Group,
                    item.Group.MatchInfoGuidePageTitle,
                    item.Group.MatchInfoGuidePageOrder,
                    GetMenuOrder(item.Group.ParentMenu),
                    item.Group.GroupPriority,
                    Mathf.Clamp(item.Group.MatchInfoGuidePageColumns, 1, 6),
                    item.ModAbbreviation,
                    modSearchTerms,
                    entries);
                pages.Add(page);
                if (combinedKey != null)
                {
                    combinedPages.Add(combinedKey, page);
                }
            }
        }

        return [.. pages
            .OrderBy(page => page.MenuOrder)
            .ThenBy(page => page.GroupPriority)
            .ThenBy(page => page.Order)
            .ThenBy(page => page.Group.GroupName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(page => page.ModAbbreviation, StringComparer.OrdinalIgnoreCase)];
    }

    private static int GetMenuOrder(MenuCategory menu)
    {
        return menu switch
        {
            MenuCategory.Game => 0,
            MenuCategory.CustomOne => 1,
            MenuCategory.CustomTwo => 2,
            _ => int.MaxValue,
        };
    }

    public static List<GuideOptionEntry> Search(IEnumerable<GuideOptionPage> pages, string query)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return [];
        }

        const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
        var pageList = pages.ToList();
        var normalizedQuery = NormalizeSearchText(query);
        var matchingModPages = normalizedQuery.Length < 3
            ? []
            : pageList
                .Where(page => page.ModSearchTerms.Any(term =>
                    NormalizeSearchText(term).Contains(normalizedQuery, StringComparison.Ordinal)))
                .ToArray();

        if (matchingModPages.Length > 0)
        {
            return [.. matchingModPages.SelectMany(page => page.Entries)];
        }

        return [.. pageList
            .SelectMany(page => page.Entries
                .Select(entry => (Entry: entry, Rank: Rank(entry, query, cmp)))
                .Where(result => result.Rank < int.MaxValue)
                .OrderBy(result => result.Rank)
                .Select(result => result.Entry))];
    }

    private static string[] GetModSearchTerms(MiraPluginInfo plugin, AbstractOptionGroup group)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            plugin.PluginId,
            plugin.PluginInfo.Metadata.Name,
            plugin.MiraPlugin.OptionsTitleText,
            plugin.MiraPlugin.ParentLocaleId,
            plugin.MiraPlugin.GetAbbreviatedModName(),
        };

        AddNamespace(terms, plugin.MiraPlugin.GetType());
        AddNamespace(terms, group.GetType());
        foreach (var option in group.Options)
        {
            AddNamespace(terms, option.GetType());
        }

        return [.. terms.Where(term => !string.IsNullOrWhiteSpace(term))];
    }

    private static void AddNamespace(HashSet<string> terms, Type type)
    {
        if (!string.IsNullOrWhiteSpace(type.Namespace))
        {
            terms.Add(type.Namespace);
        }
    }

    private static string NormalizeSearchText(string value)
    {
        return new string([.. value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant)]);
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

    private static bool IsSettingsGroup(AbstractOptionGroup group, AbstractGameMode? mode)
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
            Warning($"GroupVisible threw for {group.GetType().Name}: {e.Message}");
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
            Warning($"Visible threw for option '{option.Title}': {e.Message}");
            return false;
        }
    }

    private static GuideOptionEntry? TryCreateEntry(
        IModdedOption option,
        AbstractOptionGroup group,
        string groupName,
        string modAbbreviation,
        IReadOnlyList<string> modSearchTerms,
        Func<bool, string> formatBool)
    {
        try
        {
            var title = TranslationController.Instance.GetString(option.StringName);
            var value = FormatValue(option, formatBool);
            return new GuideOptionEntry(title, value, group, groupName, modAbbreviation, modSearchTerms);
        }
        catch (Exception e)
        {
            Warning($"Could not format option '{option.Title}' for the guide: {e.Message}");
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
                return option.Data.Type == OptionTypes.Checkbox
                    ? formatBool(Math.Abs(option.GetFloatData() - 1f) < 0.0001f)
                    : option.Data.GetValueString(option.GetFloatData());
        }
    }
}
