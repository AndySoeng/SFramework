using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//set keywords permanent in editor
[ExecuteInEditMode]
public class PlaneSectionEditor : MonoBehaviour {

	void Start () {
        Shader.EnableKeyword("CLIP_PLANE");
    }

    void OnEnable()
    {
        Shader.EnableKeyword("CLIP_PLANE");
    }

    void OnDisable()
    {
        Shader.DisableKeyword("CLIP_PLANE");
    }

    void OnApplicationQuit()
    {
        //disable clipping so we could see the materials and objects in editor properly
        Shader.DisableKeyword("CLIP_PLANE");
    }

}
