Shader "Magnet/EnemyBlink"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _BlinkColor ("Blink Color", Color) = (1, 1, 1, 1)
        _BlinkAmount ("Blink Amount", Range(0, 1)) = 0
        _PetrifyAmount ("Petrify Amount", Range(0, 1)) = 0
        _ShatterAmount ("Shatter Amount", Range(0, 1)) = 0
        _ShatterCellCount ("Shatter Cell Count", Float) = 24
        _DustAmount ("Dust Amount", Range(0, 1)) = 0
        _DustRise ("Dust Rise", Range(0, 1)) = 0.35
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
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "EnemyBlink"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _BlinkColor;
                float _BlinkAmount;
                float _PetrifyAmount;
                float _ShatterAmount;
                float _ShatterCellCount;
                float _DustAmount;
                float _DustRise;
            CBUFFER_END

            float RandomValue(float2 value)
            {
                return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.5453);
            }

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

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                spriteColor *= input.color;

                half blinkAmount = saturate(_BlinkAmount) * _BlinkColor.a;
                spriteColor.rgb = lerp(spriteColor.rgb, _BlinkColor.rgb, blinkAmount);

                half luminance = dot(spriteColor.rgb, half3(0.299, 0.587, 0.114));
                half3 stoneColor = luminance.xxx * half3(0.8, 0.84, 0.88);
                spriteColor.rgb = lerp(
                    spriteColor.rgb,
                    stoneColor,
                    saturate(_PetrifyAmount));

                float cellCount = max(_ShatterCellCount, 1.0);
                float2 cell = floor(input.uv * cellCount);
                float randomThreshold = RandomValue(cell);
                float verticalThreshold = lerp(randomThreshold, input.uv.y, 0.35);
                float visiblePiece = step(saturate(_ShatterAmount), verticalThreshold);

                float horizontalDirection = RandomValue(cell + 19.37) * 2.0 - 1.0;
                float2 dustOffset = float2(
                    horizontalDirection * _DustAmount * 0.08,
                    _DustAmount * _DustRise);
                float2 dustSourceUV = input.uv - dustOffset;
                float insideDustUV =
                    step(0.0, dustSourceUV.x) *
                    step(dustSourceUV.x, 1.0) *
                    step(0.0, dustSourceUV.y) *
                    step(dustSourceUV.y, 1.0);

                half4 dustSource = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    saturate(dustSourceUV));

                float2 dustCellPosition = frac(input.uv * cellCount) - 0.5;
                float dustRadius = lerp(0.32, 0.12, _DustAmount);
                float dustParticle = 1.0 - step(
                    dustRadius,
                    length(dustCellPosition));
                float dustAlpha =
                    dustSource.a *
                    insideDustUV *
                    dustParticle *
                    (1.0 - saturate(_DustAmount));

                float dustStage = step(0.999, saturate(_ShatterAmount));
                spriteColor.a =
                    spriteColor.a * visiblePiece * (1.0 - dustStage) +
                    dustAlpha * dustStage;

                clip(spriteColor.a - 0.001);
                return spriteColor;
            }
            ENDHLSL
        }
    }
}
