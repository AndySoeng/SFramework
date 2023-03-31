Shader "CrossSection/Empty" {

	Properties
	{
		// Material property declarations go here
	}
	SubShader
	{
		// The code that defines the rest of the SubShader goes here

		Pass
		{
		// The code that defines the Pass goes here
					CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

					#include "UnityCG.cginc"

					struct v2f
					{

					};

					v2f vert()
					{
						v2f o;
						return o;
					}

					fixed4 frag() : SV_Target
					{
						discard;
						return fixed4(1,1,1,1);
				}
				ENDCG
		}
	}

		Fallback "ExampleFallbackShader"
}
