Shader "Magnet/HoneyBurst"
{
    Properties
    {
        _Tint ("Honey Tint", Color) = (0.95, 0.58, 0.08, 1)
        _Softness ("Edge Softness", Range(0.001, 0.2)) = 0.05
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.6
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
                p.y *= 0.88;
                float radius = length(p);
                float angle = atan2(p.y, p.x);
                float wobble = 1.0 + 0.10 * sin(angle * 5.0) + 0.05 * sin(angle * 9.0 + 0.8);
                float droplet = 1.0 - smoothstep(0.76, 0.76 + _Softness, radius * wobble);
                float core = 1.0 - smoothstep(0.0, 0.52, radius);
                float ring = smoothstep(0.38, 0.58, radius) * (1.0 - smoothstep(0.68, 0.86, radius * wobble));
                float spec = pow(saturate(1.0 - length(p - float2(-0.30, 0.34)) * 2.15), 6.0);
                float spec2 = pow(saturate(1.0 - length(p - float2(0.22, -0.16)) * 3.4), 8.0) * 0.4;
                float rim = pow(saturate(radius), _RimPower) * droplet;
                float alpha = saturate(droplet * 0.9 + ring * 0.35 + spec) * i.color.a;
                float3 color = _Tint.rgb * i.color.rgb;
                float3 dark = color * 0.42;
                float3 lit = color * 1.38 + float3(0.16, 0.09, 0.01);
                color = lerp(dark, lit, saturate(core * 0.7 + rim + spec + spec2));
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
