using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]

public class SetCustomDepthNormalsShader : MonoBehaviour
{
    public Shader customShader;
	// Use this for initialization
	void Start () {
        GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseCustom);
        GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, customShader); 
	}

    void OnEnable()
    {
        GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseCustom);
        GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, customShader);
    }
    void OnDisable()
    {
        GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, customShader);
    }
	
}
