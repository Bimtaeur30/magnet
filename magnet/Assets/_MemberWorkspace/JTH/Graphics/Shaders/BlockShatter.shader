Shader "Magnet/BlockShatter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _Shatter ("Shatter", Range(0, 1)) = 0
        [PerRendererData] _ShatterSeed ("Shatter Seed", Float) = 1
        [PerRendererData] _SpriteUVRect ("Sprite UV Rect", Vector) = (0, 0, 1, 1)
        [PerRendererData] _WaterWobble ("Water Balloon Wobble", Range(0, 1)) = 0
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _CellCount ("Cell Count", Float) = 3
        _CrackWidth ("Crack Width", Range(0.001, 0.12)) = 0.038
        _CrackFill ("Crack Fill", Range(0, 1)) = 0.12
        _Separate ("Separate", Range(0, 0.2)) = 0.07
        _Crush ("Crush", Range(0, 0.2)) = 0.04
        _PopOut ("Pop Out", Range(0, 0.3)) = 0.1
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Name "BlockShatter"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _CellCount;
                float _CrackWidth;
                float _CrackFill;
                float _Separate;
                float _Crush;
                float _PopOut;
            CBUFFER_END

            float _Shatter;
            float _ShatterSeed;
            float4 _SpriteUVRect;
            float _WaterWobble;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 Hash22(float2 p)
            {
                p += float2(_ShatterSeed * 19.17, _ShatterSeed * 7.31);
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            void Voronoi(float2 uv, float density, out float2 center, out float2 id, out float edge)
            {
                float n = max(density, 1.0);
                float2 p = uv * n;
                float2 grid = floor(p);
                float2 f = frac(p);

                float minDist = 8.0;
                float2 minOffset = 0.0;
                float2 minGrid = 0.0;
                float2 minRand = 0.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 cell = float2((float)x, (float)y);
                        float2 rand = Hash22(grid + cell);
                        float2 offset = cell + rand - f;
                        float dist = dot(offset, offset);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minOffset = offset;
                            minGrid = cell;
                            minRand = rand;
                        }
                    }
                }

                float minEdge = 8.0;
                [unroll]
                for (int y2 = -1; y2 <= 1; y2++)
                {
                    [unroll]
                    for (int x2 = -1; x2 <= 1; x2++)
                    {
                        float2 cell = float2((float)x2, (float)y2);
                        float2 rand = Hash22(grid + cell);
                        float2 offset = cell + rand - f;
                        float2 between = offset - minOffset;
                        float betweenLen = length(between);
                        if (betweenLen > 0.0001)
                        {
                            minEdge = min(minEdge, dot(0.5 * (minOffset + offset), between / betweenLen));
                        }
                    }
                }

                id = grid + minGrid;
                center = (grid + minGrid + minRand) / n;
                edge = minEdge / n;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float pop = saturate(_Shatter) * _PopOut;
                float3 positionOS = input.positionOS.xyz;
                positionOS.xy *= 1.0 + pop;

                float2 rectSize = max(_SpriteUVRect.zw, float2(1e-5, 1e-5));
                float2 uv01 = (input.uv - _SpriteUVRect.xy) / rectSize;
                float2 centered = uv01 - 0.5;
                float phase = _Time.y * 11.0 + _ShatterSeed * 1.73;
                float bulge = sin(phase + centered.y * 5.2) * (1.0 - saturate(abs(centered.y) * 1.7));
                float sway = sin(phase * 0.71 + centered.y * 3.1);
                positionOS.x += (_WaterWobble * 0.045) * (bulge + sway * centered.y);
                positionOS.y += (_WaterWobble * 0.025) * sin(phase * 1.17 + centered.x * 5.7);
                positionOS.x *= 1.0 + _WaterWobble * 0.035 * sin(phase);
                positionOS.y *= 1.0 - _WaterWobble * 0.025 * sin(phase);
                uv01 = (uv01 - 0.5) * (1.0 + pop) + 0.5;

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = _SpriteUVRect.xy + uv01 * rectSize;
                output.color = input.color * _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 rectSize = max(_SpriteUVRect.zw, float2(1e-5, 1e-5));
                float2 uv01 = (input.uv - _SpriteUVRect.xy) / rectSize;
                float shatter = saturate(_Shatter);

                float2 sampleUV = input.uv;
                float3 pieceTint = 1.0;
                float3 crackTint = 1.0;
                float inCrack = 0.0;

                if (shatter > 0.001)
                {
                    float2 coarseCenter;
                    float2 coarseId;
                    float coarseEdge;
                    Voronoi(uv01, max(_CellCount, 1.0), coarseCenter, coarseId, coarseEdge);

                    float2 fillCenter;
                    float2 fillId;
                    float fillEdge;
                    Voronoi(uv01 + 0.19, max(_CellCount, 1.0) * 2.5, fillCenter, fillId, fillEdge);

                    float distCenter = length(uv01 - 0.5);
                    float centerWeight = saturate(1.0 - distCenter * 2.15);
                    float2 widthHash = Hash22(coarseId + 4.17);
                    float baseWidth = lerp(0.28, 1.85, pow(widthHash.x, 1.35));
                    float wobble = 0.55 + 0.45 * sin(dot(uv01, float2(19.3, 14.1) + widthHash * 9.0));
                    float gap = shatter * _CrackWidth * baseWidth * wobble;
                    inCrack = step(coarseEdge, gap);

                    float2 fromCenter = coarseCenter - 0.5;
                    float2 radial = fromCenter / max(length(fromCenter), 1e-5);
                    float2 jitter = Hash22(coarseId) * 2.0 - 1.0;
                    float2 moveDir = normalize(radial * 0.55 + jitter * 0.6);
                    float popVar = lerp(0.55, 1.65, Hash22(coarseId + 2.71).x);
                    float separate = shatter * _Separate * popVar;
                    float crush = shatter * _Crush * centerWeight * 0.35;

                    float2 sampleUV01 = uv01 - moveDir * separate;
                    sampleUV01 = lerp(sampleUV01, 0.5, crush);
                    sampleUV = _SpriteUVRect.xy + sampleUV01 * rectSize;

                    float sampleInside =
                        step(0.0, sampleUV01.x) *
                        step(sampleUV01.x, 1.0) *
                        step(0.0, sampleUV01.y) *
                        step(sampleUV01.y, 1.0);
                    inCrack = max(inCrack, 1.0 - sampleInside);

                    float outsideSprite = max(
                        step(uv01.x, 0.0),
                        max(step(1.0, uv01.x), max(step(uv01.y, 0.0), step(1.0, uv01.y))));
                    clip(1.1 - inCrack * outsideSprite);

                    sampleUV = lerp(sampleUV, input.uv, inCrack * (1.0 - outsideSprite));

                    float2 tilt = Hash22(coarseId + 11.3) * 2.0 - 1.0;
                    float3 fakeNormal = normalize(float3(tilt.x * 0.85, tilt.y * 0.85, 0.45));
                    float3 lightDir = normalize(float3(-0.42, 0.78, 0.52));
                    float ndotl = saturate(dot(fakeNormal, lightDir));
                    float lit = lerp(0.48, 1.38, ndotl);
                    pieceTint = lerp(1.0, lit.xxx, shatter);

                    float2 fillChroma = Hash22(fillId + 31.7);
                    float2 fillChromaB = Hash22(fillId + 8.1);
                    float fillDark = lerp(_CrackFill * 0.35, _CrackFill * 0.95, Hash22(fillId + 5.2).x);
                    crackTint = lerp(
                        float3(0.55, 0.72, 1.18),
                        float3(1.22, 0.82, 0.58),
                        float3(fillChroma.x, fillChroma.y, fillChromaB.x));
                    crackTint *= fillDark;
                }

                half4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV);
                spriteColor *= input.color;
                spriteColor.rgb *= lerp(pieceTint, crackTint, inCrack);
                spriteColor.rgb *= spriteColor.a;
                clip(spriteColor.a - 0.001);
                return spriteColor;
            }
            ENDHLSL
        }
    }
}
