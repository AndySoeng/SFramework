﻿/*
*	Copyright (c) 2017-2021. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

Shader "AnyPortrait/Editor/Linear/Grayscale Texture"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Albedo (RGBA)", 2D) = "white" {}						// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
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
				float4 texcoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			half4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ScreenSize;
			
			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(IN.vertex);

				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);

				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);


				//블러
				half bias1 = 0.001f;
				half bias2 = 0.002f;
				half4 c1 = tex2D(_MainTex, IN.uv + half2(bias1, bias1));
				half4 c2 = tex2D(_MainTex, IN.uv + half2(-bias1, bias1));
				half4 c3 = tex2D(_MainTex, IN.uv + half2(-bias1, -bias1));
				half4 c4 = tex2D(_MainTex, IN.uv + half2(bias1, -bias1));
				half4 c5 = tex2D(_MainTex, IN.uv + half2(bias2, 0.0f));
				half4 c6 = tex2D(_MainTex, IN.uv + half2(-bias2, 0.0f));
				half4 c7 = tex2D(_MainTex, IN.uv + half2(0.0f, bias2));
				half4 c8 = tex2D(_MainTex, IN.uv + half2(0.0f, -bias2));

				c.rgb = (c.rgb * 0.2f)
					+ (c1.rgb * 0.1f) + (c2.rgb * 0.1f) + (c3.rgb * 0.1f) + (c4.rgb * 0.1f)
					+ (c5.rgb * 0.1f) + (c6.rgb * 0.1f) + (c7.rgb * 0.1f) + (c8.rgb * 0.1f)
					;


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

				//c.rgb *= _Color.rgb * 2.0f;
				c.rgb *= _Color.rgb * 4.595f;//Linear
				
				half grayscale = c.r * 0.3f + c.g * 0.6f + c.b * 0.1f;
				
				c.rgb = grayscale;
				c.a *= _Color.a;

				return c;
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}


