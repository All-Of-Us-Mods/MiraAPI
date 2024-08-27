using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;
public sealed class CustomColor(StringNames name, Color32 mainColor, Color32 shadowColor)
{
    public Color32 MainColor { get; } = mainColor;
    public Color32 ShadowColor { get; } = shadowColor;
    public StringNames Name { get; } = name;

    public CustomColor(StringNames name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    public CustomColor(string name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    public CustomColor(string name, Color32 mainColor, Color32 shadowColor) : this(CustomStringName.CreateAndRegister(name), mainColor, shadowColor)
    {
    }
}