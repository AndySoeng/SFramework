// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt) /URP/Blit

Shader "Hidden/CrossSectionURP/DepthNormText"
{
	Properties
	{
		//_ColorFront("ColorFront", Color) = (1, 0, 0, 1)
		//_ColorBack("ColorBack", Color) = (0, 1, 0, 1)
	}
	SubShader
	{
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "4.5"}
		LOD 100

		Pass
		{
			Name "DepthNormalsOnly"
			Tags{"LightMode" = "DepthNormalsOnly"}

			ZWrite On

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 4.5

			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT // forward-only variant

			//--------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CUBOID CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CUBOID CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "UnlitInput.hlsl"
			#include "UnlitDepthNormalsPass.hlsl"
			ENDHLSL
		}
	}

	SubShader
	{
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "2.0"}
		LOD 100 

		Pass
		{
			Name "DepthNormalsOnly"
			Tags{"LightMode" = "DepthNormalsOnly"}

			ZWrite On

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT // forward-only variant

			//--------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CUBOID CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CUBOID CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "UnlitInput.hlsl"
			#include "UnlitDepthNormalsPass.hlsl"
			ENDHLSL
		}

	}
	Fallback Off
}
