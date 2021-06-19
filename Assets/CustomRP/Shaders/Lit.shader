Shader "ConstomRP/Lit"
{
    Properties
    {
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		_BaseMap("Texture", 2D) = "white" {}
		[Toggle(_CLIPPING)]_Clipping("Alpha Clipping", float) = 0
		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha", Float) = 0
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", float) = 1
    }

    SubShader
    {
        Pass
        {
			Tags {"LightMode" = "CustomLit"}

			Blend [_SrcBlend][_DstBlend]
			ZWrite [_ZWrite]
			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma shader_feature _PREMULTIPLY_ALPHA
			#pragma multi_compile_instancing
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#include "LitPass.hlsl"
			ENDHLSL
        }

        Pass
        {
			Tags {"LightMode" = "ShadowCaster"}

			ColorMask 0
			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
	CustomEditor "CustomShaderGUI"
}
