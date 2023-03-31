Shader "CrossSectionURP/CapPrepare"
{
    
	Properties
	{
		_StencilMask("Stencil Mask", Range(0, 255)) = 255
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Int) = 4
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0
	}
	
	
	
	Subshader
    {

		Tags { "RenderType"="Opaque" "LightMode" = "UniversalForward"  "Queue" = "Transparent-1"}
		LOD 300

		Stencil
		{
			Ref [_StencilMask]
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
			//ZWrite On

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag         
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#pragma multi_compile __ CLIP_BOX CLIP_PIE CLIP_CORNER CLIP_PLANE CLIP_SPHERE_OUT 
			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#pragma shader_feature RAY_ORIGIN //debug in editor with colormask != None

			CBUFFER_START(UnityPerMaterial)
			//half _inverse;
			int _Culling;
			int _StencilMask;
			int _StencilOp;
			int _ZTest;
			CBUFFER_END
			#include "section_clipping_CS.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				#ifdef RETRACT_BACKFACES
				float4 normal: NORMAL;
				#endif
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
			#if CLIP_BOX||CLIP_SPHERE_OUT||CLIP_PIE
				SECTION_INTERSECT(i.wpos);
			#endif
			#if CLIP_CORNER||CLIP_PLANE
				SECTION_CLIP(i.wpos);
			#endif
				return half4(1,1,1,1);
			}
			ENDHLSL  
		}
    }
}
