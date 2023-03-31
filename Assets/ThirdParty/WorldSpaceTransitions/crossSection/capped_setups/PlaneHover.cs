using UnityEngine;
using System.Collections;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WorldSpaceTransitions
{
    public class PlaneHover : MonoBehaviour
    {

        public Color hovercolor;
        public Shader hideShader;
        private Color original;
        private Shader originalShader;

        private bool selected;
        private float _a = 0;

        private Material m;
        private bool baseColorPropertyExists = false;
        private bool hidden = false;

      //  private static int i = 0;

        public Color emissionColor;
        // Use this for initialization
        void Start()
        {

            //Material m0 = GetComponent<Renderer>().sharedMaterial;
            //m = new Material(m0);
            //m.name = m0.name + i.ToString();
            m = GetComponent<Renderer>().material;
            m.EnableKeyword("_Emission");
            m.SetColor("_EmissionColor", Color.black);
            //GetComponent<Renderer>().material = m;
            original = m.color;
            originalShader = m.shader;
            baseColorPropertyExists = m.HasProperty("_BaseColor");
            if (baseColorPropertyExists) original = m.GetColor("_BaseColor");
        }

        public void HighlightColor(float a)
        {
            if (hidden) return;
            //This is to make the corner highlighted with colour when it gets very small
            _a = a;
            //float a = Mathf.Clamp01(-2.0f * sc + 1.25f);
            Color c2 = a * emissionColor + original;

            if (baseColorPropertyExists)
            {
    
                m.SetColor("_BaseColor", c2);
            }
            m.SetColor("_EmissionColor", a * emissionColor);
            m.color = c2;
        }

        public void Hide(bool val)
        {
            hidden = !val;
            if (hideShader != null)
            {
                m.shader = val ? originalShader : hideShader;
            }
            else
            {
                GetComponent<Renderer>().enabled = val;
            }

        }

        
        void OnMouseEnter()
        {
            m.color = _a * emissionColor + hovercolor;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", _a * emissionColor + hovercolor);
        }

        void OnMouseExit()
        {
            if (!selected) HighlightColor(_a);
        }

        void SetOriginal()
        {

            m.color = original;
            if (m.HasProperty("_BaseColor"))m.SetColor("_BaseColor", original);
        }

        void Update()
        {

            if (selected &&
#if ENABLE_INPUT_SYSTEM
                Mouse.current.leftButton.wasReleasedThisFrame
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.GetMouseButtonUp(0) 
#endif
                )
            {
                SetOriginal();
                selected = false;
            }
        }

    }
}
