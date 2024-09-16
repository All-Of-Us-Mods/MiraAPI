namespace MiraAPI.Roles;

/// <summary>
/// The type of hint style for a role to use.
/// </summary>
public enum RoleHintType
{
    /// <summary>
    /// No hint.
    /// </summary>
    None,

    /// <summary>
    /// Original game style (hint above tasks).
    /// </summary>
    TaskHint,

    /// <summary>
    /// Use Mira's custom role tab.
    /// </summary>
    RoleTab,
}
