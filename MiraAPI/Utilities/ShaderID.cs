using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.Utilities;

public static class ShaderID
{
    private static readonly Dictionary<string, int> Cache = new();
    
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
    
    // For player shader
    public static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
    public static readonly int BackColor = Shader.PropertyToID("_BackColor");
    public static readonly int VisorColor = Shader.PropertyToID("_VisorColor");
    
    // Main texture, very obviously used in any shader with a texture
    public static readonly int MainTex = Shader.PropertyToID("_MainTex");
    
    // Any masking stuff, like MeetingHud bubbles
    public static readonly int Mask = Shader.PropertyToID("_Mask");
    public static readonly int MaskComp = Shader.PropertyToID("_MaskComp");
    public static readonly int MaskLayer = Shader.PropertyToID("_MaskLayer");
    public static readonly int Stencil = Shader.PropertyToID("_Stencil");
    public static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
    
    // Used in many tasks
    public static readonly int Color = Shader.PropertyToID("_Color");
    
    // Has 2 uses in the game, provided for convenience
    public static readonly int Opacity = Shader.PropertyToID("_Opacity");
    
    // Used once in CooldownHelpers, once in PowerBarMining
    public static readonly int NormalizedUvs = Shader.PropertyToID("_NormalizedUvs");
    
    // Used in many consoles
    public static readonly int Outline = Shader.PropertyToID("_Outline");
    public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    
    // Used in some consoles
    public static readonly int AddColor = Shader.PropertyToID("_AddColor");
    
    // Has some uses in various locations
    public static readonly int Percent = Shader.PropertyToID("_Percent");
    public static readonly int PercentY = Shader.PropertyToID("_PercentY");
    public static readonly int Desat = Shader.PropertyToID("_Desat");
    
    // Used in LightSource
    public static readonly int PlayerRadius = Shader.PropertyToID("_PlayerRadius");
    public static readonly int LightRadius = Shader.PropertyToID("_LightRadius");
    public static readonly int LightOffset = Shader.PropertyToID("_LightOffset");
    public static readonly int FlashlightSize = Shader.PropertyToID("_FlashlightSize");
    public static readonly int FlashlightAngle = Shader.PropertyToID("_FlashlightAngle");
    
    // Used once for LightSourceGpuRenderer
    public static readonly int DepthCompressionValue = Shader.PropertyToID("_DepthCompressionValue");
    
    // Used in ProgressTracker
    public static readonly int Buckets = Shader.PropertyToID("_Buckets");
    public static readonly int FullBuckets = Shader.PropertyToID("_FullBuckets");
    
    // Used once in IntroCutscene and EndGameManager
    public static readonly int Rad = Shader.PropertyToID("_Rad");
    
    // Used only once for Quick Chat
    public static readonly int FaceColor = Shader.PropertyToID("_FaceColor");
    
    // Used for NavigationMinigame
    public static readonly int CrossHair = Shader.PropertyToID("_CrossHair");
    public static readonly int CrossColor = Shader.PropertyToID("_CrossColor");
    
    // Used for both SurveillanceMinigame (Planet and Normal)
    public static readonly int Center = Shader.PropertyToID("_Center");
    public static readonly int Color2 = Shader.PropertyToID("_Color2");
    
    // Used in ReactorShipRoom
    public static readonly int Speed = Shader.PropertyToID("_Speed");
    
    // Used only once in CourseMinigame
    public static readonly int AltTex = Shader.PropertyToID("_AltTex");
    public static readonly int Perc = Shader.PropertyToID("_Perc");

    // Used once in TextMarquee
    public static readonly int VertexOffsetX = Shader.PropertyToID("_VertexOffsetX");
    public static readonly int VertexOffsetY = Shader.PropertyToID("_VertexOffsetY");
    
    // Used in VertLineBehaviour
    public static readonly int Fade = Shader.PropertyToID("_Fade");
    
}