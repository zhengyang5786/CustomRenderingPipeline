Shader "ConstomRP/Unlit"
{
    Properties
    {
		_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_BaseMap("Texture", 2D) = "white" {}
		[Toggle(_CLIPPING)]_Clipping("Alpha Clipping", float) = 0
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", float) = 1
    }

    SubShader
    {
		Tags {"LightMode" = "SRPDefaultUnlit"}
        Pass
        {
			Blend [_SrcBlend][_DstBlend]
			ZWrite [_ZWrite]
			HLSLPROGRAM
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
        }
    }
	CustomEditor "CustomShaderGUI"
}
