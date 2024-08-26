using UnityEngine;

namespace MiraAPI.Utilities.Colors;
public sealed class CustomColor
{
    public CustomColor(Color32 mainColor, StringNames name)
    {
        MainColor = mainColor;
        ShadowColor = mainColor.GetShadowColor(60);
        Name = name;
    }

    public CustomColor(Color32 mainColor, Color32 shadowColor, StringNames name)
    {
        MainColor = mainColor;
        ShadowColor = shadowColor;
        Name = name;
    }

    public Color32 MainColor { get; }
    public Color32 ShadowColor { get; }
    public StringNames Name { get; }
}