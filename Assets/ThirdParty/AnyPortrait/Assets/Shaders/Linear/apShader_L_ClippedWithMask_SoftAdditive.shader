﻿/*
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

Shader "AnyPortrait/Transparent/Linear/Clipped With Mask (2X) SoftAdditive"
{
	Properties
	{	
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}				// Main Texture controlled by AnyPortrait
		_MaskTex("Mask Texture 1 (A)", 2D) = "white" {}					// Mask Texture for clipping Rendering (controlled by AnyPortrait)
		_MaskScreenSpaceOffset("Mask Screen Space Offset (XY_Scale)", Vector) = (0, 0, 0, 1)	// Mask Texture's Transform Offset (controlled by AnyPortrait)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane"}
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend OneMinusDstColor One//Soft Add
		LOD 200

		CGPROGRAM
		//#pragma surface surf SimpleColor alpha
		#pragma surface surf SimpleColor//AlphaBlend가 아닌 경우

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
		sampler2D _MaskTex;
		float4 _MaskScreenSpaceOffset;

		struct Input
		{
			float2 uv_MainTex;
			float4 screenPos;
			float4 color : COLOR;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			//c.rgb *= _Color.rgb * 2.0f;
			c.rgb *= _Color.rgb * 4.595f;//Linear : pow(2, 2.2) = 4.595

			float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w, 0.0001f);
			//float2 screenUVNoEdit = screenUV;
			
			screenUV -= float2(0.5f, 0.5f);

			screenUV.x *= _MaskScreenSpaceOffset.z;
			screenUV.y *= _MaskScreenSpaceOffset.w;

			screenUV.x += _MaskScreenSpaceOffset.x * _MaskScreenSpaceOffset.z;
			screenUV.y += _MaskScreenSpaceOffset.y * _MaskScreenSpaceOffset.w;

			
			screenUV += float2(0.5f, 0.5f);

			float mask = tex2D(_MaskTex, screenUV).r;
			//float maskNoEdit = tex2D(_MaskTex, screenUVNoEdit).r;
			/*c.r = mask;
			c.g = maskNoEdit;
			c.b = 0;
			c.a = 1;*/
			c.a *= mask;

			o.Alpha = c.a * _Color.a;
			
			//Additive라면 RGB * Alpha를 Albedo에 넣어야한다.
			//o.Albedo = c.rgb;
			//o.Albedo = c.rgb * o.Alpha;
			o.Albedo = pow(c.rgb, 2.2f) * o.Alpha;//Linear
		}
		ENDCG
	}
}
