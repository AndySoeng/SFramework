Shader "Unlit/DoubleSide"
{
	Properties
	{
	    _Color ("MainColor", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Opaque"="Transparent" }
		LOD 100

		Material

        {

            Diffuse [_Color]

            Ambient (1,1,1,1)

        }

        Pass
    
        {
    
    
            Lighting On
    
            Cull off
    
            Blend SrcAlpha OneMinusSrcAlpha
    
    
            SetTexture [_MainTex]
            {
    
                constantColor [_Color]
    
                Combine texture * primary DOUBLE, texture * constant
    
            }

        }
	}
	FallBack "Diffuse", 1
}
