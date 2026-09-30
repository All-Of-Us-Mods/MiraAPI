using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Colors;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// A mono script that adjusts the material colors of a <see cref="SpriteRenderer"/> component that it's attached to based on the provided variable color.
/// </summary>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity methods.")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity fields.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
public sealed class VariableColorBehaviour : MonoBehaviour
{
    private SpriteRenderer rend;
    private VariableColor? color;

    private static readonly Color VisorColor = Palette.VisorColor;

    /// <summary>
    /// Sets the current variable color.
    /// </summary>
    /// <param name="col">The new variable color.</param>
    [HideFromIl2Cpp]
    public void SetColor(VariableColor? col)
    {
        color = col;
    }

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.material.SetColor(ShaderID.VisorColor, VisorColor);
    }

    private void Update()
    {
        if (!rend || color == null)
            return;

        var time = Time.time;
        var material = rend.material;
        material.SetColor(ShaderID.BodyColor, color.EvaluateMainColor(time));
        material.SetColor(ShaderID.BackColor, color.EvaluateShadowColor(time));
    }
}
