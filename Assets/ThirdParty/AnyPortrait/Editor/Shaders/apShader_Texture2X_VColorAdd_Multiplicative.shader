/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/


Shader "AnyPortrait/Editor/Colored Texture VColor Add(2X) Multiplicative"
{
	Properties
	{	
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Albedo (RGBA)", 2D) = "white" {}						// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
		_vColorITP("Vertex Color Ratio (0~1)", Range(0, 1)) = 0			// Vertex Color Interpolation Value for Weight Rendering
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One//Add
		//Blend OneMinusDstColor One//Soft Add
		//Blend DstColor Zero//Multiply
		Blend DstColor SrcColor//2X Multiply
		
		//Cull Off

		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf SimpleColor alpha //<<AlphaBlend인 경우
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
		float4 _ScreenSize;
		float _vColorITP;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
			float4 screenPos;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV.y = 1.0f - screenUV.y;

			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb *= _Color.rgb * 2.0f;
			c.rgb += IN.color;
			
			c.rgb = IN.color * _vColorITP + c.rgb * (1.0f - _vColorITP);

			
			//o.Alpha = c.a * _Color.a * pow(IN.color.a, 0.1);
			o.Alpha = c.a * _Color.a;
			//o.Alpha = _Color.a;

			
			/*if (screenUV.x < 0.01f)
			{
				c.r = screenUV.x;
				c.g = screenUV.y;
				c.b *= 0.1f;
			}
			if (screenUV.x > 0.99f)
			{
				c.r = screenUV.x;
				c.g = screenUV.y;
				c.b *= 0.1f;
			}*/

			/*if (screenUV.y > 0.95f)
			{
				c.b = 1;
			}
			if (screenUV.y < 0.05f)
			{
				c.b = 1;
			}*/

			
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
			//Additive라면 RGB * Alpha를 Albedo에 넣어야한다.
			//o.Albedo = c.rgb;
			//o.Albedo = c.rgb * o.Alpha;
			
			//Multiply 식
			o.Albedo = c.rgb * (o.Alpha) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - o.Alpha);
		}
		ENDCG
		}
		FallBack "Diffuse"
}
