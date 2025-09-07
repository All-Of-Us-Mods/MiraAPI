using BepInEx.Configuration;

namespace MiraAPI;

/// <summary>
/// Mira API Config File Handler
/// </summary>
public class MiraApiConfig(ConfigFile config)
{
    /// <summary>
    /// Gets whether the modifiers hud should be on the left side of the screen (under roles/task tab). Recommended for streamers.
    /// </summary>
    public ConfigEntry<bool> ModifiersHudLeftSide { get; private set; } = config.Bind("Displays", "Show Modifiers HUD on Left Side", false);

    /// <summary>
    /// Gets whether the keybind icons should be visible.
    /// </summary>
    public ConfigEntry<bool> ShowKeybinds { get; private set; } = config.Bind("Displays", "Show Keybinds on buttons", true);
}
