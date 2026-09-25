using System;
using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.Translation;

/// <summary>
/// A utility class that can build ids based on the provided id part.
/// </summary>
public class TranslationIdBuilder
{
    /// <summary>
    /// Gets an instance of <see cref="TranslationIdBuilder"/> that has default behaviour.
    /// </summary>
    public static TranslationIdBuilder Default { get; } = new();

    /// <summary>
    /// Constructs a translation id for a provided enum option value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The enum option value.</param>
    /// <returns>The constructed translation id.</returns>
    public virtual string CreateEnumOptionId<T>(T value)
        where T : Enum
    {
        return CreateEnumOptionId<T>(value.ToString());
    }

    /// <summary>
    /// Constructs a translation id for a provided enum option value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The enum option value.</param>
    /// <returns>The constructed translation id.</returns>
    public virtual string CreateEnumOptionId<T>(string value)
        where T : Enum
    {
        return CreateEnumOptionId(value, typeof(T));
    }

    /// <summary>
    /// Constructs a translation id for a provided enum option value.
    /// </summary>
    /// <param name="value">The enum option value.</param>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>The constructed translation id.</returns>
    public virtual string CreateEnumOptionId(object value, Type enumType)
    {
        return CreateEnumOptionId(value.ToString()!, enumType);
    }

    /// <summary>
    /// Constructs a translation id for a provided enum option value.
    /// </summary>
    /// <param name="value">The enum option value.</param>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>The constructed translation id.</returns>
    public virtual string CreateEnumOptionId(string value, Type enumType)
    {
        return value;
    }

    /// <summary>
    /// Constructs a translation id for a string option.
    /// </summary>
    /// <param name="option">The id part.</param>
    /// <returns>The constructed translation id.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Just confusing.")]
    public virtual string CreateStringOptionId(string option)
    {
        return option;
    }

    /// <summary>
    /// Constructs a translation id for an option's title.
    /// </summary>
    /// <param name="title">The id part.</param>
    /// <returns>The constructed translation id.</returns>
    public virtual string CreateOptionTitleId(string title)
    {
        return title;
    }
}
