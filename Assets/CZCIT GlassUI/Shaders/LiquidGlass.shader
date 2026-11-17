// 共享玻璃材质，结合背景采样、边缘和颜色参数构成面板外观。
Shader "CZCIT/GlassUI/Stereo Glass"
{
    Properties
    {
        [PerRendererData] _MainTex("UI texture",2D)="white"{}
        _Aspect("Panel aspect",Float)=4
        _Radius("Corner radius",Float)=.14
        _Blur("Blur radius",Float)=3
        _Frost("Frost",Range(0,1))=.12
        _InteractionHighlight("Interaction highlight",Float)=0
        _Reveal("Graphic reveal",Range(0,1))=1
        _ConfirmPulse("Accepted click pulse",Range(0,1))=0
        _Selected("Selected state",Range(0,1))=0
        _Pressed("Pressed state",Range(0,1))=0
        _MenuLighting("Cinematic menu lighting",Range(0,1))=0
        _RimStrength("Rim strength",Range(0,2))=1
        _BackgroundLimit("Transmitted highlight limit",Range(.01,1))=.12
        _Transmission("Glass transmission",Range(0,1))=.42
        _TintStrength("Tint strength",Range(0,1))=.3
        _SheenStrength("Surface sheen",Range(0,1))=1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "LiquidGlass.hlsl"
            ENDCG
        }
    }
}
