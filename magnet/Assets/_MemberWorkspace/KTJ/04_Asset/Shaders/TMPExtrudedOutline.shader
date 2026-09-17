// TMP-compatible outlined extrusion, directional edge lighting and face gradient.
// Based on the project's TMP mobile SDF implementation and Unity 6 vertex layout.

Shader "Magnet/KTJ/TMP Extruded Outline" {

Properties {
    _FaceBottomColor ("Face Bottom Tint", Color) = (0.65,0.88,1,1)
    _FaceGradientRange ("Gradient Local Y Min Max", Vector) = (-25,25,0,0)
    _SideColor ("Extrusion Side Color", Color) = (0.075,0.06,0.32,1)
    _SideTopColor ("Extrusion Near Face Color", Color) = (0.12,0.2,0.62,1)
    _ExtrusionFraction ("Extrusion / Total Shadow Offset", Range(0,1)) = 0.75
    _EdgeLightColor ("Edge Highlight", Color) = (0.75,0.9,1,1)
    _EdgeShadeColor ("Edge Shade", Color) = (0.13,0.23,0.55,1)
    _EdgeWidth ("Edge Width (SDF)", Range(0.001,0.15)) = 0.035
    _EdgeStrength ("Edge Strength", Range(0,1)) = 0.45
    _LightDirection ("Light Direction XY", Vector) = (-0.5,1,0,0)
	_FaceColor          ("Face Color", Color) = (1,1,1,1)
	_FaceDilate			("Face Dilate", Range(-1,1)) = 0

	_OutlineColor	    ("Outline Color", Color) = (0,0,0,1)
	_OutlineWidth		("Outline Thickness", Range(0,1)) = 0
	_OutlineSoftness	("Outline Softness", Range(0,1)) = 0

	_UnderlayColor	    ("Border Color", Color) = (0,0,0,.5)
	_UnderlayOffsetX 	("Border OffsetX", Range(-1,1)) = 0
	_UnderlayOffsetY 	("Border OffsetY", Range(-1,1)) = 0
	_UnderlayDilate		("Border Dilate", Range(-1,1)) = 0
	_UnderlaySoftness 	("Border Softness", Range(0,1)) = 0

	_WeightNormal		("Weight Normal", float) = 0
	_WeightBold			("Weight Bold", float) = .5

	_ShaderFlags		("Flags", float) = 0
	_ScaleRatioA		("Scale RatioA", float) = 1
	_ScaleRatioB		("Scale RatioB", float) = 1
	_ScaleRatioC		("Scale RatioC", float) = 1

	_MainTex			("Font Atlas", 2D) = "white" {}
	_TextureWidth		("Texture Width", float) = 512
	_TextureHeight		("Texture Height", float) = 512
	_GradientScale		("Gradient Scale", float) = 5
	_ScaleX				("Scale X", float) = 1
	_ScaleY				("Scale Y", float) = 1
	_PerspectiveFilter	("Perspective Correction", Range(0, 1)) = 0.875
	_Sharpness			("Sharpness", Range(-1,1)) = 0

	_VertexOffsetX		("Vertex OffsetX", float) = 0
	_VertexOffsetY		("Vertex OffsetY", float) = 0

	_ClipRect			("Clip Rect", vector) = (-32767, -32767, 32767, 32767)
	_MaskSoftnessX		("Mask SoftnessX", float) = 0
	_MaskSoftnessY		("Mask SoftnessY", float) = 0

	_StencilComp		("Stencil Comparison", Float) = 8
	_Stencil			("Stencil ID", Float) = 0
	_StencilOp			("Stencil Operation", Float) = 0
	_StencilWriteMask	("Stencil Write Mask", Float) = 255
	_StencilReadMask	("Stencil Read Mask", Float) = 255

	_CullMode			("Cull Mode", Float) = 0
	_ColorMask			("Color Mask", Float) = 15
}

SubShader {
	Tags
	{
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}


	Stencil
	{
		Ref [_Stencil]
		Comp [_StencilComp]
		Pass [_StencilOp]
		ReadMask [_StencilReadMask]
		WriteMask [_StencilWriteMask]
	}

	Cull [_CullMode]
	ZWrite Off
	Lighting Off
	Fog { Mode Off }
	ZTest [unity_GUIZTestMode]
	Blend One OneMinusSrcAlpha
	ColorMask [_ColorMask]

	Pass {
		CGPROGRAM
		#pragma target 3.0
		#pragma vertex VertShader
		#pragma fragment PixShader
		#define OUTLINE_ON 1
		#define UNDERLAY_ON 1

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#pragma multi_compile __ UNITY_UI_ALPHACLIP

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		#include "Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc"

		struct vertex_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4	vertex			: POSITION;
			float3	normal			: NORMAL;
			fixed4	color			: COLOR;
			float4	texcoord0		: TEXCOORD0;
			float2	texcoord1		: TEXCOORD1;
		};

		struct pixel_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
			float4	vertex			: SV_POSITION;
			fixed4	faceColor		: COLOR;
			fixed4	outlineColor	: COLOR1;
			float4	texcoord0		: TEXCOORD0;			// Texture UV, Mask UV
			half4	param			: TEXCOORD1;			// Scale(x), BiasIn(y), BiasOut(z), Bias(w)
			half4	mask			: TEXCOORD2;			// Position in clip space(xy), Softness(zw)
			#if (UNDERLAY_ON | UNDERLAY_INNER)
			float4	texcoord1		: TEXCOORD3;			// Texture UV, alpha, reserved
			half2	underlayParam	: TEXCOORD4;			// Scale(x), Bias(y)
			#endif
		};

		float4 _FaceBottomColor, _FaceGradientRange, _SideColor, _SideTopColor;
        float4 _EdgeLightColor, _EdgeShadeColor, _LightDirection;
        float _ExtrusionFraction, _EdgeWidth, _EdgeStrength;
        float _UIMaskSoftnessX;
        float _UIMaskSoftnessY;
        int _UIVertexColorAlwaysGammaSpace;

		pixel_t VertShader(vertex_t input)
		{
			pixel_t output;

			UNITY_INITIALIZE_OUTPUT(pixel_t, output);
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			float bold = step(input.texcoord0.w, 0);

			float4 vert = input.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;
			float4 vPosition = UnityObjectToClipPos(vert);

			float2 pixelSize = vPosition.w;
			pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

			float scale = rsqrt(dot(pixelSize, pixelSize));
			scale *= abs(input.texcoord0.w) * _GradientScale * (_Sharpness + 1);
			if(UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

			float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
			weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

			float layerScale = scale;

			scale /= 1 + (_OutlineSoftness * _ScaleRatioA * scale);
			float bias = (0.5 - weight) * scale - 0.5;
			float outline = _OutlineWidth * _ScaleRatioA * 0.5 * scale;

            if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
            {
                input.color.rgb = UIGammaToLinear(input.color.rgb);
            }
            float opacity = input.color.a;
			#if (UNDERLAY_ON | UNDERLAY_INNER)
			opacity = 1.0;
			#endif

			float gradient = saturate((vert.y - _FaceGradientRange.x) / max(0.001, _FaceGradientRange.y - _FaceGradientRange.x));
            fixed4 faceColor = fixed4(input.color.rgb, opacity) * _FaceColor * lerp(_FaceBottomColor, float4(1,1,1,1), gradient);
			faceColor.rgb *= faceColor.a;

			fixed4 outlineColor = _OutlineColor;
			outlineColor.a *= opacity;
			outlineColor.rgb *= outlineColor.a;
			outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline * 2))));

			#if (UNDERLAY_ON | UNDERLAY_INNER)
			layerScale /= 1 + ((_UnderlaySoftness * _ScaleRatioC) * layerScale);
			float layerBias = (.5 - weight) * layerScale - .5 - ((_UnderlayDilate * _ScaleRatioC) * .5 * layerScale);

			float x = -(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureWidth;
			float y = -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureHeight;
			float2 layerOffset = float2(x, y);
			#endif

			// Generate UV for the Masking Texture
			float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
			float2 maskUV = (vert.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);

			// Populate structure for pixel shader
			output.vertex = vPosition;
			output.faceColor = faceColor;
			output.outlineColor = outlineColor;
			output.texcoord0 = float4(input.texcoord0.x, input.texcoord0.y, maskUV.x, maskUV.y);
			output.param = half4(scale, bias - outline, bias + outline, bias);

			const half2 maskSoftness = half2(max(_UIMaskSoftnessX, _MaskSoftnessX), max(_UIMaskSoftnessY, _MaskSoftnessY));
			output.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * maskSoftness + pixelSize.xy));
			#if (UNDERLAY_ON || UNDERLAY_INNER)
			output.texcoord1 = float4(input.texcoord0 + layerOffset, input.color.a, 0);
			output.underlayParam = half2(layerScale, layerBias);
			#endif

			return output;
		}


		// PIXEL SHADER
		fixed4 PixShader(pixel_t input) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(input);

			float2 uv = input.texcoord0.xy;
            float sdf = tex2D(_MainTex, uv).a;
            float d = sdf * input.param.x;
            float faceMask = saturate(d - input.param.z);
            float outerMask = saturate(d - input.param.y);
            float2 texel = 1.0 / float2(_TextureWidth, _TextureHeight);
            float2 normal = -float2(
                tex2D(_MainTex, uv + float2(texel.x, 0)).a - tex2D(_MainTex, uv - float2(texel.x, 0)).a,
                tex2D(_MainTex, uv + float2(0, texel.y)).a - tex2D(_MainTex, uv - float2(0, texel.y)).a);
            normal /= max(length(normal), 0.00001);
            float2 light = _LightDirection.xy / max(length(_LightDirection.xy), 0.00001);
            float lighting = dot(normal, light);
            float faceBoundary = (input.param.z + 0.5) / max(input.param.x, 0.00001);
            float edge = (1 - smoothstep(0, _EdgeWidth, max(0, sdf - faceBoundary))) * _EdgeStrength;
            float4 face = input.faceColor;
            face.rgb = lerp(face.rgb, _EdgeLightColor.rgb * face.a, edge * saturate(lighting) * _EdgeLightColor.a);
            face.rgb = lerp(face.rgb, _EdgeShadeColor.rgb * face.a, edge * saturate(-lighting) * _EdgeShadeColor.a);
            float4 c = lerp(input.outlineColor, face, faceMask) * outerMask;

            // Sweep the outlined silhouette. TMP underlay properties reserve the padding.
            float2 totalOffset = input.texcoord1.xy - uv;
            float sideMask = 0;
            float closestDepth = 1;
            [unroll]
            for (int sampleIndex = 1; sampleIndex <= 16; ++sampleIndex)
            {
                float depth = sampleIndex / 16.0;
                float sd = tex2D(_MainTex, uv + totalOffset * (_ExtrusionFraction * depth)).a;
                float coverage = saturate(sd * input.param.x - input.param.y);
                sideMask = max(sideMask, coverage);
                if (coverage > 0.5) closestDepth = min(closestDepth, depth);
            }
            float4 side = lerp(_SideTopColor, _SideColor, closestDepth);
            side.a *= sideMask;
            side.rgb *= side.a;
            c += side * (1 - c.a);

            float shadowDistance = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
            float shadowMask = saturate(shadowDistance - input.underlayParam.y);
            c += float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a) * shadowMask * (1 - c.a);
// Alternative implementation to UnityGet2DClipping with support for softness.
			#if UNITY_UI_CLIP_RECT
			half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
			c *= m.x * m.y;
			#endif

			#if (UNDERLAY_ON | UNDERLAY_INNER)
			c *= input.texcoord1.z;
			#endif

			#if UNITY_UI_ALPHACLIP
			clip(c.a - 0.001);
			#endif

			return c;
		}
		ENDCG
	}
}


CustomEditor "Magnet.KTJ.Editor.ExtrudedTextShaderGUI"
}
