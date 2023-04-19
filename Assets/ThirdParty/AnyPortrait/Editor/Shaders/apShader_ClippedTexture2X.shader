/*
*	Copyright (c) 2017-2018. RainyRizzle. All rights reserved
*	contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of AnyPortrait.
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of Seungjik Lee.
*/

Shader "AnyPortrait/Editor/Clipped Colored Texture (2X)"
{
	//Masked Colored Texture와 달리
	//Child가 렌더링을 하는 구조이다.
	//Parent가 미리 MaskRenderTexture를 만들어서 줘야한다.
	//Multipass는 아니다.
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)		// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Base Texture (RGBA)", 2D) = "white" {}				// Main Texture controlled by AnyPortrait
		_ScreenSize("Screen Size (xywh)", Vector) = (0, 0, 1, 1)		// ScreenSize for clipping in Editor
		_MaskRenderTexture("Mask Render Texture", 2D) = "clear" {}		// Mask Texture for Clipping
		_MaskColor("Mask Color (A)", Color) = (1, 1, 1, 1)				// Parent Mask Color
		_vColorITP("Vertex Color Ratio (0~1)", Range(0, 1)) = 0			// Vertex Color Interpolation Value for Weight Rendering
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane"}
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
				//float2 uv1 : TEXCOORD1;
				//float2 uv2 : TEXCOORD2;
				//float2 uv3 : TEXCOORD3;
				float4 screenPos : TEXCOORD4;
				//float4 worldPos : TEXCOORD5;
				//float3 vertColor : TEXCOORD6;
				
			};

			half4 _Color;
			sampler2D _MainTex;
			
			float4 _MainTex_ST;
			float4 _ScreenSize;
			
			sampler2D _MaskRenderTexture;

			float _vColorITP;
			float4 _MaskColor;

			vertexOutput vert(vertexInput IN)
			{
				vertexOutput o;
				//o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.pos = UnityObjectToClipPos(IN.vertex);

				/*float itp_black = saturate(1.0f - (IN.color.r + IN.color.g + IN.color.b));
				float itp_r = saturate(IN.color.r);
				float itp_g = saturate(IN.color.g);
				float itp_b = saturate(IN.color.b);*/

				o.color = IN.color;

				//o.vertColor = IN.vertColor;
				//o.vertColor = (itp_black * IN.vertColor_Z) + (itp_r * IN.vertColor_R) + (itp_g * IN.vertColor_G) + (itp_b * IN.vertColor_B);

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

				c.rgb *= _Color.rgb * 2;
				c.a *= _Color.a * maskTexture.a * _MaskColor.a;

				
				//Vert Color
				c.rgb += IN.color;
				c.rgb = IN.color * _vColorITP + c.rgb * (1.0f - _vColorITP);

				//c.r = 1;
				//c.a = 1;
				////Additive인 경우
				//c.rgb *= c.a;

				////Multiply인 경우
				//c.rgb = c.rgb * (c.a) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - c.a);

				return c;
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}
