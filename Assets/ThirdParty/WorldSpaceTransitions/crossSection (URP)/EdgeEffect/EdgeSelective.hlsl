TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
float4 _CameraColorTexture_TexelSize;

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);

//possibly to do: create depth texture copy in MaskFeature(ScriptableRendererFeature)
//TEXTURE2D(_CameraDepthTexture);//possibly to do: create depth texture copy in MaskFeature(ScriptableRendererFeature)
//SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_EdgeMap);
SAMPLER(sampler_EdgeMap);

float3 DecodeNormal(float4 enc)
{
    float kScale = 1.7777;
    float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
    float g = 2.0 / dot(nn.xyz,nn.xyz);
    float3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}

inline float DecodeFloatRG(float2 enc)
{
	float2 kDecodeDot = float2(1.0, 1 / 255.0);
	return dot(enc, kDecodeDot);
}

inline float3 DecodeViewNormal(float4 enc4)
{
	float kScale = 1.7777;
	float3 nn = enc4.xyz*float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
	float g = 2.0 / dot(nn.xyz, nn.xyz);
	float3 n;
	n.xy = g * nn.xy;
	n.z = g - 1;
	return n;
}


void DecodeDepthNormal(float4 enc, bool masked, out float depth, out float3 normal)
{
	if (masked) 
	{
		normal = float3(0, 0, 1);
		depth = 1.0;
		return;
	}
	depth = DecodeFloatRG(enc.zw);
	normal = DecodeViewNormal(enc);
}

void Outline_float(float2 UV, float OutlineThickness,
#if defined(ALL_EDGES)
float DepthSensitivity, float NormalsSensitivity, float ColorSensitivity, half4 OutlineColor, float MaskSensitivity, 
#endif
float BackfaceSensitivity, half4 BackfaceOutlineColor, 
out half4 Out)

{
    float halfScaleFloor = floor(OutlineThickness * 0.5);
    float halfScaleCeil = ceil(OutlineThickness * 0.5);
    float2 Texel = (1.0) / float2(_CameraColorTexture_TexelSize.z, _CameraColorTexture_TexelSize.w);

    float2 uvSamples[4];
#if defined(ALL_EDGES)
	float depthSamples[4], maskSamples[4];
    float3 normalSamples[4], colorSamples[4];
#endif	
	float backfaceSamples[4];

    uvSamples[0] = UV - float2(Texel.x, Texel.y) * halfScaleFloor;
    uvSamples[1] = UV + float2(Texel.x, Texel.y) * halfScaleCeil;
    uvSamples[2] = UV + float2(Texel.x * halfScaleCeil, -Texel.y * halfScaleFloor);
    uvSamples[3] = UV + float2(-Texel.x * halfScaleFloor, Texel.y * halfScaleCeil);

    for(int i = 0; i < 4 ; i++)
    {
		backfaceSamples[i] = SAMPLE_TEXTURE2D(_EdgeMap, sampler_EdgeMap, uvSamples[i]).y;
#if defined(ALL_EDGES)
		maskSamples[i] = SAMPLE_TEXTURE2D(_EdgeMap, sampler_EdgeMap, uvSamples[i]).x;
		float4 samples = maskSamples[i]*SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uvSamples[i]);//
		bool masked = (backfaceSamples[i]>0.5);

		//depthSamples[i] = maskSamples[i] * SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvSamples[i]).r;
		//normalSamples[i] = masked ? float3(0, 0, 1) : DecodeNormal(maskSamples[i] * SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uvSamples[i]));
		DecodeDepthNormal(samples, masked, depthSamples[i], normalSamples[i]);//

        colorSamples[i] = maskSamples[i]*SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uvSamples[i]).xyz;
#endif	
    }
#if defined(ALL_EDGES)
    // Depth
    float depthFiniteDifference0 = depthSamples[1] - depthSamples[0];
    float depthFiniteDifference1 = depthSamples[3] - depthSamples[2];
    float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
    float depthThreshold = (1/DepthSensitivity) * depthSamples[0];
    edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

    // Normals
    float3 normalFiniteDifference0 = normalSamples[1] - normalSamples[0];
    float3 normalFiniteDifference1 = normalSamples[3] - normalSamples[2];
    float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
    edgeNormal = edgeNormal > (1/NormalsSensitivity) ? 1 : 0;

    // Color
    float3 colorFiniteDifference0 = colorSamples[1] - colorSamples[0];
    float3 colorFiniteDifference1 = colorSamples[3] - colorSamples[2];
    float edgeColor = sqrt(dot(colorFiniteDifference0, colorFiniteDifference0) + dot(colorFiniteDifference1, colorFiniteDifference1));
	edgeColor = edgeColor > (1/ColorSensitivity) ? 1 : 0;

	// mask
	float maskFiniteDifference0 = maskSamples[1] - maskSamples[0];
	float maskFiniteDifference1 = maskSamples[3] - maskSamples[2];
	float edgeMask = sqrt(pow(maskFiniteDifference0, 2) + pow(maskFiniteDifference1, 2))*10;
	float maskThreshold = (1 / MaskSensitivity) * maskSamples[0];
	edgeMask = edgeMask > maskThreshold ? 1 : 0;
#endif
	// backfaceMask
	float backfaceMaskFiniteDifference0 = backfaceSamples[1] - backfaceSamples[0];
	float backfaceMaskFiniteDifference1 = backfaceSamples[3] - backfaceSamples[2];
	float edgeBackface = sqrt(pow(backfaceMaskFiniteDifference0, 2) + pow(backfaceMaskFiniteDifference1, 2)) * 10;
	float edgeBackfaceThreshold = (1 / BackfaceSensitivity) * backfaceSamples[0];
	edgeBackface = edgeBackface > edgeBackfaceThreshold ? 1 : 0;

#if !defined(ALL_EDGES)
	float edge = edgeBackface;
	half4 Color = BackfaceOutlineColor;
#endif
#if defined(ALL_EDGES)
    float edge = max(edgeMask, max(edgeDepth, max(edgeNormal, edgeColor)));
	half4 Color = edgeBackface > 0.1*edge ? BackfaceOutlineColor : OutlineColor;
	edge = max(edge, edgeBackface);
#endif
    half4 original = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uvSamples[0]);	
	//half4 original = SAMPLE_TEXTURE2D(_ScreenCopyTexture, sampler_ScreenCopyTexture, uvSamples[0]);
    Out = ((1 - edge) * original) + (edge * lerp(original, Color,  Color.a));
}


