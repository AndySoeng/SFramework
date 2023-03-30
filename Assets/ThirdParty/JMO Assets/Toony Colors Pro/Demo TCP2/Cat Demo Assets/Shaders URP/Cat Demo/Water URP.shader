// Toony Colors Pro+Mobile 2
// (c) 2014-2019 Jean Moreno

// This shader has been written manually, the Water Template is not yet available for URP

Shader "Toony Colors Pro 2/Examples URP/Cat Demo/Water"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_BaseColor ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		_BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.1
		[TCP2Separator]
		
		[TCP2HeaderHelp(Rim Lighting)]
		[TCP2ColorNoAlpha] _RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.5)
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1
		[TCP2Separator]
		
		[Header(Depth Color)]
		_DepthColor ("Depth Color", Color) = (0.5,0.5,0.5,1.0)
		[PowerSlider(5.0)] _DepthDistance ("Depth Distance", Range(0.01,3)) = 0.5

		[Header(Foam)]
		_FoamSpread ("Foam Spread", Range(0.01,5)) = 2
		_FoamStrength ("Foam Strength", Range(0.01,1)) = 0.8
		_FoamColor ("Foam Color (RGB) Opacity (A)", Color) = (0.9,0.9,0.9,1.0)
		[NoScaleOffset]
		_FoamTex ("Foam (RGB)", 2D) = "white" {}
		_FoamSmooth ("Foam Smoothness", Range(0,0.5)) = 0.02
		_FoamSpeed ("Foam Speed", Vector) = (2,2,2,2)

		[Header(Vertex Waves Animation)]
		_WaveSpeed ("Speed", Float) = 2
		_WaveHeight ("Height", Float) = 0.1
		_WaveFrequency ("Frequency", Range(0,10)) = 1

		[Header(UV Waves Animation)]
		_UVWaveSpeed ("Speed", Float) = 1
		_UVWaveAmplitude ("Amplitude", Range(0.001,0.5)) = 0.05
		_UVWaveFrequency ("Frequency", Range(0,10)) = 1
		
		[TCP2Separator]
		
		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff ("Receive Shadows", Float) = 1

		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Transparent"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		ENDHLSL
		Pass
		{
			Name "Main"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 3.0

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			// -------------------------------------
			// Material keywords
			//#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			// -------------------------------------

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex Vertex
			#pragma fragment Fragment

			// Uniforms
			CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			sampler2D _BaseMap;
			float4 _BaseMap_ST;
			fixed4 _BaseColor;
			float _RampThreshold;
			float _RampSmoothing;
			fixed3 _SColor;
			fixed3 _HColor;
			float _RimMin;
			float _RimMax;
			fixed3 _RimColor;
			fixed4 _DepthColor;
			half _DepthDistance;
			half4 _FoamSpeed;
			half _FoamSpread;
			half _FoamStrength;
			sampler2D _FoamTex;
			fixed4 _FoamColor;
			half _FoamSmooth;
			half _WaveHeight;
			half _WaveFrequency;
			half _WaveSpeed;
			half _UVWaveAmplitude;
			half _UVWaveFrequency;
			half _UVWaveSpeed;
			CBUFFER_END

			TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

			// vertex input
			struct Attributes
			{
				float4 vertex       : POSITION;
				float3 normal       : NORMAL;
				float4 tangent      : TANGENT;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// vertex output / fragment input
			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 worldPosAndFog : TEXCOORD0;
			#ifdef _MAIN_LIGHT_SHADOWS
				float4 shadowCoord    : TEXCOORD1; // compute shadow coord per-vertex for the main light
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				half3 vertexLights : TEXCOORD2;
			#endif
				float2 pack0 : TEXCOORD3; /* pack0.xy = texcoord0 */
				// water
				float2 sinAnim : TEXCOORD4;
				float4 sPos : TEXCOORD5;
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

				// Texture Coordinates
				output.pack0.xy = (vertexInput.positionWS.xz * 0.1) * _BaseMap_ST.xy + _BaseMap_ST.zw;

			#ifdef _MAIN_LIGHT_SHADOWS
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif

				// world position
				output.worldPosAndFog = float4(vertexInput.positionWS.xyz, 0);

				// Water
				half2 x = ((input.vertex.xy + input.vertex.yz) * _UVWaveFrequency) + (_Time.yy * _UVWaveSpeed);
				output.sinAnim = x;

				//vertex waves
				float3 _pos = vertexInput.positionWS.xyz * _WaveFrequency;
				float _phase = _Time.y * _WaveSpeed;
				half4 vsw_offsets = half4(1.0, 2.2, 0.6, 1.3);
				half4 vsw_ph_offsets = half4(1.0, 1.3, 2.2, 0.4);
				half4 waveXZ = sin((_pos.xxzz * vsw_offsets) + (_phase.xxxx * vsw_ph_offsets));
				float waveFactorX = dot(waveXZ.xy, 1) * _WaveHeight / 2;
				float waveFactorZ = dot(waveXZ.zw, 1) * _WaveHeight / 2;
				input.vertex.y += (waveFactorX + waveFactorZ);
				half4 waveXZn = cos((_pos.xxzz * vsw_offsets) + (_phase.xxxx * vsw_ph_offsets)) * (vsw_offsets / 2);
				float xn = -_WaveHeight * (waveXZn.x + waveXZn.y);
				float zn = -_WaveHeight * (waveXZn.z + waveXZn.w);
				input.normal = normalize(float3(xn, 1, zn));

				// normal
				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normal);
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				// Vertex lighting
				output.vertexLights = VertexLighting(vertexInput.positionWS, vertexNormalInput.normalWS);
			#endif
				output.normal = NormalizeNormalPerVertex(vertexNormalInput.normalWS);

				vertexInput = GetVertexPositionInputs(input.vertex.xyz);

				// clip position
				output.positionCS = vertexInput.positionCS;

				// screen-space pos
				//output.sPos = ComputeScreenPos(vertexInput.positionCS);
				output.sPos = vertexInput.positionNDC;

				return output;
			}

			half4 Fragment(Varyings input) : SV_Target
			{
				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = NormalizeNormalPerPixel(input.normal);
				half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - positionWS);

				half2 uvDistort = ((sin(0.9*input.sinAnim.xy) + sin(1.33*input.sinAnim.xy+3.14) + sin(2.4*input.sinAnim.xy+5.3))/3) * _UVWaveAmplitude;
				input.pack0.xy += uvDistort.xy;

				// Shader Properties Sampling
				float4 __albedo = ( tex2D(_BaseMap, (input.pack0.xy)).aaaa );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float __rimMin = ( _RimMin );
				float __rimMax = ( _RimMax );
				float3 __rimColor = ( _RimColor.rgb );
				float __rimStrength = ( 1.0 );
				float __ambientIntensity = ( 1.0 );

				half ndv = max(0, dot(viewDirWS, normalWS));
				half ndvRaw = ndv;

				// main texture
				half3 albedo = lerp(1, __mainColor.rgb, __albedo.r);
				half alpha = __alpha;
				half3 emission = half3(0,0,0);

				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.sPos.xy / input.sPos.w);
				if (unity_OrthoParams.w > 0)
				{
					// orthographic camera
				#if defined(UNITY_REVERSED_Z)
					sceneZ = 1.0f - sceneZ;
				#endif
					sceneZ = (sceneZ * _ProjectionParams.z) + _ProjectionParams.y;
				}
				else
				{
					// perspective camera
					sceneZ = LinearEyeDepth(sceneZ, _ZBufferParams);
				}

				float partZ = LinearEyeDepth(positionWS, UNITY_MATRIX_V);
				float depthDiff = abs(sceneZ - partZ);
				depthDiff *= ndv * 2;
				//Depth-based foam
				half2 foamUV = input.pack0.xy;
				foamUV.xy += _Time.yy*_FoamSpeed.xy*0.05;
				fixed4 foam = tex2D(_FoamTex, foamUV);
				foamUV.xy += _Time.yy*_FoamSpeed.zw*0.05;
				fixed4 foam2 = tex2D(_FoamTex, foamUV);
				foam = (foam + foam2) / 2;
				float foamDepth = saturate(_FoamSpread * depthDiff);
				half foamTerm = (smoothstep(foam.r - _FoamSmooth, foam.r + _FoamSmooth, saturate(_FoamStrength - foamDepth)) * saturate(1 - foamDepth)) * _FoamColor.a;
				//Alter color based on depth buffer (soft particles technique)
				albedo = lerp(_DepthColor.rgb, albedo.rgb, saturate(_DepthDistance * depthDiff));	//N.V corrects the result based on view direction (depthDiff tends to not look consistent depending on view angle)));
				albedo = lerp(albedo.rgb, _FoamColor.rgb, foamTerm);

				// main light: direction, color, distanceAttenuation, shadowAttenuation
			#ifdef _MAIN_LIGHT_SHADOWS
				Light mainLight = GetMainLight(input.shadowCoord);
			#else
				Light mainLight = GetMainLight();
			#endif

				half3 lightDir = mainLight.direction;
				half3 lightColor = mainLight.color.rgb;
				half atten = mainLight.shadowAttenuation;

				half ndl = max(0, dot(normalWS, lightDir));
				half3 ramp;
				half rampThreshold = __rampThreshold;
				half rampSmooth = __rampSmoothing * 0.5;
				ndl = saturate(ndl);
				ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);
				fixed3 rampGrayscale = ramp;

				// apply attenuation
				ramp *= atten;

				// highlight/shadow colors
				ramp = lerp(__shadowColor, __highlightColor, ramp);

				// output color
				half3 color = half3(0,0,0);
				// Rim Lighting
				half rim = 1 - ndvRaw;
				half rimMin = __rimMin;
				half rimMax = __rimMax;
				rim = smoothstep(rimMin, rimMax, rim);
				half3 rimColor = __rimColor;
				half rimStrength = __rimStrength;
				color.rgb += rim * rimColor * rimStrength;
				color += albedo * lightColor.rgb * ramp;

				// Additional lights loop
			#ifdef _ADDITIONAL_LIGHTS
				int additionalLightsCount = GetAdditionalLightsCount();
				for (int i = 0; i < additionalLightsCount; ++i)
				{
					Light light = GetAdditionalLight(i, positionWS);
					half atten = light.shadowAttenuation * light.distanceAttenuation;
					half3 lightDir = light.direction;
					half3 lightColor = light.color.rgb;

					half ndl = max(0, dot(normalWS, lightDir));
					half3 ramp;
					ndl = saturate(ndl);
					ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

					// apply attenuation (shadowmaps & point/spot lights attenuation)
					ramp *= atten;

					// apply highlight color
					ramp = lerp(half3(0,0,0), __highlightColor, ramp);

					// output color
					color += albedo * lightColor.rgb * ramp;

				}
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				color += input.vertexLights * albedo;
			#endif

				// ambient or lightmap
			#ifdef LIGHTMAP_ON
				// Normal is required in case Directional lightmaps are baked
				half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
			#else
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
			#endif
				half occlusion = 1;
				half3 indirectDiffuse = bakedGI;
				indirectDiffuse *= occlusion * albedo * __ambientIntensity;
				color += indirectDiffuse;

				color += emission;

				return half4(color, alpha);
			}
			ENDHLSL
		}

		// Depth & Shadow Caster Passes
		HLSLINCLUDE
		#if defined(SHADOW_CASTER_PASS) || defined(DEPTH_ONLY_PASS)

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			float3 _LightDirection;

			CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			sampler2D _BaseMap;
			float4 _BaseMap_ST;
			fixed4 _BaseColor;
			float _RampThreshold;
			float _RampSmoothing;
			fixed3 _SColor;
			fixed3 _HColor;
			float _RimMin;
			float _RimMax;
			fixed3 _RimColor;
			half _WaveHeight;
			half _WaveFrequency;
			half _WaveSpeed;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float2 pack0 : TEXCOORD0; /* pack0.xy = texcoord0 */
			#if defined(DEPTH_ONLY_PASS)
				UNITY_VERTEX_OUTPUT_STEREO
			#endif
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

			#if UNITY_REVERSED_Z
				positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#else
				positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#endif

				return positionCS;
			}

			Varyings ShadowDepthPassVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
			#if defined(DEPTH_ONLY_PASS)
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			#endif
				// Texture Coordinates
				output.pack0.xy = (input.texcoord0.xy) * _BaseMap_ST.xy + _BaseMap_ST.zw;

				//vertex waves
				float3 worldPos = mul(unity_ObjectToWorld, input.positionOS).xyz;
				float3 _pos = worldPos.xyz * _WaveFrequency;
				float _phase = _Time.y * _WaveSpeed;
				half4 vsw_offsets = half4(1.0, 2.2, 0.6, 1.3);
				half4 vsw_ph_offsets = half4(1.0, 1.3, 2.2, 0.4);
				half4 waveXZ = sin((_pos.xxzz * vsw_offsets) + (_phase.xxxx * vsw_ph_offsets));
				float waveFactorX = dot(waveXZ.xy, 1) * _WaveHeight / 2;
				float waveFactorZ = dot(waveXZ.zw, 1) * _WaveHeight / 2;
				input.positionOS.y += (waveFactorX + waveFactorZ);

			#if defined(DEPTH_ONLY_PASS)
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
			#elif defined(SHADOW_CASTER_PASS)
				output.positionCS = GetShadowPositionHClip(input);
			#endif

				return output;
			}

			half4 ShadowDepthPassFragment(Varyings input) : SV_TARGET
			{
				// Shader Properties Sampling
				float4 __albedo = ( tex2D(_BaseMap, (input.pack0.xy)).aaaa );
				float4 __mainColor = ( _BaseColor.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );
				float __rimMin = ( _RimMin );
				float __rimMax = ( _RimMax );
				float3 __rimColor = ( _RimColor.rgb );
				float __rimStrength = ( 1.0 );
				float __ambientIntensity = ( 1.0 );

				half3 albedo = __albedo.rgb;
				half alpha = __alpha;
				half3 emission = half3(0,0,0);
				return 0;
			}

		#endif
		ENDHLSL

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile SHADOW_CASTER_PASS

			// -------------------------------------
			// Material Keywords
			//#pragma shader_feature _ALPHATEST_ON
			//#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			// #pragma shader_feature _ALPHATEST_ON
			// #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

		// Depth prepass
		// UsePass "Universal Render Pipeline/Lit/DepthOnly"

		// Used for Baking GI. This pass is stripped from build.
		UsePass "Universal Render Pipeline/Lit/Meta"
	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.4.0";tmplt:"SG2_Template_URP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","TEMPLATE_URP","RIM","SHADOW_COLOR_MAIN_DIR"];flags:list[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",RIM_LABEL="Rim Lighting"];shaderProperties:list[sp(name:"Albedo";imps:list[imp_mp_texture(uto:True;tov:"";gto:True;sbt:False;scr:False;scv:"";gsc:False;roff:False;goff:False;notile:False;def:"white";locked_uv:False;uv:0;cc:4;chan:"AAAA";mip:-1;mipprop:False;ssuv:False;ssuv_vert:False;ssuv_obj:False;prop:"_BaseMap";md:"";op:Multiply;lbl:"Albedo";gpu_inst:False;locked:False;impl_index:0)])];customTextures:list[]) */
/* TCP_HASH 07bca1a53adafc9a6bf8d4de89a9bf76 */
