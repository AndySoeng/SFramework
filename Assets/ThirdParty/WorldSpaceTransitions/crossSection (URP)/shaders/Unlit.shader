Shader "CrossSectionURP/Unlit"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
		[MainTexture] _BaseMap("Texture", 2D) = "white" {}
		[MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
		_Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

		// BlendMode
		_Surface("__surface", Float) = 0.0
		_Blend("__mode", Float) = 0.0
		_Cull("__cull", Float) = 2.0
		[ToggleUI] _AlphaClip("__clip", Float) = 0.0
		[HideInInspector] _BlendOp("__blendop", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0

		// Editmode props
		_QueueOffset("Queue offset", Float) = 0.0

		// ObsoleteProperties
		[HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
		[HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
		[HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit

		// CrossSection properties
		_SectionColor ("Section Color", Color) = (1,0,0,1)
		[Toggle(INVERSE)] _inverse("inverse", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 0

		//[HideInInspector] _SectionPoint("_SectionPoint", Vector) = (0,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane("_SectionPlane", Vector) = (1,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane2("_SectionPlane2", Vector) = (0,1,0,1)	//expose as local properties
		//[HideInInspector] _Radius("_Radius", Vector) = (0,1,0,1)	//expose as local properties

		//[HideInInspector] _SectionScale("_SectionScale", Vector) = (0,0,1,1)	//expose as local properties
    }

    SubShader
    {
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "4.5"}
		LOD 100

		Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]
        Cull[_Cull]

        Pass
        {
            Name "Unlit"

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 4.5

			#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ DEBUG_DISPLAY

            // -------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.
			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#pragma shader_feature INVERSE

			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment

            #include "UnlitInput.hlsl"
			#include "UnlitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON

			//--------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

            //--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "UnlitInput.hlsl"
            #include "DepthOnlyPass.hlsl"
            ENDHLSL
        }

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
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "UnlitInput.hlsl"
			#include "UnlitDepthNormalsPass.hlsl"
			ENDHLSL
		}

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 4.5

			#pragma vertex UniversalVertexMeta
			#pragma fragment UniversalFragmentMetaUnlit
			#pragma shader_feature EDITOR_VISUALIZATION

            #include "UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }

	SubShader
	{
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "2.0"}
		LOD 100

		Blend[_SrcBlend][_DstBlend]
		ZWrite[_ZWrite]
		Cull[_Cull]

		Pass
		{
			Name "Unlit"
			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ DEBUG_DISPLAY

			// -------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.

			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#pragma shader_feature INVERSE

			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment

			#include "UnlitInput.hlsl"
			#include "UnlitForwardPass.hlsl"
			ENDHLSL
		}



		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			//--------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

			#include "UnlitInput.hlsl"
			#include "DepthOnlyPass.hlsl"
			ENDHLSL
		}

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
			#pragma multi_compile __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_SPHERE CLIP_BOX CLIP_CORNER // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "UnlitInput.hlsl"
			#include "UnlitDepthNormalsPass.hlsl"
			ENDHLSL
		}

		// This pass it not used during regular rendering, only for lightmap baking.
		Pass
		{
			Name "Meta"
			Tags{"LightMode" = "Meta"}

			Cull Off

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma vertex UniversalVertexMeta
			#pragma fragment UniversalFragmentMetaUnlit
			#pragma shader_feature EDITOR_VISUALIZATION

			#include "UnlitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

			ENDHLSL
		}
	}
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.CrossSection.URP.ShaderGUI.UnlitShader"
}
