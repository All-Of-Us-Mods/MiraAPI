using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using MiraAPI.Translation;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to define a list of modded options.
/// </summary>
/// <param name="title">The title of the options.</param>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ModdedOptionListAttribute(string title) : PropertyOptionAttribute
{
    internal IBaseModdedOptionList? HolderOptionList { get; set; }
    internal IList? Value { get; set; }
    internal Type ValueType { get; set; }

    /// <summary>
    /// Gets the title of the options.
    /// If the string contains a format parameter, e.g., <c>{0}</c>, then the option's index will be given to it.
    /// The string cannot contain more than one parameter.
    /// If no parameter is given, the title will simply append the index to the end of it.
    /// </summary>
    public string Title => title;

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

    internal abstract IBaseModdedOptionList? CreateOptionList(IList value, PropertyInfo property);

    /// <summary>
    /// Gets the <see cref="Title"/> formatted with the option's index.
    /// </summary>
    /// <param name="index">The option's index.</param>
    /// <returns>The formatted option's title.</returns>
    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "I don't even know if this is worth it.")]
    protected string GetFormattedTitle(int index)
    {
        var match = Regex.Match(Title.Translate(), @"\{(\d+)(?::[^}]+)?\}", default, Regex.InfiniteMatchTimeout);

        if (!match.Success)
        {
            return Title + index;
        }

        if (match.Value == "{0}" && !match.NextMatch().Success)
        {
            return string.Format(Title, index);
        }

        Error($"ModdedOptionList Title of {Title} cannot contain more than the first parameter for formatting.");
        return Title + index;
    }
}
