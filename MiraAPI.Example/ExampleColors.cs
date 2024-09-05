using MiraAPI.Colors;
using UnityEngine;

namespace MiraAPI.Example;

[RegisterCustomColors]
public static class ExampleColors
{
    public static CustomColor Cerulean { get; } = new("Cerulean", new Color(0.0f, 0.48f, 0.65f));

    public static CustomColor Rose { get; } = new("Rose", new Color(0.98f, 0.26f, 0.62f));

    public static CustomColor Gold { get; } = new("Gold", new Color(1.0f, 0.84f, 0.0f));
}
