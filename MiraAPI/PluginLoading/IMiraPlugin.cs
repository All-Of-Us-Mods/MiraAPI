using System.Linq;
using BepInEx.Configuration;

namespace MiraAPI.PluginLoading;

/// <summary>
/// The interface that all Mira plugins must implement.
/// </summary>
public interface IMiraPlugin
{
    /// <summary>
    /// Gets a value indicating whether to display the plugin in the options menu.
    /// </summary>
    virtual bool DisplayOnOptionsMenu => true;

    /// <summary>
    /// Gets the name to display on the options menu.
    /// </summary>
    string OptionsTitleText { get; }

    /// <summary>
    /// Gets the abbreviated name to display for other mods to pick up.
    /// </summary>
    /// <returns>The <see cref="string"/> text for the plugin.</returns>
    virtual string GetAbbreviatedModName()
    {
        return new([.. OptionsTitleText.Where(c => !char.IsLower(c) && !char.IsWhiteSpace(c))]);
    }

    /// <summary>
    /// Gets the name for the first custom category in the game options menu, if any.
    /// </summary>
    virtual string CustomOptionMenuNameOne => "Custom Category 1";

    /// <summary>
    /// Gets the name for the second custom category in the game options menu, if any.
    /// </summary>
    virtual string CustomOptionMenuNameTwo => "Custom Category 2";

    /// <summary>
    /// Gets the description for the second custom category in the game options menu, if any.
    /// </summary>
    virtual string ModifierMenuDescription => "Configure modifiers and their settings here!";

    /// <summary>
    /// Gets the description for the first custom category in the game options menu, if any.
    /// </summary>
    virtual string CustomOptionMenuOneDescription => "Apply game settings for this mod!";

    /// <summary>
    /// Gets the description for the second custom category in the game options menu, if any.
    /// </summary>
    virtual string CustomOptionMenuTwoDescription => "Apply game settings for this mod!";

    /// <summary>
    /// Gets the <see cref="ConfigFile"/> for the plugin.
    /// </summary>
    /// <returns>The <see cref="ConfigFile"/> for the plugin.</returns>
    ConfigFile GetConfigFile();
}
