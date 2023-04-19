﻿/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/


Shader "AnyPortrait/Editor/Linear/GUI Color (Vertex Color 2X)"
{
	Properties
	{
		_MainTex("Albedo (RGBA)", 2D) = "white" {}						// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
	}

		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Add
		//Blend OneMinusDstColor One//Soft Add
		//Blend DstColor Zero//Multiply
		//Blend DstColor SrcColor//2X Multiply
		LOD 200



		//----------------------------------------------------------------------------------
		Pass
		{
			ColorMask RGB
			ZWrite off

			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ScreenSize;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);

				o.color = IN.color;
				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);

				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);
		
				float2 screenUV = IN.screenPos.xy;// *_ScreenParams.xy;
				float2 clipScreenUV = screenUV;// *_ScreenParams.xy;
				clipScreenUV.y = 1.0f - clipScreenUV.y;


				if (clipScreenUV.x < _ScreenSize.x || clipScreenUV.x > _ScreenSize.z)
				{
					c.a = 0.0f;
					discard;
				}
				if (clipScreenUV.y < _ScreenSize.y || clipScreenUV.y > _ScreenSize.w)
				{
					c.a = 0.0f;
					discard;
				}

				c.rgb *= IN.color.rgb * 2.0f;
				c.a *= IN.color.a;

				return c;
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}
