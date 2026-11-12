using System;
using System.Collections.Generic;

namespace MiraAPI.GameOptions;

/// <summary>
/// Allows an option group to compress noisy option blocks in the wiki "Options" section.
/// Implementers can hide specific option keys and replace them with one or more summary lines.
/// </summary>
[Obsolete("Use IWikiSummaryProvider instead, as it allows for multiple summarisations. This will be removed in future versions.")]
public interface ISummarizedOptions
{
    /// <summary>
    /// Gets the option string keys (StringNames) to omit from the wiki options list (e.g., a large set of related options).
    /// </summary>
    IReadOnlySet<StringNames> WikiHiddenOptionKeys { get; }

    /// <summary>
    /// Gets summary lines to insert when the first hidden option would have appeared.
    /// Lines should already be formatted as "Title: Value".
    /// </summary>
    /// <returns>The summarisation of the hidden options.</returns>
    IEnumerable<string> GetWikiOptionSummaryLines();
}
