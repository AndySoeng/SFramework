Shader "CrossSectionURP/CapPrepare"
{
    
	Properties
	{
		[HideInInspector] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		_StencilMask("Stencil Mask", Range(0, 255)) = 255
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Int) = 4
		[HideInInspector][Toggle(INVERSE)] _inverse("inverse", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0
	}


		Subshader
		{
			Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline"}
			LOD 100

			Stencil
			{
				Ref[_StencilMask]
				CompBack Always
				PassBack Replace

				CompFront Always
				PassFront Zero
			}

			Pass
			{

				Cull Off
				ColorMask[_ColorMask]
				ZTest[_ZTest]
				ZWrite On

				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag         
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#pragma shader_feature_local_vertex RETRACT_BACKFACES
				#pragma multi_compile __ CLIP_BOX CLIP_CORNER CLIP_PLANE CLIP_SPHERE CLIP_SPHERE_OUT
				#include "UnlitInput.hlsl"
				#include "section_clipping_CS.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float3 wpos : TEXCOORD1;
			};
            

            // Computes object space view direction
            inline float3 ObjSpaceViewDir(in float4 v)
            {
                float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
                return objSpaceCameraPos - v.xyz;
            }



			v2f vert (appdata v)
			{
			#ifdef RETRACT_BACKFACES
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				float dotProduct = dot(v.normal, viewDir);
				if(dotProduct<0) {
					float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
					float3 worldNorm = TransformObjectToWorldNormal(v.normal);
					worldPos -= worldNorm * _BackfaceExtrusion;
					v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
				}
			#endif
                v2f o;
                o.vertex = TransformObjectToHClip (v.vertex.xyz);
				o.wpos = TransformObjectToWorld(v.vertex.xyz);
                return o;



			}

			half4 frag (v2f i): SV_Target {
			#if CLIP_BOX||CLIP_SPHERE_OUT
				SECTION_INTERSECT(i.wpos);
			#endif
			#if CLIP_CORNER||CLIP_PLANE||CLIP_SPHERE
				SECTION_CLIP(i.wpos);
			#endif
				return half4(1,1,1,1);
			}
			ENDHLSL  
		}

			Pass
			{
				Name "DepthOnly"
				Tags{"LightMode" = "DepthOnly"}

				ZWrite On
				ColorMask 0
				Cull[_Cull]

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
				#pragma multi_compile __ CLIP_BOX CLIP_CORNER CLIP_PLANE CLIP_SPHERE CLIP_SPHERE_OUT

				//--------------------------------------
				// GPU Instancing
				#pragma multi_compile_instancing
				#pragma multi_compile _ DOTS_INSTANCING_ON

				#include "UnlitInput.hlsl"
				#include "DepthOnlyPass.hlsl"
				ENDHLSL
			}
    }
}
