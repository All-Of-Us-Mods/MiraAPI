using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;
public sealed class CustomColor(Color32 mainColor, Color32 shadowColor, StringNames name)
{
    public Color32 MainColor { get; } = mainColor;
    public Color32 ShadowColor { get; } = shadowColor;
    public StringNames Name { get; } = name;

    public CustomColor(Color32 mainColor, StringNames name) : this(mainColor, mainColor.GetShadowColor(60), name)
    {
    }

    public CustomColor(Color32 mainColor, string name) : this(mainColor, mainColor.GetShadowColor(60), name)
    {
    }

    public CustomColor(Color32 mainColor, Color32 shadowColor, string name) : this(mainColor, shadowColor, CustomStringName.CreateAndRegister(name))
    {
    }
}