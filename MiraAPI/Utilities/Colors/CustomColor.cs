using UnityEngine;

namespace MiraAPI.Utilities;
public class CustomColor
{
    public CustomColor(Color32 mainColor, StringNames name)
    {
        MainColor = mainColor;
        ShadowColor = GetShadowColor(mainColor, 60);
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

    public static Color32 GetShadowColor(Color32 c, byte darknessAmount)
    {
        return
            new Color32((byte)Mathf.Clamp(c.r - darknessAmount, 0, 255), (byte)Mathf.Clamp(c.g - darknessAmount, 0, 255),
            (byte)Mathf.Clamp(c.b - darknessAmount, 0, 255), byte.MaxValue);
    }
}