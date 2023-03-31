using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorldSpaceTransitions.Examples
{
    public class CappedSphereSectionExample : MonoBehaviour
    {

        public Transform clippingSphere;
        public Slider radiusSlider;
        public Text radiusValueText;
        public bool inverse = false;

        void Start()
        {
            Shader.DisableKeyword("CLIP_NONE");
            Shader.EnableKeyword("CLIP_SPHERE_OUT");
            Shader.SetGlobalInt("CLIP_SPHERE_OUT", 0);
            //we have declared: "material.EnableKeyword("CLIP_PLANE");" on all the crossSectionStandard derived materials - in the CrossSectionStandardShaderGUI editor script - so we have to switch it off
            Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();

            if (radiusSlider)
            {
                clippingSphere.localScale = radiusSlider.value * 2 * Vector3.one;//get initial values from UI
                radiusValueText.text = radiusSlider.value.ToString("0.00");
                radiusSlider.onValueChanged.AddListener(SetRadius);
                Shader.SetGlobalFloat("_Radius", radiusSlider.value);
                Shader.SetGlobalVector("_SectionPoint", clippingSphere.position);
            }
                
        }

        void SetRadius(float val)
        {
            Shader.SetGlobalFloat("_Radius", val);
            radiusValueText.text = radiusSlider.value.ToString("0.00");
            clippingSphere.localScale = 2*val*Vector3.one;
        }

        void OnEnable()
        {
            Shader.DisableKeyword("CLIP_NONE");
            Shader.EnableKeyword("CLIP_SPHERE_OUT");
            Shader.SetGlobalInt("_CLIP_SPHERE_OUT", 1);
            //Shader.EnableKeyword("CLIP_PLANE");
        }

        void OnDisable()
        {
            Shader.EnableKeyword("CLIP_NONE");
            Shader.DisableKeyword("CLIP_SPHERE_OUT");
            Shader.SetGlobalInt("_CLIP_SPHERE_OUT", 0);
        }

        void OnApplicationQuit()
        {
            //disable clipping so we could see the materials and objects in editor properly
            Shader.EnableKeyword("CLIP_NONE");
            Shader.DisableKeyword("CLIP_SPHERE_OUT");
            Shader.SetGlobalInt("_CLIP_SPHERE_OUT", 0);
        }
    }
}
