/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

Shader "AnyPortrait/Editor/Linear/Masked Only"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}				// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
		_PosOffsetX("PosOffsetX", Float) = 0
		_PosOffsetY("PosOffsetY", Float) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 500

		//----------------------------------------------------------------------------------
		Pass
		{
			//ColorMask 0
			ZWrite off//<이 단계에서는 Zwrite를 하지 않는다.
			ZTest Always
			Lighting Off
			//Cull Off

			//Stencil : "Z 테스트만 된다면 특정 값(53)을 저장해두자"
			/*stencil
			{
				ref 53
				comp Always
				pass replace
				fail zero
				zfail zero
			}*/

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				/*float3 vertColor_Black : TEXCOORD1;
				float3 vertColor_Red : TEXCOORD2;*/
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			half4 _Color;
			sampler2D _MainTex;
			float _PosOffsetX;
			float _PosOffsetY;

			float4 _MainTex_ST;

			float4 _ScreenSize;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				//o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.pos = UnityObjectToClipPos(IN.vertex);

				o.pos.x += _PosOffsetX;
				o.pos.y += _PosOffsetY;

				//o.color = IN.color;
				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);
				//c.a *= _Color.a;
				
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				screenUV.y = 1.0f - screenUV.y;

				if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
				{
					c.a = 0;
				}
				if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
				{
					c.a = 0;
				}
				
				if (c.a < 0.5f)
				{
					c.a = 0;
				}
				
				return c;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
