namespace MiraAPI.Translation;

/// <summary>
/// Extension method for translating strings.
/// </summary>
public static class TranslationExtensions
{
    /// <summary>
    /// Translates a key to the current language.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <returns>The corresponding translation.</returns>
    public static string Translate(this string key)
    {
        return MiraLocaleManager.Get(key);
    }

    /// <summary>
    /// Translates a key to the current language.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="args">The values to format the string with.</param>
    /// <returns>The corresponding translation.</returns>
    public static string Translate(this string key, params object[] args)
    {
        return MiraLocaleManager.GetParsed(key, string.Empty, args);
    }
}
