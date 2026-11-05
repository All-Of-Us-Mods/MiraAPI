using System.Diagnostics.CodeAnalysis;
using MiraAPI.Patches.Options;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Utilities.Components;

/// <summary>
/// Updates the visuals of the <see cref="RolesSettingsMenu"/> in a fixed update loop.
/// </summary>
/// <param name="cppPtr">The C++ pointer to the object.</param>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
public sealed class RoleSettingsMenuUpdater(nint cppPtr) : MonoBehaviour(cppPtr)
{
    private RolesSettingsMenu menu;

    private void Awake()
    {
        menu = GetComponent<RolesSettingsMenu>();
    }

    private void FixedUpdate()
    {
        if (MenuState.Instance.CurrentModIdx == 0) return;
        if (menu == null || RoleSettingMenuPatches.CurrentRole == null || RoleSettingMenuPatches.CurrentRoleOptions == null) return;

        var hasImage = RoleSettingMenuPatches.CurrentRole.RoleScreenshot != null;
        var num = hasImage ? -0.872f : -1;
        foreach (var opt in RoleSettingMenuPatches.CurrentRoleOptions)
        {
            var behaviour = opt.OptionBehaviour;

            if (behaviour == null) continue;

            if (!opt.Visible())
            {
                behaviour.gameObject.SetActive(false);
                continue;
            }

            behaviour.transform.localPosition = new Vector3(hasImage ? 2.17f : 1.1f, num, -2f);
            behaviour.gameObject.SetActive(true);
            num += -0.45f;
        }

        menu.scrollBar.SetYBoundsMax(-num - 3);
    }
}
