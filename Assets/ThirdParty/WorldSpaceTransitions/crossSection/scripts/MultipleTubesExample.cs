using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MultipleTubesExample : MonoBehaviour {

    private Vector4[] centerPoints;
    private Vector4[] AxisDirs;
    private float[] radiuses;
    [Range(0, 64)]
    public int n = 5;

    private int i = 0;


	void Start () {
        centerPoints = new Vector4[64];
        AxisDirs = new Vector4[64];
        radiuses = new float[64];
       // Shader.DisableKeyword("CLIP_PLANE");
       //Shader.DisableKeyword("CLIP_PIE");
       // Shader.DisableKeyword("CLIP_SPHERE");
        Shader.SetGlobalInt("_centerCount", 0);
        //we have declared: "material.EnableKeyword("CLIP_PLANE");" on all the crossSectionStandard derived materials - in the CrossSectionStandardShaderGUI editor script - so we have to switch it off
        Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allrenderers)
        {
            Material[] mats = r.sharedMaterials;
            foreach (Material m in mats) if (m.shader.name.Substring(0, 13) == "CrossSection/") m.DisableKeyword("CLIP_PLANE");
        }
    }
	
	void Update () {
        
        if (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.wasPressedThisFrame
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.GetMouseButtonDown(0) 
#endif
            )
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = Camera.main.ScreenPointToRay(
#if ENABLE_INPUT_SYSTEM
                Mouse.current.position.ReadValue()
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition 
#endif
                );
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                if (hit.transform.IsChildOf(transform))
                {
                    Debug.Log("hit");
                    Shader.EnableKeyword("CLIP_TUBES");

                    centerPoints[i % n] = hit.point;
                    AxisDirs[i % n] = (hit.point - Camera.main.transform.position).normalized;
                    radiuses[i % n] = 0.1f;

                    Shader.SetGlobalVectorArray("_centerPoints", centerPoints);
                    Shader.SetGlobalVectorArray("_AxisDirs", AxisDirs);
                    Shader.SetGlobalFloatArray("_Radiuses", radiuses);//*/

                    i++;
                    Shader.SetGlobalInt("_centerCount", (int)Mathf.Min(i, n));
                    StartCoroutine(drag());
                }
            }
        }
	}

    void OnEnable()
    {
        Shader.EnableKeyword("CLIP_TUBES");
        //Shader.EnableKeyword("CLIP_PLANE");
        //Shader.EnableKeyword("CLIP_PLANE");
    }

    void OnDisable()
    {
        Shader.DisableKeyword("CLIP_TUBES");
        Shader.SetGlobalInt("_centerCount", 0);
       // Shader.DisableKeyword("CLIP_PLANE");
    }

    void OnApplicationQuit()
    {
        //disable clipping so we could see the materials and objects in editor properly
        Shader.DisableKeyword("CLIP_TUBES");

    }


    IEnumerator drag()
    {
        float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
            Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.mousePosition.x, Input.mousePosition.y
#endif
            , cameraDistance));
        Vector3 translation = Vector3.zero;
        Camera.main.GetComponent<MaxCamera>().enabled = false;
        while (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.isPressed
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.GetMouseButton(0)
#endif
            )
        {
            translation = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
                Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition.x, Input.mousePosition.y
#endif
                , cameraDistance
                )) - startPoint;
            float m = translation.magnitude;
            if (m > 0.1f)
            {
                //Shader.SetGlobalFloat("_Radius", m);
                radiuses[(i-1) % n] = m;
                Shader.SetGlobalFloatArray("_Radiuses", radiuses);
                //Shader.SetGlobalFloat("_Rad" + ((i-1) % n).ToString(), radiuses[(i-1) % n]);
            }
            yield return null;
        }
        Camera.main.GetComponent<MaxCamera>().enabled = true;
        
    }
}
