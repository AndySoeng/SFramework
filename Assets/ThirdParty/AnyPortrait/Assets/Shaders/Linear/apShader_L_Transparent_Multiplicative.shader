/*
*	Copyright (c) 2017-2022. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee].
*
*	Unless this file is downloaded from the Unity Asset Store or RainyRizzle homepage,
*	this file and its users are illegal.
*	In that case, the act may be subject to legal penalties.
*/



Shader "AnyPortrait/Transparent/Linear/Colored Texture (2X) Multiplicative"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}				// Main Texture controlled by AnyPortrait
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend DstColor SrcColor//2X Multiply
		//Cull Off//<<?

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf SimpleColor alpha
		#pragma surface surf SimpleColor finalcolor:alphaCorrection noforwardadd//AlphaBlend가 아닌경우

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

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			//c.rgb *= _Color.rgb * 2.0f;
			c.rgb *= _Color.rgb * 4.595f;//Linear : pow(2, 2.2) = 4.595

			o.Alpha = c.a * _Color.a;
			//o.Albedo = c.rgb;
			
			//Multiply 식
			//o.Albedo = pow(c.rgb, 2.2f) * (o.Alpha) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - o.Alpha);//Linear
			o.Albedo = pow(c.rgb, 2.2f);
		}

		void alphaCorrection(Input IN, SurfaceOutput o, inout half4 color)
		{
			color.rgb = color.rgb * (color.a) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - color.a);
			color.a = 1.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
