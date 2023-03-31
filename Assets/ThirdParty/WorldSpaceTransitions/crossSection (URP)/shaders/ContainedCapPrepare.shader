Shader "CrossSectionURP/ContainedCapPrepare"
{
    
	Properties
	{
		_BaseColor("Main Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CullMode)] _Culling("Cull Mode", Int) = 2
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Operation", Int) = 2
		_StencilMask("Stencil Mask", Range(0, 255)) = 1
	}
	
	
	
	Subshader
    {

		Tags { "RenderType"="Opaque" "LightMode" = "UniversalForward" }
		LOD 300

		Stencil
		{
			Ref [_StencilMask]
			Comp Always
			Pass [_StencilOp]

		}

		Pass
		{

			Cull [_Culling]
			//ColorMask 0

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag         
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CUBOID

			CBUFFER_START(UnityPerMaterial)
			half4 _BaseColor;
			//half _inverse;
			int _Culling;
			int _StencilMask;
			int _StencilOp;
			CBUFFER_END

			#include "section_clipping_CS.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
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
			/*
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
			*/
				
                v2f o;
                o.vertex = TransformObjectToHClip (v.vertex.xyz);
				o.wpos = TransformObjectToWorld(v.vertex.xyz);
                return o;



			}


			half4 frag (v2f i): SV_Target 
			{
				SECTION_CLIP(i.wpos);
				return _BaseColor;
			}
			ENDHLSL  
		}
    }
}
