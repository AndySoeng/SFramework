using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MultipleSpheresExample : MonoBehaviour {

    private Vector4[] centerPoints;
    //private Vector4[] AxisDirs;
    private float[] radiuses;

    public Transform box;

    public float radius = 1.2f;



	void Start () {
        centerPoints = new Vector4[64];
         radiuses = new float[64];
        //Shader.DisableKeyword("CLIP_PLANE");
        //Shader.SetGlobalVector("_SectionPoint", new Vector3(0, 0.62f,0));
        //Shader.SetGlobalVector("_SectionPlane", Vector3.up);
        //Shader.SetGlobalVector("_SectionCentre", box.position);
        Shader.SetGlobalVector("_SectionScale", box.localScale);
        //Shader.SetGlobalVector("_SectionDirX", box.right);
        //Shader.SetGlobalVector("_SectionDirY", box.up);
        //Shader.SetGlobalVector("_SectionDirZ", box.forward);
        Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
    }
	
	void LateUpdate () {
        int i = 0;
        foreach (Transform t in transform)
        {
            centerPoints[i] = t.position;
            radiuses[i] = radius;
            i++;
            if (i > 64) break;
        }
        Shader.SetGlobalVectorArray("_centerPoints", centerPoints);
        Shader.SetGlobalFloatArray("_Radiuses", radiuses);//*/
        Shader.SetGlobalInt("_centerCount", i);
      }

    void OnEnable()
    {
        Shader.EnableKeyword("CLIP_SPHERES");
    }

    void OnDisable()
    {
        Shader.DisableKeyword("CLIP_SPHERES");
        Shader.SetGlobalInt("_centerCount", 0);
    }

    void OnApplicationQuit()
    {
        //disable clipping so we could see the materials and objects in editor properly
        Shader.DisableKeyword("CLIP_SPHERES");

    }


}
