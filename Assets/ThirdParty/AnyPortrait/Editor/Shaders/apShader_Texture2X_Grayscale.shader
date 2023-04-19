/*
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
Shader "AnyPortrait/Editor/Grayscale Texture"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Albedo (RGBA)", 2D) = "white" {}						// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
	}
		
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		
		//Cull Off
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleColor alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		half4 LightingSimpleColor(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		half4 _Color;
		sampler2D _MainTex;
		float4 _ScreenSize;

		struct Input
		{
			float2 uv_MainTex;
			float4 screenPos;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV.y = 1.0f - screenUV.y;

			half4 c = tex2D(_MainTex, IN.uv_MainTex);

			//블러
			half bias1 = 0.001f;
			half bias2 = 0.002f;
			half4 c1 = tex2D(_MainTex, IN.uv_MainTex + half2(bias1, bias1));
			half4 c2 = tex2D(_MainTex, IN.uv_MainTex + half2(-bias1, bias1));
			half4 c3 = tex2D(_MainTex, IN.uv_MainTex + half2(-bias1, -bias1));
			half4 c4 = tex2D(_MainTex, IN.uv_MainTex + half2(bias1, -bias1));
			half4 c5 = tex2D(_MainTex, IN.uv_MainTex + half2(bias2, 0.0f));
			half4 c6 = tex2D(_MainTex, IN.uv_MainTex + half2(-bias2, 0.0f));
			half4 c7 = tex2D(_MainTex, IN.uv_MainTex + half2(0.0f, bias2));
			half4 c8 = tex2D(_MainTex, IN.uv_MainTex + half2(0.0f, -bias2));

			c.rgb = (c.rgb * 0.2f) 
				+ (c1.rgb * 0.1f) + (c2.rgb * 0.1f) + (c3.rgb * 0.1f) + (c4.rgb * 0.1f)
				+ (c5.rgb * 0.1f) + (c6.rgb * 0.1f) + (c7.rgb * 0.1f) + (c8.rgb * 0.1f)
				;


			c.rgb *= _Color.rgb * 2.0f;

			//기본 명도 색상으로 바꾼다.
			half grayscale = (c.r * 0.3f) + (c.g * 0.6f) + (c.b * 0.1f);
			c.rgb = grayscale;
			
			o.Alpha = c.a;
			
			if (screenUV.x < _ScreenSize.x || screenUV.x > _ScreenSize.z)
			{
				o.Alpha = 0;
				discard;
			}
			if (screenUV.y < _ScreenSize.y || screenUV.y > _ScreenSize.w)
			{
				o.Alpha = 0;
				discard;
			}
			o.Alpha = c.a * _Color.a;
			o.Albedo = c.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
