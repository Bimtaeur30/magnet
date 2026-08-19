Shader "Magnet/PaperBurst"
{
    Properties
    {
        _Tint ("Paper Tint", Color) = (0.8, 0.7, 0.55, 1)
        _Softness ("Edge Softness", Range(0.001, 0.2)) = 0.055
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Tint;
            float _Softness;
            float _RimPower;

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                float2 folded = abs(p + float2(0.16 * p.y, 0));
                float paper = (1.0 - smoothstep(0.60, 0.60 + _Softness, folded.x)) *
                              (1.0 - smoothstep(0.82, 0.82 + _Softness, folded.y));
                float crease = smoothstep(0.015, 0.06, abs(p.x + p.y * 0.35));
                float rim = saturate(max(folded.x / .60, folded.y / .82)) * paper;
                float spec = (1.0 - crease) * .16;
                float alpha = paper * i.color.a;
                float3 color = _Tint.rgb * i.color.rgb;
                color = lerp(color * 0.72, color * 1.4 + 0.25, saturate(rim + spec));
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
