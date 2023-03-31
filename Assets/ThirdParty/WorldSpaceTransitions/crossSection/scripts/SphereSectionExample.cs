using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WorldSpaceTransitions.Examples
{
    public class SphereSectionExample : MonoBehaviour
    {

        public bool inverse = false;

        void Start()
        {

            Shader.DisableKeyword("CLIP_SPHERE");
            //Shader.SetGlobalInt("_CLIP_SPHERE", 0);
            //we have declared: "material.EnableKeyword("CLIP_PLANE");" on all the crossSectionStandard derived materials - in the CrossSectionStandardShaderGUI editor script - so we have to switch it off
            Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allrenderers)
            {
                Material[] mats = r.sharedMaterials;
                foreach (Material m in mats) if (m.shader.name.Substring(0, 13) == "CrossSection/")
                    {
                        m.DisableKeyword("CLIP_PLANE");
                        m.SetInt("_CLIP_PLANE", 0);
                    }
            }
        }

        void Update()
        {

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
                        Shader.EnableKeyword("CLIP_SPHERE");
                        //Shader.SetGlobalInt("_CLIP_SPHERE", 1);
                        Shader.SetGlobalVector("_SectionPoint", hit.point);
                        Shader.SetGlobalFloat("_Radius", 0.1f);
                        StartCoroutine(drag());
                    }
                }
            }
        }

        void OnEnable()
        {
            Shader.DisableKeyword("CLIP_NONE");
        }

        void OnDisable()
        {
            Shader.DisableKeyword("CLIP_SPHERE");
            Shader.EnableKeyword("CLIP_NONE");
            //Shader.SetGlobalInt("_CLIP_SPHERE", 0);
            //Shader.DisableKeyword("CLIP_PLANE");
        }

        void OnApplicationQuit()
        {
            //disable clipping so we could see the materials and objects in editor properly
            Shader.DisableKeyword("CLIP_SPHERE");
            Shader.EnableKeyword("CLIP_NONE");
            //Shader.SetGlobalInt("_CLIP_SPHERE", 0);
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
                    , cameraDistance)) - startPoint;
                float m = translation.magnitude;
                if (inverse) m *= -1;
                if (m > 0.1f || m < 0.1f) Shader.SetGlobalFloat("_Radius", m);
                yield return null;
            }
            Camera.main.GetComponent<MaxCamera>().enabled = true;

        }
    }
}
