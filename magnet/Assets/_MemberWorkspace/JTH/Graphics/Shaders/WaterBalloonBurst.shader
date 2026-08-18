Shader "Magnet/WaterBalloonBurst"
{
    Properties
    {
        _Tint ("Water Tint", Color) = (0.2, 0.75, 1, 1)
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
                float radius = length(p);
                float droplet = 1.0 - smoothstep(0.82, 0.82 + _Softness, radius);
                float ring = smoothstep(0.52, 0.67, radius) * (1.0 - smoothstep(0.76, 0.91, radius));
                float spec = pow(saturate(1.0 - length(p - float2(-0.32, 0.34)) * 2.2), 5.0);
                float rim = pow(saturate(radius), _RimPower) * droplet;
                float alpha = saturate(droplet * 0.38 + ring * 0.72 + spec) * i.color.a;
                float3 color = _Tint.rgb * i.color.rgb;
                color = lerp(color * 0.72, color * 1.4 + 0.25, saturate(rim + spec));
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
