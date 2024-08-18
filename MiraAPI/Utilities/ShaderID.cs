using UnityEngine;

namespace MiraAPI.Utilities;

public static class ShaderID
{
    // For player shader
    public static readonly int _BodyColor = Shader.PropertyToID("_BodyColor");
    public static readonly int _BackColor = Shader.PropertyToID("_BackColor");
    public static readonly int _VisorColor = Shader.PropertyToID("_VisorColor");
    
    // Main texture, very obviously used in any shader with a texture
    public static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    
    // Any masking stuff, like MeetingHud bubbles
    public static readonly int _Mask = Shader.PropertyToID("_Mask");
    public static readonly int _MaskComp = Shader.PropertyToID("_MaskComp");
    public static readonly int _MaskLayer = Shader.PropertyToID("_MaskLayer");
    public static readonly int _Stencil = Shader.PropertyToID("_Stencil");
    public static readonly int _StencilComp = Shader.PropertyToID("_StencilComp");
    
    // Used in many tasks
    public static readonly int _Color = Shader.PropertyToID("_Color");
    
    // Has 2 uses in the game, provided for convenience
    public static readonly int _Opacity = Shader.PropertyToID("_Opacity");
    
    // Used once in CooldownHelpers, once in PowerBarMining
    public static readonly int _NormalizedUvs = Shader.PropertyToID("_NormalizedUvs");
    
    // Used in many consoles
    public static readonly int _Outline = Shader.PropertyToID("_Outline");
    public static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");
    
    // Used in some consoles
    public static readonly int _AddColor = Shader.PropertyToID("_AddColor");
    
    // Has some uses in various locations
    public static readonly int _Percent = Shader.PropertyToID("_Percent");
    public static readonly int _PercentY = Shader.PropertyToID("_PercentY");
    public static readonly int _Desat = Shader.PropertyToID("_Desat");
    
    // Used in LightSource
    public static readonly int _PlayerRadius = Shader.PropertyToID("_PlayerRadius");
    public static readonly int _LightRadius = Shader.PropertyToID("_LightRadius");
    public static readonly int _LightOffset = Shader.PropertyToID("_LightOffset");
    public static readonly int _FlashlightSize = Shader.PropertyToID("_FlashlightSize");
    public static readonly int _FlashlightAngle = Shader.PropertyToID("_FlashlightAngle");
    
    // Used once for LightSourceGpuRenderer
    public static readonly int _DepthCompressionValue = Shader.PropertyToID("_DepthCompressionValue");
    
    // Used in ProgressTracker
    public static readonly int _Buckets = Shader.PropertyToID("_Buckets");
    public static readonly int _FullBuckets = Shader.PropertyToID("_FullBuckets");
    
    // Used once in IntroCutscene and EndGameManager
    public static readonly int _Rad = Shader.PropertyToID("_Rad");
    
    // Used only once for Quick Chat
    public static readonly int _FaceColor = Shader.PropertyToID("_FaceColor");
    
    // Used for NavigationMinigame
    public static readonly int _CrossHair = Shader.PropertyToID("_CrossHair");
    public static readonly int _CrossColor = Shader.PropertyToID("_CrossColor");
    
    // Used for both SurveillanceMinigame (Planet and Normal)
    public static readonly int _Center = Shader.PropertyToID("_Center");
    public static readonly int _Color2 = Shader.PropertyToID("_Color2");
    
    // Used in ReactorShipRoom
    public static readonly int _Speed = Shader.PropertyToID("_Speed");
    
    // Used only once in CourseMinigame
    public static readonly int _AltTex = Shader.PropertyToID("_AltTex");
    public static readonly int _Perc = Shader.PropertyToID("_Perc");

    // Used once in TextMarquee
    public static readonly int _VertexOffsetX = Shader.PropertyToID("_VertexOffsetX");
    public static readonly int _VertexOffsetY = Shader.PropertyToID("_VertexOffsetY");
    
    // Used in VertLineBehaviour
    public static readonly int _Fade = Shader.PropertyToID("_Fade");
    
}