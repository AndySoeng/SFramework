using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AdvancedGizmo
{
    public class Hover : MonoBehaviour
    {

        public Color hovercolor;
        private Color original;
        private Renderer rend;
        private bool selected;
        private float t = 0;

        private EventSystem ES;

        void Start()
        {
            ES = EventSystem.current;
            rend = transform.GetComponent<Renderer>();
            original = rend.material.color;
            if (rend.material.HasProperty("_BaseColor")) original = rend.material.GetColor("_BaseColor");
        }

        void OnMouseEnter()
        {

            if (ES) if (EventSystem.current.IsPointerOverGameObject()) return;
            SetHovered();
        }

        void OnMouseExit()
        {
            if (ES) if (EventSystem.current.IsPointerOverGameObject()) return;
            if (!selected)
                SetOriginal();
        }

        void SetHovered()
        {

            rend.material.color = hovercolor;
            if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", hovercolor);
        }

        void SetOriginal()
        {

            rend.material.color = original;
            if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", original);
        }

        void OnMouseDown()
        {
            if (ES) if (EventSystem.current.IsPointerOverGameObject()) return;
            selected = true;
            if (Time.time - t < 0.3f)
            {
                SendMessageUpwards("ChangeMode");
                SetOriginal();
            }
            t = Time.time;
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
