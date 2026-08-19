Shader "Magnet/ThemeBurst"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _Mode ("Theme Mode", Float) = 0
        _Softness ("Edge Softness", Range(.001,.15)) = .025
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
            float _Mode;
            float _Softness;
            struct A { float4 positionOS:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            V vert(A v) { V o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.color=v.color; o.uv=v.uv; return o; }
            half4 frag(V i):SV_Target
            {
                float2 p=i.uv*2-1;
                float r=length(p);
                float a=atan2(p.y,p.x);
                float shape;
                if (_Mode < .5) shape=1-smoothstep(.66,.66+_Softness,max(abs(p.x+p.y*.42),abs(p.y-p.x*.25))); // ice
                else if (_Mode < 1.5) shape=1-smoothstep(.76,.76+_Softness,r*(1+.15*sin(a*5))); // lava
                else if (_Mode < 2.5) { float c=min(length(p-float2(-.22,.05)),min(length(p-float2(.22,.06)),length(p-float2(0,-.18)))); shape=1-smoothstep(.45,.45+_Softness,c); }
                else if (_Mode < 3.5) shape=(1-smoothstep(.66,.66+_Softness,max(abs(p.x),abs(p.y)))); // pixels
                else if (_Mode < 4.5) shape=1-smoothstep(.7,.7+_Softness,r*(.72+.28*abs(sin(a*5)))); // stars
                else if (_Mode < 5.5) shape=1-smoothstep(.7,.7+_Softness,max(abs(p.x),abs(p.y))); // crumbs
                else if (_Mode < 6.5) shape=1-smoothstep(.68,.68+_Softness,max(abs(p.x+p.y*.3),abs(p.y-p.x*.3))); // candy
                else if (_Mode < 7.5) shape=1-smoothstep(.7,.7+_Softness,max(abs(p.x),abs(p.y*.42))); // wood chip
                else if (_Mode < 8.5) shape=1-smoothstep(.68,.68+_Softness,max(abs(p.x+p.y*.24),abs(p.y))); // fabric
                else shape=1-smoothstep(.76,.76+_Softness,r*(1+.2*sin(a*7))); // ink
                float rim=saturate(r)*shape;
                float3 col=lerp(_Tint.rgb*.55,_Tint.rgb*1.35+.08,rim);
                return half4(col,shape*i.color.a);
            }
            ENDHLSL
        }
    }
}
