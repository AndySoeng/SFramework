using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CuboidSectionExample : MonoBehaviour {
    public GameObject hatchedCube;
    private float mcsr = 0.05f; //minimal radius of inscribed sphere;

	void Start () {

        Shader.DisableKeyword("CLIP_PLANE");
        Shader.DisableKeyword("CLIP_CUBOID");
        Shader.DisableKeyword("CLIP_NONE");
        Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allrenderers)
        {
            Material[] mats = r.sharedMaterials;
            foreach (Material m in mats) if (m.shader.name.Substring(0, 13) == "CrossSection/") m.DisableKeyword("CLIP_PLANE");
        }
        Shader.SetGlobalColor("_SectionColor", Color.black);
        Matrix4x4 mx = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Shader.SetGlobalMatrix("_WorldToObjectMatrix", mx.inverse);
    }
	
	void Update () {
        //Shader.SetGlobalFloat("_Radius", 0.2f);
        //return;
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
                    Vector3 upVector = Vector3.Cross(hit.transform.up, hit.normal).normalized;
                    Quaternion rot = Quaternion.LookRotation(hit.normal, upVector);
                    Matrix4x4 mx = Matrix4x4.TRS(hit.point, rot, Vector3.one);
                    Shader.EnableKeyword("CLIP_CUBOID");


                    Renderer renderer = hit.transform.GetComponent<Renderer>();
                    Material m = renderer.sharedMaterial;
                    //foreach 
                    List<string> kwds = new List<string>(m.shaderKeywords);
                    bool hasKwd = kwds.Contains("CLIP_CUBOID");
                    if (hasKwd)
                    {
                        Debug.Log("CLIP_CUBOID");
                        m.EnableKeyword("CLIP_CUBOID");
                    }


                    Shader.SetGlobalVector("_SectionPoint", hit.point);
                    Shader.SetGlobalMatrix("_WorldToObjectMatrix", mx.inverse);
                    Shader.SetGlobalVector("_SectionScale", mcsr * Vector3.one);
                    if (hatchedCube)
                    {
                        hatchedCube.transform.position = hit.point;
                        hatchedCube.transform.rotation = rot;
                        StartCoroutine(drag(hatchedCube.transform));
                    }
                    else
                    {
                        StartCoroutine(drag());
                    }
                }
            }
        }
	}

    void OnEnable()
    {
        Shader.DisableKeyword("CLIP_NONE");
        Shader.EnableKeyword("CLIP_CUBOID");
    }

    void OnDisable()
    {
        Shader.DisableKeyword("CLIP_CUBOID");
        Shader.EnableKeyword("CLIP_NONE");

    }

    void OnApplicationQuit()
    {
        //disable clipping so we could see the materials and objects in editor properly
        Shader.DisableKeyword("CLIP_CUBOID");
        Shader.EnableKeyword("CLIP_NONE");
        Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allrenderers)
        {
            Material[] mats = r.sharedMaterials;
            foreach (Material m in mats) if (m.shader.name.Substring(0, 13) == "CrossSection/") m.DisableKeyword("CLIP_CUBOID");
        }

    }


    IEnumerator drag()
    {
        float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, Input.mousePosition.y
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
                , cameraDistance)) - startPoint;
            float m = translation.magnitude;
            if(m> mcsr) Shader.SetGlobalVector("_SectionScale", 2*m*Vector3.one);
            yield return null;
        }
        Camera.main.GetComponent<MaxCamera>().enabled = true;
        
    }

    IEnumerator drag(Transform hatchCube)
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
        hatchCube.localScale = 0.1f * Vector3.one;
        hatchCube.gameObject.SetActive(true);
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
                , cameraDistance)) - startPoint;

            float m = translation.magnitude;
            if (m > mcsr)
            {
                Vector3 translationY = Vector3.Project(translation, hatchCube.up);
                Vector3 translationX = Vector3.Project(translation, hatchCube.right);
                Vector3 scaleVector = new Vector3(translationX.magnitude, translationY.magnitude, m);
                Shader.SetGlobalVector("_SectionScale", scaleVector);
                hatchCube.localScale = scaleVector;
            }
            yield return null;
        }

        Camera.main.GetComponent<MaxCamera>().enabled = true;
        //hatchCube.gameObject.SetActive(false);
    }
}
