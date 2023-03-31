#ifndef UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "section_clipping_CS.cginc"

//float  _MapScale;

//static const float4x4 projMatrix = float4x4(float4(1, 0, 0, 0), float4(0, 1, 0, 0), float4(0, 0, 1, 0), float4(0, 0, 0, 1));
static const float3x3 projMatrix = float3x3(float3(1, 0, 0), float3(0, 1, 0), float3(0, 0, 1));
struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
	float2 staticLightmapUV    : TEXCOORD1;
	float2 dynamicLightmapUV    : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv                       : TEXCOORD0;

	float3 positionWS                  : TEXCOORD1;    // xyz: posWS

#ifdef _NORMALMAP
	half4 normalWS                 : TEXCOORD2;    // xyz: normal, w: viewDir.x
	half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: viewDir.y
	half4 bitangentWS              : TEXCOORD4;    // xyz: bitangent, w: viewDir.z
#else
	half3  normalWS                : TEXCOORD2;
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	half4 fogFactorAndVertexLight  : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
	half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	float4 shadowCoord             : TEXCOORD6;
#endif

	DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

#ifdef DYNAMICLIGHTMAP_ON
	float2  dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
#endif

#ifdef _BOXMAP
	float3 positionOS : TEXCOORD9; 
	half3  normalOS : TEXCOORD10;
#endif

	float4 positionCS                  : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
	inputData = (InputData)0;

	inputData.positionWS = input.positionWS;

#ifdef _NORMALMAP
	half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
	inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
	inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
#else
	half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
	inputData.normalWS = input.normalWS;
#endif

	inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
	viewDirWS = SafeNormalize(viewDirWS);

	inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
	inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
	inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
	inputData.vertexLighting = half3(0, 0, 0);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
	inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
	inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
	inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

#if defined(DEBUG_DISPLAY)
#if defined(DYNAMICLIGHTMAP_ON)
	inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
#endif
#if defined(LIGHTMAP_ON)
	inputData.staticLightmapUV = input.staticLightmapUV;
#else
	inputData.vertexSH = input.vertexSH;
#endif
#endif

}

///////////////////////////////////////////////////////////////////////////////
//                            LIBRARY FUNCTIONS                              //
inline float3 ObjSpaceViewDir( in float4 v )
{
    float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
    return objSpaceCameraPos - v.xyz;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

#ifdef _BOXMAP
	//Calculate object scale
	float4 modelX = float4(1.0, 0.0, 0.0, 0.0);
	float4 modelY = float4(0.0, 1.0, 0.0, 0.0);
	float4 modelZ = float4(0.0, 0.0, 1.0, 0.0);

	float4 modelXInWorld = mul(UNITY_MATRIX_M, modelX);
	float4 modelYInWorld = mul(UNITY_MATRIX_M, modelY);
	float4 modelZInWorld = mul(UNITY_MATRIX_M, modelZ);

	float scaleX = length(modelXInWorld);
	float scaleY = length(modelYInWorld);
	float scaleZ = length(modelZInWorld);

	float3x3 _ScaleMatrix = float3x3(float3(scaleX, 0, 0), float3(0, scaleY, 0), float3(0, 0, scaleZ));

	output.positionOS = mul(_ScaleMatrix, input.positionOS.xyz);
	output.normalOS = input.normalOS;
#endif

    VertexPositionInputs vertexInput;
	
	#if SECTION_CLIPPING_ENABLED 

	if(_retractBackfaces==1)
	{
		float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
		float frontface = dot(input.normalOS, ObjSpaceViewDir(input.positionOS));
		if(frontface<0) 
		{
			float3 worldNorm = TransformObjectToWorldNormal(input.normalOS);
			worldPos -= worldNorm * _BackfaceExtrusion;
			vertexInput = GetVertexPositionInputs(mul(unity_WorldToObject, float4(worldPos, 1)).xyz);
		}
		else
		{
			vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
		}
	}
	else
	{
	#endif
		vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	#if SECTION_CLIPPING_ENABLED
	}
	#endif

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

#if defined(_FOG_FRAGMENT)
	half fogFactor = 0;
#else
	half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	output.positionWS.xyz = vertexInput.positionWS;
	output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
	half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
	output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
	output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
	output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
	output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
#endif

	OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
	output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
	OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
	output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
	output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

	return output;
}



// Used for StandardSimpleLighting shader
half4 LitPassFragmentSimple(Varyings input
#if SECTION_CLIPPING_ENABLED
, bool isFrontFace : SV_IsFrontFace
#endif
) : SV_Target
{
	SECTION_CLIP(input.positionWS);
	UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	SurfaceData surfaceData;
	InitializeSimpleLitSurfaceData(input.uv, surfaceData);

	InputData inputData;
	InitializeInputData(input, surfaceData.normalTS, inputData);
	SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
	ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

	half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
	color.rgb = MixFog(color.rgb, inputData.fogCoord);
	color.a = OutputAlpha(color.a, _Surface);
    #if SECTION_CLIPPING_ENABLED
	if(!isFrontFace)
	{
		color = _SectionColor;
	}
	#endif
    return color;
};


half4 LitPassFragmentSimpleTriplanar(Varyings input) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.uv;
	//half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
	//half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

	//half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));

	//InputData inputData;
	//InitializeInputData(input, normalTS, inputData);

	SurfaceData surfaceData;
	InitializeSimpleLitSurfaceData(input.uv, surfaceData);

	InputData inputData;
	InitializeInputData(input, surfaceData.normalTS, inputData);
	SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

	float3 projPos;
	float3 projNorm;
#ifdef _BOXMAP
	projPos = input.positionOS;
	projNorm = input.normalOS;
#else
	projPos = mul(projMatrix, inputData.positionWS.xyz);
	projNorm = mul(projMatrix, inputData.normalWS.xyz);
#endif

	half2 UV;
	UV = projPos.zy * _MapScale; // side
	half4 cx = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y)); // use WALLSIDE texture
	UV = projPos.xy * _MapScale; // front
	half4 cz = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y)); // use WALL texture
	UV = projPos.xz * _MapScale; // top
	half4 cy = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y)); // use FLR texture

	//generate weights from world normals
	float3 weights = projNorm;
	//show texture on both sides of the object (positive and negative)
	weights = abs(weights);

	//make it so the sum of all components is 1
	weights = weights / (weights.x + weights.y + weights.z);

	half4 diffuseAlpha = cx* weights.x + cy * weights.y + cz * weights.z;
	half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

	half alpha = diffuseAlpha.a * _BaseColor.a;
	AlphaDiscard(alpha, _Cutoff);
#ifdef _ALPHAPREMULTIPLY_ON
	diffuse *= alpha;
#endif
	half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
	half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
	half smoothness = specular.a;
	half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha, surfaceData.normalTS);
	color.rgb = MixFog(color.rgb, inputData.fogCoord);
	return color;
};

#endif
