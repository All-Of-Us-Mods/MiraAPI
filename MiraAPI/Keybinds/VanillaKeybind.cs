using System.Globalization;

namespace MiraAPI.Keybinds;

/// <summary>
/// Represents a vanilla keybind.
/// </summary>
public class VanillaKeybind : BaseKeybind
{
    /// <summary>
    /// Gets the <see cref="ActionButton"/> this keybind is bound to.
    /// </summary>
    public ActionButton? Button { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VanillaKeybind"/> class.
    /// </summary>
    /// <param name="id">The keybind id.</param>
    public VanillaKeybind(int id)
    {
        RewiredInputAction = KeybindUtils.GetInputActionById(id);
        Id = RewiredInputAction != null ? RewiredInputAction.name : id.ToString(CultureInfo.InvariantCulture);
    }
}
