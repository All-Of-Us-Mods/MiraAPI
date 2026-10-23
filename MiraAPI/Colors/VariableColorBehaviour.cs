using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Colors;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// The base class for variable color behaviour on <see cref="SpriteRenderer"/>s.
/// </summary>
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity methods.")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity fields.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
public abstract class BaseVariableColorBehaviour : MonoBehaviour
{
    /// <summary>
    /// The <see cref="VariableColor"/> to apply.
    /// </summary>
    protected VariableColor? color;

    /// <summary>
    /// Sets the current variable color.
    /// </summary>
    /// <param name="newColor">The new <see cref="VariableColor"/>.</param>
    [HideFromIl2Cpp]
    public void SetColor(VariableColor? newColor)
    {
        color = newColor;
    }
}

/// <summary>
/// A mono script that adjusts the material colors of a <see cref="Renderer"/> component that it's attached to based on the provided variable color.
/// </summary>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity methods.")]
public sealed class VariableColorBehaviour : BaseVariableColorBehaviour
{
    private Renderer? rend;

    private static readonly Color VisorColor = Palette.VisorColor;

    /// <summary>
    /// Sets the new <see cref="Renderer"/> target.
    /// </summary>
    /// <param name="newRend">The new <see cref="Renderer"/> that the script must target.</param>
    public void SetRend(Renderer? newRend)
    {
        rend = newRend;
    }

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (color == null || rend == null)
            return;

        var time = Time.time;
        var material = rend.material;

        material.SetColor(ShaderID.VisorColor, VisorColor);

        material.SetColor(ShaderID.BodyColor, color.EvaluateMainColor(time));
        material.SetColor(ShaderID.BackColor, color.EvaluateShadowColor(time));
    }
}

/// <summary>
/// A mono script that adjusts the graphic colors of a <see cref="SpriteRenderer"/> component that it's attached to based on the provided variable color.
/// </summary>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity methods.")]
public sealed class BasicVariableColorBehaviour : BaseVariableColorBehaviour
{
    private SpriteRenderer? rend;

    /// <summary>
    /// Sets the new <see cref="SpriteRenderer"/> target.
    /// </summary>
    /// <param name="newRend">The new <see cref="SpriteRenderer"/> that the script must target.</param>
    public void SetRend(SpriteRenderer? newRend)
    {
        rend = newRend;
    }

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (color != null && rend != null)
            rend.color = color.EvaluateMainColor(Time.time);
    }
}
