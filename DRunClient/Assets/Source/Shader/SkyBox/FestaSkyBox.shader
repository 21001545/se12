Shader "LifeFesta/FestaSkyBox"
{
	Properties
	{
		[Space(10)] [Header(Layer0)]
		_Layer0_StartPosition("_Layer0_StartPosition", Vector) = (1,1,0,0)
		_Layer0_EndPosition("_Layer0_EndPosition", Vector) = (0,0,0,0)
		_Layer0_Alpha("_Layer0_Alpha", Range(0,1)) = 1
		_Layer0_StartColor("_Layer0_StartColor", Color) = (1,1,1,1)
		_Layer0_EndColor("_Layer0_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer1)]
		_Layer1_StartPosition("_Layer1_StartPosition", Vector) = (1,1,0,0)
		_Layer1_EndPosition("_Layer1_EndPosition", Vector) = (0,0,0,0)
		_Layer1_Alpha("_Layer1_Alpha", Range(0,1)) = 1
		_Layer1_StartColor("_Layer1_StartColor", Color) = (1,1,1,1)
		_Layer1_EndColor("_Layer1_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer2)]
		_Layer2_StartPosition("_Layer2_StartPosition", Vector) = (1,1,0,0)
		_Layer2_EndPosition("_Layer2_EndPosition", Vector) = (0,0,0,0)
		_Layer2_Alpha("_Layer2_Alpha", Range(0,1)) = 1
		_Layer2_StartColor("_Layer2_StartColor", Color) = (1,1,1,1)
		_Layer2_EndColor("_Layer2_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer3)]
		_Layer3_StartPosition("_Layer3_StartPosition", Vector) = (1,1,0,0)
		_Layer3_EndPosition("_Layer3_EndPosition", Vector) = (0,0,0,0)
		_Layer3_Alpha("_Layer3_Alpha", Range(0,1)) = 1
		_Layer3_EndColor("_Layer3_EndColor", Color) = (1,1,1,1)
		_Layer3_StartColor("_Layer3_StartColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer4)]
		_Layer4_StartPosition("_Layer4_StartPosition", Vector) = (1,1,0,0)
		_Layer4_EndPosition("_Layer4_EndPosition", Vector) = (0,0,0,0)
		_Layer4_Alpha("_Layer4_Alpha", Range(0,1)) = 1
		_Layer4_StartColor("_Layer4_StartColor", Color) = (1,1,1,1)
		_Layer4_EndColor("_Layer4_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer5)]
		_Layer5_StartPosition("_Layer5_StartPosition", Vector) = (1,1,0,0)
		_Layer5_EndPosition("_Layer5_EndPosition", Vector) = (0,0,0,0)
		_Layer5_Alpha("_Layer5_Alpha", Range(0,1)) = 1
		_Layer5_StartColor("_Layer5_StartColor", Color) = (1,1,1,1)
		_Layer5_EndColor("_Layer5_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer6)]
		_Layer6_StartPosition("_Layer6_StartPosition", Vector) = (1,1,0,0)
		_Layer6_EndPosition("_Layer6_EndPosition", Vector) = (0,0,0,0)
		_Layer6_Alpha("_Layer6_Alpha", Range(0,1)) = 1
		_Layer6_StartColor("_Layer6_StartColor", Color) = (1,1,1,1)
		_Layer6_EndColor("_Layer6_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer7)]
		_Layer7_StartPosition("_Layer7_StartPosition", Vector) = (1,1,0,0)
		_Layer7_EndPosition("_Layer7_EndPosition", Vector) = (0,0,0,0)
		_Layer7_Alpha("_Layer7_Alpha", Range(0,1)) = 1
		_Layer7_StartColor("_Layer7_StartColor", Color) = (1,1,1,1)
		_Layer7_EndColor("_Layer7_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer8)]
		_Layer8_StartPosition("_Layer8_StartPosition", Vector) = (1,1,0,0)
		_Layer8_EndPosition("_Layer8_EndPosition", Vector) = (0,0,0,0)
		_Layer8_Alpha("_Layer8_Alpha", Range(0,1)) = 1
		_Layer8_StartColor("_Layer8_StartColor", Color) = (1,1,1,1)
		_Layer8_EndColor("_Layer8_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer9)]
		_Layer9_StartPosition("_Layer9_StartPosition", Vector) = (1,1,0,0)
		_Layer9_EndPosition("_Layer9_EndPosition", Vector) = (0,0,0,0)
		_Layer9_Alpha("_Layer9_Alpha", Range(0,1)) = 1
		_Layer9_StartColor("_Layer9_StartColor", Color) = (1,1,1,1)
		_Layer9_EndColor("_Layer9_EndColor", Color) = (1,1,1,1)

		[Space(10)][Header(Layer10)]
		_Layer10_StartPosition("_Layer10_StartPosition", Vector) = (1,1,0,0)
		_Layer10_EndPosition("_Layer10_EndPosition", Vector) = (0,0,0,0)
		_Layer10_Alpha("_Layer10_Alpha", Range(0,1)) = 1
		_Layer10_StartColor("_Layer10_StartColor", Color) = (1,1,1,1)
		_Layer10_EndColor("_Layer10_EndColor", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "IgnoreProjector" = "True" }
		Cull Back     // Render side
		Fog{Mode Off} // Don't use fog
		ZWrite Off    // Don't draw to depth buffer

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vertexMain
			#pragma fragment fragmentMain

			#pragma shader_feature_local LAYER_0_DARKEN_BLEND 
			#pragma shader_feature_local LAYER_0_LIGHTEN_BLEND 
			#pragma shader_feature_local LAYER_0_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local LAYER_0_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_0_USE_LINEAR_GRADIENT

			#pragma shader_feature_local LAYER_1_USE_LINEAR_GRADIENT
			#pragma shader_feature_local LAYER_1_DARKEN_BLEND 
			#pragma shader_feature_local LAYER_1_LIGHTEN_BLEND 
			#pragma shader_feature_local LAYER_1_LINEAR_LIGHT_BLEND 
			#pragma shader_feature_local LAYER_1_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_2_USE_LINEAR_GRADIENT 
			#pragma shader_feature_local LAYER_2_DARKEN_BLEND 
			#pragma shader_feature_local LAYER_2_LIGHTEN_BLEND 
			#pragma shader_feature_local LAYER_2_LINEAR_LIGHT_BLEND 
			#pragma shader_feature_local LAYER_2_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_3_USE_LINEAR_GRADIENT 
			#pragma shader_feature_local LAYER_3_DARKEN_BLEND 
			#pragma shader_feature_local LAYER_3_LIGHTEN_BLEND 
			#pragma shader_feature_local LAYER_3_LINEAR_LIGHT_BLEND 
			#pragma shader_feature_local LAYER_3_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_4_USE_LINEAR_GRADIENT 
			#pragma shader_feature_local LAYER_4_DARKEN_BLEND 
			#pragma shader_feature_local LAYER_4_LIGHTEN_BLEND 
			#pragma shader_feature_local LAYER_4_LINEAR_LIGHT_BLEND 
			#pragma shader_feature_local LAYER_4_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_5_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_5_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_5_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_5_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_5_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_6_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_6_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_6_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_6_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_6_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_7_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_7_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_7_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_7_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_7_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_8_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_8_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_8_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_8_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_8_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_9_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_9_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_9_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_9_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_9_SOFT_LIGHT_BLEND
			#pragma shader_feature_local LAYER_10_USE_LINEAR_GRADIENT
			#pragma shader_feature_local  LAYER_10_DARKEN_BLEND
			#pragma shader_feature_local  LAYER_10_LIGHTEN_BLEND
			#pragma shader_feature_local  LAYER_10_LINEAR_LIGHT_BLEND
			#pragma shader_feature_local  LAYER_10_SOFT_LIGHT_BLEND

			#pragma target 3.0
			#include "UnityCG.cginc"

			// Mesh data
			struct VertexData
			{
				float4 vertex : POSITION;
			};

	// Vertex to fragment
	struct OutVertexData
	{
		float4 Position : SV_POSITION;
		float3 WorldPos : TEXCOORD0;
		float3 SunPos   : TEXCOORD1;
		float3 MoonPos  : TEXCOORD2;
		float3 StarPos  : TEXCOORD3;
		float4 CloudUV  : TEXCOORD4;
	};

	uniform float2 _Layer0_StartPosition;
	uniform float2 _Layer0_EndPosition;
	uniform float4 _Layer0_StartColor;
	uniform float4 _Layer0_EndColor;
	uniform float _Layer0_Alpha;

	uniform float2 _Layer1_StartPosition;
	uniform float2 _Layer1_EndPosition;
	uniform float4 _Layer1_StartColor;
	uniform float4 _Layer1_EndColor;
	uniform float _Layer1_Alpha;

	uniform float2 _Layer2_StartPosition;
	uniform float2 _Layer2_EndPosition;
	uniform float4 _Layer2_StartColor;
	uniform float4 _Layer2_EndColor;
	uniform float _Layer2_Alpha;

	uniform float2 _Layer3_StartPosition;
	uniform float2 _Layer3_EndPosition;
	uniform float4 _Layer3_StartColor;
	uniform float4 _Layer3_EndColor;
	uniform float _Layer3_Alpha;

	uniform float2 _Layer4_StartPosition;
	uniform float2 _Layer4_EndPosition;
	uniform float4 _Layer4_StartColor;
	uniform float4 _Layer4_EndColor;
	uniform float _Layer4_Alpha;

	uniform float2 _Layer5_StartPosition;
	uniform float2 _Layer5_EndPosition;
	uniform float4 _Layer5_StartColor;
	uniform float4 _Layer5_EndColor;
	uniform float _Layer5_Alpha;

	uniform float2 _Layer6_StartPosition;
	uniform float2 _Layer6_EndPosition;
	uniform float4 _Layer6_StartColor;
	uniform float4 _Layer6_EndColor;
	uniform float _Layer6_Alpha;

	uniform float2 _Layer7_StartPosition;
	uniform float2 _Layer7_EndPosition;
	uniform float4 _Layer7_StartColor;
	uniform float4 _Layer7_EndColor;
	uniform float _Layer7_Alpha;

	uniform float2 _Layer8_StartPosition;
	uniform float2 _Layer8_EndPosition;
	uniform float4 _Layer8_StartColor;
	uniform float4 _Layer8_EndColor;
	uniform float _Layer8_Alpha;

	uniform float2 _Layer9_StartPosition;
	uniform float2 _Layer9_EndPosition;
	uniform float4 _Layer9_StartColor;
	uniform float4 _Layer9_EndColor;
	uniform float _Layer9_Alpha;

	uniform float2 _Layer10_StartPosition;
	uniform float2 _Layer10_EndPosition;
	uniform float4 _Layer10_StartColor;
	uniform float4 _Layer10_EndColor;
	uniform float _Layer10_Alpha;


	OutVertexData vertexMain(VertexData v)
	{
		OutVertexData Output = (OutVertexData)0;

		Output.Position = UnityObjectToClipPos(v.vertex);
		Output.WorldPos = normalize(mul((float3x3)unity_WorldToObject, v.vertex.xyz));
		return Output;
	}

	float LinearGradient(float2 startPos, float2 endPos, float2 currentPos)
	{
		float2 dest = endPos - startPos;
		float2 D = startPos + dot((currentPos - startPos), dest) * dest / dot(dest, dest);
		float l = distance(D, startPos) / distance(endPos, startPos);
		return smoothstep(0, 1, l);
	}

	float RadialGradient(float2 startPos, float2 endPos, float2 currentPos)
	{
		//float2 dest = endPos - startPos;
		//float2 D = startPos + dot((currentPos - startPos), dest) * dest / dot(dest, dest);
		float l = distance(currentPos, startPos) / distance(endPos, startPos) ;
		return smoothstep(0, 1, l);
	}

	float3 NormalBlend(float3 base, float3 blend, float opacity)
	{
		return blend * opacity + base * (1.0 - opacity);
	}

	float3 DarkenBlend(float3 base, float3 blend, float opacity)
	{
		return float3(min(base.x,blend.x), min(base.y,blend.y), min(base.z, blend.z)) * opacity + base * (1.0 - opacity);
	}

	float3 LinearLightBlend(float3 base, float3 blend, float opacity)
	{
		float3 temp;
		temp.x = (blend.x > 0.5) ? (max(base.x, 2.0 * (blend.x - 0.5))) : (min(base.x, 2.0 * blend.x));
		temp.y = (blend.y > 0.5) ? (max(base.y, 2.0 * (blend.y - 0.5))) : (min(base.y, 2.0 * blend.y));
		temp.z = (blend.z > 0.5) ? (max(base.z, 2.0 * (blend.z - 0.5))) : (min(base.z, 2.0 * blend.z));

		return temp * opacity * opacity + base * (1.0 - opacity);
	}

	float3 SoftLightBlend(float3 base, float3 blend, float opacity)
	{
		float3 temp;
		temp.x = (blend.x > 0.5) ? (1.0 - (1.0 - base.x) * (1.0 - (blend.x - 0.5))) : (base.x * (blend.x + 0.5));
		temp.y = (blend.y > 0.5) ? (1.0 - (1.0 - base.y) * (1.0 - (blend.y - 0.5))) : (base.y * (blend.y + 0.5));
		temp.z = (blend.z > 0.5) ? (1.0 - (1.0 - base.z) * (1.0 - (blend.z - 0.5))) : (base.z * (blend.z + 0.5));

		return temp * opacity * opacity + base * (1.0 - opacity);
	}

	float LightenBlend(float3 base, float3 blend, float opacity)
	{
		return float3(max(base.x,blend.x* opacity), max(base.y,blend.y * opacity), max(base.z, blend.z* opacity)) * opacity + base * (1.0 - opacity);
	}

	float4 fragmentMain(OutVertexData Input) : SV_Target
	{
		float3 result = float3(1,1,1);

		float4 color;
		float opacity;

		#if LAYER_0_USE_LINEAR_GRADIENT
			float value = LinearGradient(_Layer0_StartPosition, _Layer0_EndPosition, Input.WorldPos.xy);
			color = lerp(_Layer0_StartColor, _Layer0_EndColor, value);
		#else 
			float value = RadialGradient(_Layer0_StartPosition, _Layer0_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer0_StartColor, _Layer0_EndColor, value);
		#endif

		opacity = _Layer0_Alpha * color.a;

		#if LAYER_0_SOFT_LIGHT_BLEND
			result = SoftLightBlend(result, color, opacity);
		#elif LAYER_0_LINEAR_LIGHT_BLEND
			result = LinearLightBlend(result, color, opacity);
		#elif LAYER_0_DARKEN_BLEND
			result = DarkenBlend(result, color, opacity);
		#elif LAYER_0_LIGHTEN_BLEND
			result = LightenBlend(result, color, opacity);
		#else
			result = NormalBlend(result, color, opacity);
		#endif 

		#if LAYER_1_USE_LINEAR_GRADIENT
		value = LinearGradient(_Layer1_StartPosition, _Layer1_EndPosition, Input.WorldPos.xy);
		color = lerp(_Layer1_StartColor, _Layer1_EndColor, value);
		#else 
			value = RadialGradient(_Layer1_StartPosition, _Layer1_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer1_StartColor, _Layer1_EndColor, value);
		#endif
		opacity = _Layer1_Alpha * color.a;
		#if LAYER_1_SOFT_LIGHT_BLEND
			result = SoftLightBlend(result, color, opacity);
		#elif LAYER_1_LINEAR_LIGHT_BLEND
			result = LinearLightBlend(result, color, opacity);
		#elif LAYER_1_DARKEN_BLEND
			result = DarkenBlend(result, color, opacity);			
		#elif LAYER_1_LIGHTEN_BLEND
			result = LightenBlend(result, color, opacity);
		#else
			result = NormalBlend(result, color, opacity);
		#endif

		#if LAYER_2_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer2_StartPosition, _Layer2_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer2_StartColor, _Layer2_EndColor, value);
		#else 
			flo value = RadialGradient(_Layer2_StartPosition, _Layer2_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer2_StartColor, _Layer2_EndColor, value);
		#endif
			 opacity = _Layer2_Alpha * color.a;

			 #if LAYER_2_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_2_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_2_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_2_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_3_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer3_StartPosition, _Layer3_EndPosition, Input.WorldPos.xy);
			 color= lerp(_Layer3_StartColor, _Layer3_EndColor, value);
		#else 
			value = RadialGradient(_Layer3_StartPosition, _Layer3_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer3_StartColor, _Layer3_EndColor, value);
		#endif
			 opacity = _Layer3_Alpha* color.a;

			 #if LAYER_3_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_3_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_3_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_3_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_4_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer4_StartPosition, _Layer4_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer4_StartColor, _Layer4_EndColor, value);
		#else 
			value = RadialGradient(_Layer4_StartPosition, _Layer4_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer4_StartColor, _Layer4_EndColor, value);
		#endif
			 opacity = _Layer4_Alpha* color.a;

			 #if LAYER_4_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_4_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_4_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_4_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_5_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer5_StartPosition, _Layer5_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer5_StartColor, _Layer5_EndColor, value);
		#else 
			value = RadialGradient(_Layer5_StartPosition, _Layer5_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer5_StartColor, _Layer5_EndColor, value);
		#endif
			 opacity = _Layer5_Alpha* color.a;

			 #if LAYER_5_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_5_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_5_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_5_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_6_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer6_StartPosition, _Layer6_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer6_StartColor, _Layer6_EndColor, value);
		#else 
			value = RadialGradient(_Layer6_StartPosition, _Layer6_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer6_StartColor, _Layer6_EndColor, value);
		#endif
			 opacity = _Layer6_Alpha* color.a;

			 #if LAYER_6_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_6_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_6_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_6_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_7_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer7_StartPosition, _Layer7_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer7_StartColor, _Layer7_EndColor, value);
		#else 
			value = RadialGradient(_Layer7_StartPosition, _Layer7_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer7_StartColor, _Layer7_EndColor, value);
		#endif
			 opacity = _Layer7_Alpha* color.a;

			 #if LAYER_7_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_7_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_7_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_7_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_8_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer8_StartPosition, _Layer8_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer8_StartColor, _Layer8_EndColor, value);
		#else 
			value = RadialGradient(_Layer8_StartPosition, _Layer8_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer8_StartColor, _Layer8_EndColor, value);
		#endif
			 opacity = _Layer8_Alpha* color.a;

			 #if LAYER_8_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_8_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_8_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_8_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_9_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer9_StartPosition, _Layer9_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer9_StartColor, _Layer9_EndColor, value);
		#else 
			value = RadialGradient(_Layer9_StartPosition, _Layer9_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer9_StartColor, _Layer9_EndColor, value);
		#endif
			 opacity = _Layer9_Alpha* color.a;

			 #if LAYER_9_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_9_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_9_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_9_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif

		#if LAYER_10_USE_LINEAR_GRADIENT
			 value = LinearGradient(_Layer10_StartPosition, _Layer10_EndPosition, Input.WorldPos.xy);
			 color = lerp(_Layer10_StartColor, _Layer10_EndColor, value);
		#else 
			value = RadialGradient(_Layer10_StartPosition, _Layer10_EndPosition, Input.WorldPos.xy); // 방사형
			color = lerp(_Layer10_StartColor, _Layer10_EndColor, value);
		#endif
			 opacity = _Layer10_Alpha* color.a;

			 #if LAYER_10_SOFT_LIGHT_BLEND
			 	result = SoftLightBlend( result, color, opacity);
			 #elif LAYER_10_LINEAR_LIGHT_BLEND
			 	result = LinearLightBlend( result, color, opacity);
			 #elif LAYER_10_DARKEN_BLEND
			 	result = DarkenBlend( result, color, opacity);
			 #elif LAYER_10_LIGHTEN_BLEND
			 	result = LightenBlend( result, color, opacity);
			 #else
			 	result = NormalBlend( result, color, opacity);
			 #endif
			return float4(result.rgb, 1);
		}

		ENDHLSL
	}
	}
}
