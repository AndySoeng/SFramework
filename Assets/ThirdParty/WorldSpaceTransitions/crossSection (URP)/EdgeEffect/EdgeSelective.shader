Shader "CrossSectionURP/EdgeSelective"
{
    Properties
    {
		_depthSensitivity("DepthSensitivity", Range(0.0, 1.0)) = 0.0
		_normalsSensitivity("NormalsSensitivity", Range(0.0, 10.0)) = 0.0
		_colorSensitivity("ColorSensitivity", Range(0.0, 10.0)) = 0.0
		_maskSensitivity("maskSensitivity", Range(0.0, 10.0)) = 0.0
		_backfaceSensitivity("backfaceSensitivity", Range(0.0, 10.0)) = 0.0
		_edgeColor("edgeColor", Color) = (1, 1, 1, 1)
		_backfaceEdgeColor("backfaceEdgeColor", Color) = (1, 0, 0, 1)
		_outlineThickness("EdgeThickness", Range(0.0, 4.0)) = 1.0
		[Toggle(ALL_EDGES)] _all_edges("all edges", Float) = 1

	}
		SubShader
		{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		LOD 100

			ZTest Always

			Pass
			{
				Name "Unlit"
				ZTest Always
				ZWrite Off
				Cull Off
				HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma shader_feature ALL_EDGES
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
			#include "EdgeSelective.hlsl"

			float _depthSensitivity;
			float _normalsSensitivity;
			float _colorSensitivity;
			float _maskSensitivity;
			float _backfaceSensitivity;
			float _outlineThickness;
			half4 _edgeColor;
			half4 _backfaceEdgeColor;

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                //output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				output.uv = input.uv;
                //output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;

				half4 _out;
				Outline_float(uv, _outlineThickness, 
#if defined(ALL_EDGES)
					_depthSensitivity, _normalsSensitivity, _colorSensitivity, _edgeColor,_maskSensitivity, 
#endif	
					_backfaceSensitivity, _backfaceEdgeColor, _out);

				return _out;
            }
            ENDHLSL
        }

    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
