Shader "Magnet/PTY/HologramGlitchBurst"
{
    Properties
    {
        _Tint ("Hologram Tint", Color) = (0.25, 0.9, 1, 1)
        _GlitchStrength ("Glitch Strength", Range(0, 1)) = 0.32
        _ScanlineDensity ("Scanline Density", Range(4, 32)) = 18
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
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
            float _GlitchStrength;
            float _ScanlineDensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                float scanlineIndex = floor((p.y + 1.0) * _ScanlineDensity);
                float jitter = frac(sin(scanlineIndex * 73.13 + _Time.y * 11.7) * 43758.55) - 0.5;
                p.x += jitter * _GlitchStrength * 0.22;

                float2 grid = abs(frac((p + 1.0) * float2(4.0, 6.0)) - 0.5);
                float pixel = 1.0 - step(0.42, max(grid.x, grid.y));
                float rectangle = 1.0 - smoothstep(0.72, 0.77, max(abs(p.x), abs(p.y)));
                float scanline = 0.58 + 0.42 * step(0.55, frac((p.y + 1.0) * _ScanlineDensity));
                float flicker = 0.78 + 0.22 * sin(_Time.y * 17.0 + scanlineIndex * 2.4);
                float shape = rectangle * pixel * scanline;

                float edge = saturate(max(abs(p.x), abs(p.y)) * 1.45 - 0.38) * shape;
                float3 cyan = _Tint.rgb * (0.7 + scanline * 0.6);
                float3 chromaticShift = float3(0.16, 0.0, 0.22) * edge;
                float3 color = cyan + chromaticShift;
                return half4(color * flicker, shape * input.color.a * flicker);
            }
            ENDHLSL
        }
    }
}
