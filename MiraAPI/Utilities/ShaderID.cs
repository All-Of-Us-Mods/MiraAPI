using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// Utility class for caching shader property IDs.
/// </summary>
public static class ShaderID
{
    private static readonly Dictionary<string, int> Cache = [];

    /// <summary>
    /// Retrieves the cached shader property ID for the specified name. If it is not cached, it registers and caches the ID before returning it.
    /// </summary>
    /// <param name="name">The name of the shader property to look up.</param>
    /// <returns>The integer ID of the shader property.</returns>
    public static int Get(string name)
    {
        if (Cache.TryGetValue(name, out var id))
        {
            return id;
        }

        id = Shader.PropertyToID(name);
        Cache[name] = id;
        return id;
    }

    // For player shaders

    /// <summary>
    /// The shader property ID for "_BodyColor".
    /// </summary>
    public static readonly int BodyColor = Get("_BodyColor");

    /// <summary>
    /// The shader property ID for "_BackColor".
    /// </summary>
    public static readonly int BackColor = Get("_BackColor");

    /// <summary>
    /// The shader property ID for "_VisorColor".
    /// </summary>
    public static readonly int VisorColor = Get("_VisorColor");

    // Main texture, very obviously used in any shader with a texture

    /// <summary>
    /// The shader property ID for "_MainTex".
    /// </summary>
    public static readonly int MainTex = Get("_MainTex");

    // Any masking stuff, like MeetingHud bubbles

    /// <summary>
    /// The shader property ID for "_Mask".
    /// </summary>
    public static readonly int Mask = Get("_Mask");

    /// <summary>
    /// The shader property ID for "_MaskComp".
    /// </summary>
    public static readonly int MaskComp = Get("_MaskComp");

    /// <summary>
    /// The shader property ID for "_MaskLayer".
    /// </summary>
    public static readonly int MaskLayer = Get("_MaskLayer");

    /// <summary>
    /// The shader property ID for "_Stencil".
    /// </summary>
    public static readonly int Stencil = Get("_Stencil");

    /// <summary>
    /// The shader property ID for "_StencilComp".
    /// </summary>
    public static readonly int StencilComp = Get("_StencilComp");

    // Used in many tasks

    /// <summary>
    /// The shader property ID for "_Color".
    /// </summary>
    public static readonly int Color = Get("_Color");

    // Has 2 uses in the game, provided for convenience

    /// <summary>
    /// The shader property ID for "_Opacity".
    /// </summary>
    public static readonly int Opacity = Get("_Opacity");

    // Used once in CooldownHelpers, once in PowerBarMining

    /// <summary>
    /// The shader property ID for "_NormalizedUvs".
    /// </summary>
    public static readonly int NormalizedUvs = Get("_NormalizedUvs");

    // Used in many consoles

    /// <summary>
    /// The shader property ID for "_Outline".
    /// </summary>
    public static readonly int Outline = Get("_Outline");

    /// <summary>
    /// The shader property ID for "_OutlineColor".
    /// </summary>
    public static readonly int OutlineColor = Get("_OutlineColor");

    // Used in some consoles

    /// <summary>
    /// The shader property ID for "_AddColor".
    /// </summary>
    public static readonly int AddColor = Get("_AddColor");

    // Has some uses in various locations

    /// <summary>
    /// The shader property ID for "_Percent".
    /// </summary>
    public static readonly int Percent = Get("_Percent");

    /// <summary>
    /// The shader property ID for "_PercentY".
    /// </summary>
    public static readonly int PercentY = Get("_PercentY");

    /// <summary>
    /// The shader property ID for "_Desat".
    /// </summary>
    public static readonly int Desat = Get("_Desat");

    // Used in LightSource

    /// <summary>
    /// The shader property ID for "_PlayerRadius".
    /// </summary>
    public static readonly int PlayerRadius = Get("_PlayerRadius");

    /// <summary>
    /// The shader property ID for "_LightRadius".
    /// </summary>
    public static readonly int LightRadius = Get("_LightRadius");

    /// <summary>
    /// The shader property ID for "_LightOffset".
    /// </summary>
    public static readonly int LightOffset = Get("_LightOffset");

    /// <summary>
    /// The shader property ID for "_FlashlightSize".
    /// </summary>
    public static readonly int FlashlightSize = Get("_FlashlightSize");

    /// <summary>
    /// The shader property ID for "_FlashlightAngle".
    /// </summary>
    public static readonly int FlashlightAngle = Get("_FlashlightAngle");

    // Used once for LightSourceGpuRenderer

    /// <summary>
    /// The shader property ID for "_DepthCompressionValue".
    /// </summary>
    public static readonly int DepthCompressionValue = Get("_DepthCompressionValue");

    // Used in ProgressTracker

    /// <summary>
    /// The shader property ID for "_Buckets".
    /// </summary>
    public static readonly int Buckets = Get("_Buckets");

    /// <summary>
    /// The shader property ID for "_FullBuckets".
    /// </summary>
    public static readonly int FullBuckets = Get("_FullBuckets");

    // Used once in IntroCutscene and EndGameManager

    /// <summary>
    /// The shader property ID for "_Rad".
    /// </summary>
    public static readonly int Rad = Get("_Rad");

    // Used only once for Quick Chat

    /// <summary>
    /// The shader property ID for "_FaceColor".
    /// </summary>
    public static readonly int FaceColor = Get("_FaceColor");

    // Used for NavigationMinigame

    /// <summary>
    /// The shader property ID for "_CrossHair".
    /// </summary>
    public static readonly int CrossHair = Get("_CrossHair");

    /// <summary>
    /// The shader property ID for "_CrossColor".
    /// </summary>
    public static readonly int CrossColor = Get("_CrossColor");

    // Used for both SurveillanceMinigame (Planet and Normal)

    /// <summary>
    /// The shader property ID for "_Center".
    /// </summary>
    public static readonly int Center = Get("_Center");

    /// <summary>
    /// The shader property ID for "_Color2".
    /// </summary>
    public static readonly int Color2 = Get("_Color2");

    // Used in ReactorShipRoom

    /// <summary>
    /// The shader property ID for "_Speed".
    /// </summary>
    public static readonly int Speed = Get("_Speed");

    // Used only once in CourseMinigame

    /// <summary>
    /// The shader property ID for "_AltTex".
    /// </summary>
    public static readonly int AltTex = Get("_AltTex");

    /// <summary>
    /// The shader property ID for "_Perc".
    /// </summary>
    public static readonly int Perc = Get("_Perc");

    // Used once in TextMarquee

    /// <summary>
    /// The shader property ID for "_VertexOffsetX".
    /// </summary>
    public static readonly int VertexOffsetX = Get("_VertexOffsetX");

    /// <summary>
    /// The shader property ID for "_VertexOffsetY".
    /// </summary>
    public static readonly int VertexOffsetY = Get("_VertexOffsetY");

    // Used in VertLineBehaviour

    /// <summary>
    /// The shader property ID for "_Fade".
    /// </summary>
    public static readonly int Fade = Get("_Fade");
}
