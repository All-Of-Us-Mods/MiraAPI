using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to define a list of modded options.
/// </summary>
/// <param name="title">The title of the options.</param>
[AttributeUsage(AttributeTargets.Property)]
[SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")] // why so many warnings???
public abstract class ModdedOptionListAttribute(string? title = null) : PropertyOptionAttribute
{
    internal IModdedOptionList? HolderOptionList;
    internal IList? Value;
    internal Type ValueType;
    internal string[] Titles;

    private static readonly Regex FormatParams = new(@"\{(\d+)(?::[^}]+)?\}", default, Regex.InfiniteMatchTimeout);

    /// <summary>
    /// Gets or sets the title of the options.
    /// If the string contains a format parameter, e.g., <c>{0}</c>, then the option's index will be given to it.
    /// The string cannot contain more than one parameter.
    /// If no parameter is given, the title will simply append the index to the end of it.
    /// </summary>
    public string? Title { get; protected set; } = title;

    /// <summary>
    /// Sets the value of all the options.
    /// </summary>
    /// <param name="value">The new values as an object.</param>
    public override void SetValue(object value)
    {
        Value = (IList)value;

        if (Value.Count != HolderOptionList!.Count)
        {
            throw new InvalidOperationException($"Value set to option list cannot change the list's length.");
        }

        for (int i = 0; i < Value.Count; i++)
        {
            SetValue(i, Value[i]!);
        }
    }

    /// <summary>
    /// Sets the value of the specific option.
    /// </summary>
    /// <param name="idx">The option to set.</param>
    /// <param name="value">The new value as an object.</param>
    public abstract void SetValue(int idx, object value);

    /// <summary>
    /// Gets the value of all the options.
    /// </summary>
    /// <returns>The value of the options as an object.</returns>
    public override object GetValue()
    {
        return Value!;
    }

    /// <summary>
    /// Gets the value of the specific option.
    /// </summary>
    /// <param name="idx">The option to set.</param>
    /// <returns>The value of the option as an object.</returns>
    public abstract object GetValue(int idx);

    internal abstract IModdedOptionList? CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group);

    /// <summary>
    /// Gets the <see cref="Title"/> formatted with the option's index.
    /// </summary>
    /// <param name="index">The option's index.</param>
    /// <param name="property">The property info for fallback title.</param>
    /// <returns>The formatted option's title.</returns>
    protected string GetFormattedTitle(int index, PropertyInfo property)
    {
        var baseTitle = Title ?? property.Name;
        return Titles[index] = GetFormattedTitle(baseTitle, index, FormatParams.Match(baseTitle));
    }

    private static string GetFormattedTitle(string baseTitle, int index, Match match)
    {
        if (!match.Success)
        {
            return baseTitle + index;
        }

        if (match.Value == "{0}" && !match.NextMatch().Success)
        {
            return string.Format(CultureInfo.InvariantCulture, baseTitle, index);
        }

        Error($"ModdedOptionList Title of {baseTitle} cannot contain more than the first parameter for formatting.");
        return baseTitle + index;
    }
}
