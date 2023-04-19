/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

Shader "AnyPortrait/Editor/Linear/Clipped Colored Texture ToneColor (2X)"
{
	//Masked Colored Texture와 달리
	//Child가 렌더링을 하는 구조이다.
	//Parent가 미리 MaskRenderTexture를 만들어서 줘야한다.
	//Multipass는 아니다.
	Properties
	{
		_Color("2X Tone Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}					// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)			// ScreenSize for clipping in Editor
		_MaskRenderTexture("Mask Render Texture", 2D) = "clear" {}			// Mask Texture for Clipping
		_MaskColor("Mask Color (A)", Color) = (1, 1, 1, 1)					// Parent Mask Color
		//_vColorITP("Vertex Color Ratio (0~1)", Range(0, 1)) = 0
		_Thickness("Thickness (0~1)", Range(0, 1)) = 0.5
		_ShapeRatio("ShapeRatio(0 : Outline / 1 : Solid)", Range(0, 1)) = 0
		_PosOffsetX("PosOffsetX", Float) = 0
		_PosOffsetY("PosOffsetY", Float) = 0
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
			//ZTest Always
			//Cull Off

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
				//float3 vertColor : TEXCOORD1;
			};

			struct vertexOutput
			{
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD4;
				
				
			};

			half4 _Color;
			sampler2D _MainTex;
			
			float4 _MainTex_ST;
			float4 _ScreenSize;
			float _Thickness;
			fixed _ShapeRatio;
			float _PosOffsetX;
			float _PosOffsetY;
			
			sampler2D _MaskRenderTexture;

			float4 _MaskColor;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				//o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.pos = UnityObjectToClipPos(IN.vertex + float4(_PosOffsetX, _PosOffsetY, 0, 0));

				//o.pos.x += _PosOffsetX;
				//o.pos.y += _PosOffsetY;

				o.color = IN.color;

				o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
				o.screenPos = ComputeScreenPos(o.pos);
				//o.worldPos = o.pos;
				return o;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);

				float2 screenUV = IN.screenPos.xy;// *_ScreenParams.xy;
				float2 clipScreenUV = screenUV;// *_ScreenParams.xy;
				clipScreenUV.y = 1.0f - clipScreenUV.y;
				//float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				//screenUV.y = 1.0f - screenUV.y;

				half4 maskTexture = tex2D(_MaskRenderTexture, screenUV);

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

				float sampleDigonal = (0.005f * (1.0f - _Thickness)) + (0.015f * _Thickness);//기본 0.01
				float sampleCross = (0.005f * (1.0f - _Thickness)) + (0.015f * _Thickness);//기본 0.015

				half a_0 = tex2D(_MainTex, IN.uv + float2(sampleDigonal, sampleDigonal)).a;
				half a_1 = tex2D(_MainTex, IN.uv + float2(-sampleDigonal, sampleDigonal)).a;
				half a_2 = tex2D(_MainTex, IN.uv + float2(sampleDigonal, -sampleDigonal)).a;
				half a_3 = tex2D(_MainTex, IN.uv + float2(-sampleDigonal, -sampleDigonal)).a;

				half a_4 = tex2D(_MainTex, IN.uv + float2(sampleCross, 0)).a;
				half a_5 = tex2D(_MainTex, IN.uv + float2(0, sampleCross)).a;
				half a_6 = tex2D(_MainTex, IN.uv + float2(-sampleCross, 0)).a;
				half a_7 = tex2D(_MainTex, IN.uv + float2(0, -sampleCross)).a;

				half outlineItp = 1 - ((a_0 + a_1 + a_2 + a_3 + a_4 + a_5 + a_6 + a_7) / 8.0f); // 0~1 => 0.2 ~ 1 
				outlineItp = (outlineItp * 0.8f) + 0.2f;


				float grayScale = c.r * 0.3f + c.g * 0.6f + c.b * 0.1f;
				c.rgb = grayScale;//<<GrayScale
				c.rgb *= 2.0f;

				//c.rgb *= _Color.rgb * 2;
				c.rgb *= _Color.rgb * 4.595f;//Linear
				
				//c.a *= _Color.a * maskTexture.a * _MaskColor.a * outlineItp;
				c.a = c.a * _Color.a * maskTexture.a * (outlineItp * (1 - _ShapeRatio) + _ShapeRatio);

				
				return c;
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}
