using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// A spinning ring used to indicate that content is loading.
/// </summary>
[RegisterInIl2Cpp]
public class LoadingRing(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private static Sprite? _ringSprite;

    /// <summary>
    /// Gets or sets the rotation speed in degrees per second.
    /// </summary>
    public float Speed { get; set; } = 270f;

    /// <summary>
    /// Creates a <see cref="LoadingRing"/> on the UI layer.
    /// </summary>
    /// <param name="parent">The transform to parent the spinner to.</param>
    /// <param name="localPosition">The local position of the spinner.</param>
    /// <param name="scale">The uniform scale of the spinner.</param>
    /// <returns>The created <see cref="LoadingRing"/>.</returns>
    public static LoadingRing Create(Transform parent, Vector3 localPosition, float scale)
    {
        _ringSprite ??= CreateRingSprite();
        var obj = new GameObject("LoadingRing")
        {
            transform =
            {
                parent = parent,
                localPosition = localPosition,
                localScale = Vector3.one * scale,
            },
            layer = LayerMask.NameToLayer("UI"),
        };
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = _ringSprite;
        return obj.AddComponent<LoadingRing>();
    }

    /// <summary>
    /// Rotates the ring.
    /// </summary>
    public void Update()
    {
        transform.Rotate(0f, 0f, -Speed * Time.deltaTime);
    }

    private static Sprite CreateRingSprite()
    {
        const int size = 128;
        const float innerRadius = 46f;
        const float outerRadius = 60f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x + 0.5f - (size / 2f);
                var dy = y + 0.5f - (size / 2f);
                var radius = Mathf.Sqrt((dx * dx) + (dy * dy));
                var ring = Mathf.Clamp01(radius - innerRadius) * Mathf.Clamp01(outerRadius - radius);
                var angle = (Mathf.Atan2(dy, dx) + Mathf.PI) / (2f * Mathf.PI);
                pixels[(y * size) + x] = new Color(1f, 1f, 1f, ring * Mathf.Lerp(1f, 0.1f, angle));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
