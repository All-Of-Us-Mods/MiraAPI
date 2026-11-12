using System;
using System.Collections.Generic;

namespace MiraAPI.GameOptions;

/// <summary>
/// A holder option for a summary and option string names that are hidden behind it.
/// </summary>
/// <param name="hiddenKeys">The <see cref="StringNames"/> of the options being hidden.</param>
/// <param name="summaryLines">A delegate that produces the summary to replace the default text of the options being hidden.</param>
public sealed class WikiSummaryGroup(IReadOnlySet<StringNames>? hiddenKeys, Func<IEnumerable<string>>? summaryLines)
{
    /// <summary>
    /// Gets the option string keys (StringNames) to omit from the wiki options list (e.g., a large set of related options).
    /// </summary>
    public IReadOnlySet<StringNames> HiddenKeys { get; } = hiddenKeys ?? new HashSet<StringNames>(0);

    /// <summary>
    /// Gets summary lines to insert when the first hidden option would have appeared.
    /// Lines should already be formatted as "Title: Value".
    /// </summary>
    public Func<IEnumerable<string>> SummaryLines { get; } = summaryLines ?? Array.Empty<string>;
}

/// <summary>
/// Allows an option group to compress and format blocks or noisy options in the wiki "Options" section.
/// </summary>
public interface IWikiSummaryProvider
{
    /// <summary>
    /// Gets a dictionary where the key is the target option's <see cref="StringNames"/> to insert the
    /// summary at, and the value is the specific group of options being replaced by a summary.
    /// </summary>
    IReadOnlyDictionary<StringNames, WikiSummaryGroup> SummaryGroups { get; }
}
