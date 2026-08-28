namespace MiraAPI.Translation;

/// <summary>
/// Extension method for translating strings.
/// </summary>
public static class TranslationExtensions
{
    /// <summary>
    /// Translates a key to the current language.
    /// </summary>
    public static string Translate(this string key)
    {
        return MiraLocaleManager.Get(key);
    }
}
