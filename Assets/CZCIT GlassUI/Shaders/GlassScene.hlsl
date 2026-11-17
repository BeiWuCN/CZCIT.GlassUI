UNITY_DECLARE_SCREENSPACE_TEXTURE(_CZCITGlassScene);
float4 _CZCITGlassScene_TexelSize;
float _Aspect, _Radius, _Blur, _Frost, _CZCITGlassSceneReady;
float _InteractionHighlight;
float _Selected, _Pressed, _Reveal, _ConfirmPulse;
float _MenuLighting, _RimStrength, _BackgroundLimit;
float _Transmission, _TintStrength, _SheenStrength;
struct A
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct V
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 screen : TEXCOORD1;
    float4 color : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO
};
V vert(A v)
{
    V o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(V, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.screen = ComputeNonStereoScreenPos(o.vertex);
    o.uv = v.uv;
    o.color = v.color;
    return o;
}
float3 sampleBackground(float2 uv)
{
    uv = UnityStereoTransformScreenSpaceTex(uv);
    float3 sampleColor = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CZCITGlassScene, uv).rgb;
    return sampleColor;
}
float4 frag(V i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    if (_Reveal < .999)
        clip(_Reveal - i.uv.x);
    float2 p = (i.uv - .5) * float2(_Aspect, 1);
    float radius = clamp(_Radius, 0, min(max(_Aspect, .001), 1) * .5);
    float2 q = abs(p) - (float2(_Aspect, 1) * .5 - radius);
    float d = length(max(q, 0)) + min(max(q.x, q.y), 0) - radius;
    float mask = 1 - smoothstep(-.003, .003, d);
    float rim = exp(-abs(d) * 110);
    float2 uv = i.screen.xy / i.screen.w;
    float2 bend = normalize(p + float2(.0001, 0)) * pow(saturate(1 + d * 9), 3) * .009;
    uv += bend;
    float3 color = 0;
    // Preserve the exhibit's existing 7x7 stereo background filter.
    float weights = 0;
    for (int x = -3; x <= 3; x++)
        for (int y = -3; y <= 3; y++)
        {
            float weight = exp(-.3 * (x * x + y * y));
            color +=
                sampleBackground(saturate(uv + float2(x, y) * _CZCITGlassScene_TexelSize.xy * _Blur * .3)) *
                weight;
            weights += weight;
        }
    color /= weights;
    color = lerp(float3(.025, .035, .06), color, saturate(_CZCITGlassSceneReady));
    float luminance = dot(color, float3(.2126, .7152, .0722));
    float3 controlled = color / (1 + luminance / max(_BackgroundLimit, .01));
    color = lerp(color, controlled, _MenuLighting);
    color = lerp(color * _Transmission, i.color.rgb * _TintStrength, _Frost);
    color += float3(.006, .012, .021) * _MenuLighting;
    float sheen = pow(saturate(1 - i.uv.y), 5) * .045 + pow(saturate(i.uv.y), 9) * .085;
    color += float3(.86, .94, 1) * sheen * lerp(1, .55, _MenuLighting) * _SheenStrength +
             rim * (.35 + .2 * i.uv.y) * _RimStrength;
    color += rim * float3(.1, .04, .012) * pow(1 - i.uv.y, 3) * _MenuLighting;
    float focus = saturate(_InteractionHighlight / .18);
    float focusRail = (1 - smoothstep(.012, .023, 1 - i.uv.y)) * step(i.uv.x, pow(focus, .65)) * focus;
    color +=
        float3(.10, .20, .21) * _InteractionHighlight + rim * float3(.34, .73, .76) * _InteractionHighlight;
    color += float3(.34, .73, .76) * focusRail * .35;
    float2 edgeUV = min(i.uv, 1 - i.uv);
    float brackets = (1 - smoothstep(.006, .012, edgeUV.x)) * (1 - smoothstep(.09, .14, edgeUV.y));
    brackets += (1 - smoothstep(.012, .022, edgeUV.y)) * (1 - smoothstep(.025, .045, edgeUV.x));
    color += float3(.68, .88, .9) * brackets * focus * .55;
    float scan = exp(-abs(i.uv.x - _Reveal) * 180) * step(.001, _Reveal) * (1 - step(.999, _Reveal));
    float confirm = exp(-abs(i.uv.x - (1 - _ConfirmPulse)) * 70) * _ConfirmPulse;
    color += float3(.63, .87, .89) * (scan * .3 + confirm * (.10 + rim * .45));
    float selectionRail = (1 - smoothstep(.009, .015, i.uv.x)) * smoothstep(.12, .18, i.uv.y) *
                          (1 - smoothstep(.82, .88, i.uv.y));
    color += float3(.34, .73, .76) * (_Selected * (selectionRail * .8 + rim * .25));
    color *= lerp(1, .78, _Pressed);
    return float4(color, mask * i.color.a);
}
