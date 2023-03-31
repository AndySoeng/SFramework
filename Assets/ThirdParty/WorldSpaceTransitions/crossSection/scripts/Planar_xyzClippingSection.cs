using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class Planar_xyzClippingSection : MonoBehaviour, ISizedSection
    {
        private GameObject xyzSectionPanel;
        private Text topSliderLabel, middleSliderLabel, bottomSliderLabel;
        private Slider slider;
        private Toggle xtoggle, ytoggle, ztoggle, gizmotoggle;

        private Vector3 sectionplane = Vector3.up;

        [HideInInspector]
        public GameObject model;

        public Transform ZeroReference;

        [SerializeField]
        [HideInInspector]
        private BoundsOrientation boundsMode = BoundsOrientation.worldOriented;

        [SerializeField]
        [HideInInspector]
        private Bounds bounds;

        public enum ConstrainedAxis { X, Y, Z };
        public ConstrainedAxis selectedAxis = ConstrainedAxis.Y;

        private GameObject rectGizmo;

        private Vector3 zeroReferenceVector = Vector3.zero;

        public bool gizmoOn = true;

        private Vector3 sliderRange = Vector3.zero;

        private float sectionX = 0;
        private float sectionY = 0;
        private float sectionZ = 0;

        void Awake()
        {
            //return;
            Debug.Log("bounds " + bounds.ToString());
            xyzSectionPanel = GameObject.Find("xyzSectionPanel");
            if (xyzSectionPanel)
            {
                slider = xyzSectionPanel.GetComponentInChildren<Slider>();
                topSliderLabel = xyzSectionPanel.transform.Find("sliderPanel/MaxText").GetComponent<Text>();
                middleSliderLabel = xyzSectionPanel.transform.Find("sliderPanel/Slider").GetComponentInChildren<Text>();
                bottomSliderLabel = xyzSectionPanel.transform.Find("sliderPanel/MinText").GetComponent<Text>();
                if (xyzSectionPanel.transform.Find("axisOptions"))
                {
                    xtoggle = xyzSectionPanel.transform.Find("axisOptions/Panel/X_Toggle").GetComponent<Toggle>();
                    ytoggle = xyzSectionPanel.transform.Find("axisOptions/Panel/Y_Toggle").GetComponent<Toggle>();
                    ztoggle = xyzSectionPanel.transform.Find("axisOptions/Panel/Z_Toggle").GetComponent<Toggle>();
                    xtoggle.isOn = selectedAxis == ConstrainedAxis.X;
                    ytoggle.isOn = selectedAxis == ConstrainedAxis.Y;
                    ztoggle.isOn = selectedAxis == ConstrainedAxis.Z;
                }
                if (xyzSectionPanel.transform.Find("gizmoToggle"))
                {
                    gizmotoggle = xyzSectionPanel.transform.Find("gizmoToggle").GetComponent<Toggle>();
                    gizmotoggle.isOn = gizmoOn;
                }
            }
            
        }

        void Start()
        {
            if (!Application.isPlaying) return;
            //Debug.Log("bounds s " + bounds.ToString());
            if (slider) slider.onValueChanged.AddListener(SliderListener);
            //if (xyzSectionPanel) xyzSectionPanel.SetActive(enabled);

            Shader.EnableKeyword("CLIP_PLANE");
            Shader.SetGlobalVector("_SectionPlane", Vector3.up);
            if (xtoggle) xtoggle.onValueChanged.AddListener(delegate { SetAxis(xtoggle.isOn, ConstrainedAxis.X); });
            if (ytoggle) ytoggle.onValueChanged.AddListener(delegate { SetAxis(ytoggle.isOn, ConstrainedAxis.Y); });
            if (ztoggle) ztoggle.onValueChanged.AddListener(delegate { SetAxis(ztoggle.isOn, ConstrainedAxis.Z); });
            if (gizmotoggle) gizmotoggle.onValueChanged.AddListener(GizmoOn);

            SliderSetup();
            setupGizmo();
            setSection();
        }

        void SliderSetup()
        {
            if (ZeroReference)
            {
                zeroReferenceVector = boundsMode==BoundsOrientation.worldOriented? ZeroReference.position : ZeroReference.localPosition;
            }

            sliderRange = new Vector3((float)SignificantDigits.CeilingToSignificantFigures((decimal)(1.08f * 2 * bounds.extents.x), 2),
            (float)SignificantDigits.CeilingToSignificantFigures((decimal)(1.08f * 2 * bounds.extents.y), 2),
            (float)SignificantDigits.CeilingToSignificantFigures((decimal)(1.08f * 2 * bounds.extents.z), 2));
            Vector3 Min = (boundsMode == BoundsOrientation.worldOriented) ? bounds.min : bounds.center - model.transform.rotation * bounds.extents;
            //sliderRange = (boundsMode == BoundsOrientation.worldOriented) ? sliderRange : model.transform.rotation * sliderRange;
            sectionX = Min.x + sliderRange.x;
            sectionY = Min.y + sliderRange.y;
            sectionZ = Min.z + sliderRange.z;
            Debug.Log(new Vector3(sectionX, sectionY, sectionZ).ToString());
        }

        public void SliderListener(float value)
        {
            if (middleSliderLabel) middleSliderLabel.text = value.ToString("0.0");
            Vector3 gpos = bounds.center;
            Vector3 glocpos = model.transform.InverseTransformDirection(bounds.center - model.transform.position);
            switch (selectedAxis)
            {
                case ConstrainedAxis.X:
                    sectionX = value + zeroReferenceVector.x;
                    gpos.x = sectionX;
                    gpos = (boundsMode == BoundsOrientation.worldOriented) ? gpos : model.transform.position + model.transform.right * (value + zeroReferenceVector.x); //+ model.transform.InverseTransformDirection(model.transform.position - bounds.center)
                    glocpos.x = value + zeroReferenceVector.x;
                    break;
                case ConstrainedAxis.Y:
                    sectionY = value + zeroReferenceVector.y;
                    gpos.y = sectionY;
                    gpos = (boundsMode == BoundsOrientation.worldOriented) ? gpos : model.transform.position + model.transform.up * (value + zeroReferenceVector.y);
                    glocpos.y = value + zeroReferenceVector.y;
                    break;
                case ConstrainedAxis.Z:
                    sectionZ = value + zeroReferenceVector.z;
                    gpos.z = sectionZ;
                    gpos = (boundsMode == BoundsOrientation.worldOriented) ? gpos : model.transform.position + model.transform.forward * (value + zeroReferenceVector.z);
                    glocpos.z = value + zeroReferenceVector.z;
                    break;
            }
            if (rectGizmo)
            {
                Vector3 wpos = model.transform.TransformPoint(glocpos);
                rectGizmo.transform.localPosition = (boundsMode == BoundsOrientation.worldOriented) ? gpos : wpos;
            }
            Shader.SetGlobalVector("_SectionPoint", gpos);
        }

        public void SetAxis(bool b, ConstrainedAxis a)
        {
            if (b)
            {
                SliderSetup();
                selectedAxis = a;
                Debug.Log(a);
                RectGizmo rg = rectGizmo.GetComponent<RectGizmo>();
                rg.transform.position = Vector3.zero;
                rg.SetSizedGizmo(bounds.size, selectedAxis);
                setSection();
            }
        }

        void setSection()
        {
            float sliderMaxVal = 0f;
            float sliderVal = 0f;
            float sliderMinVal = 0f;
            Vector3 sectionpoint = new Vector3(sectionX, sectionY, sectionZ);
            if ((boundsMode == BoundsOrientation.worldOriented)) sectionpoint = bounds.center;
            Vector3 sectionpointLocal = model.transform.InverseTransformPoint(sectionpoint);
            Vector3 sectionpointWorld;
            //Debug.Log(bounds.ToString());
            Debug.Log(selectedAxis.ToString());
            Vector3 Min = (boundsMode == BoundsOrientation.worldOriented) ? bounds.min : -bounds.extents - model.transform.InverseTransformDirection(model.transform.position - bounds.center); 
            switch (selectedAxis)
            {
                case ConstrainedAxis.X:
                    sectionplane = (boundsMode == BoundsOrientation.worldOriented) ? Vector3.right: model.transform.right;
                    sliderMaxVal = Min.x + sliderRange.x - zeroReferenceVector.x;
                    sliderMinVal = Min.x - 0.01f - zeroReferenceVector.x;
                    //sliderVal = sectionX - zeroReferenceVector.x;
                    if (boundsMode == BoundsOrientation.objectOriented)
                    {
                        sectionpointLocal.x = Mathf.Clamp(sectionpointLocal.x, Min.x, Min.x + sliderRange.x);
                        sliderVal = sectionpointLocal.x - zeroReferenceVector.x;
                    }
                    else
                    {
                        sectionX = Mathf.Clamp(sectionX, bounds.min.x, bounds.min.x + sliderRange.x);
                        sliderVal = sectionX - zeroReferenceVector.x;
                        sectionpoint.x = sectionX;
                    }
                    break;
                case ConstrainedAxis.Y:
                    sectionplane = (boundsMode == BoundsOrientation.worldOriented) ? Vector3.up : model.transform.up;
                    sliderMaxVal = Min.y + sliderRange.y - zeroReferenceVector.y;
                    sliderMinVal = Min.y - 0.01f - zeroReferenceVector.y;
                    //sliderVal = sectionY - zeroReferenceVector.y;
                    if (boundsMode == BoundsOrientation.objectOriented)
                    {
                        sectionpointLocal.y = Mathf.Clamp(sectionpointLocal.y, Min.y, Min.y + sliderRange.y);
                        sliderVal = sectionpointLocal.y - zeroReferenceVector.y;
                    }
                    else
                    {
                        sectionY = Mathf.Clamp(sectionY, bounds.min.y, bounds.min.y + +sliderRange.y);
                        sliderVal = sectionY - zeroReferenceVector.y;
                        sectionpoint.y = sectionY;
                    }
                    break;
                case ConstrainedAxis.Z:
                    sectionplane = (boundsMode == BoundsOrientation.worldOriented) ? Vector3.forward : model.transform.forward;
                    sliderMaxVal = Min.z + sliderRange.z - zeroReferenceVector.z;
                    sliderMinVal = Min.z - 0.01f - zeroReferenceVector.z;
                    //sliderVal = sectionZ - zeroReferenceVector.z;
                    if (boundsMode == BoundsOrientation.objectOriented)
                    {
                        sectionpointLocal.z = Mathf.Clamp(sectionpointLocal.z, Min.z, Min.z + sliderRange.z);
                        sliderVal = sectionpointLocal.z - zeroReferenceVector.z;
                    }
                    else
                    {
                        sectionZ = Mathf.Clamp(sectionZ, bounds.min.z, bounds.min.z + sliderRange.z);
                        sliderVal = sectionZ - zeroReferenceVector.z;
                        sectionpoint.z = sectionZ;
                    }
                    break;
                default:
                    Debug.Log("case default");
                    break;
            }
            if (boundsMode == BoundsOrientation.objectOriented)
            {
                Vector3 wpos = model.transform.TransformPoint(sectionpointLocal);
                rectGizmo.transform.position = wpos;
            }
            else
            {
                rectGizmo.transform.position = sectionpoint;
            }

            Shader.SetGlobalVector("_SectionPoint", sectionpoint);
            Shader.SetGlobalVector("_SectionPlane", sectionplane);


            if (topSliderLabel) topSliderLabel.text = sliderMaxVal.ToString("0.0");
            if (bottomSliderLabel) bottomSliderLabel.text = sliderMinVal.ToString("0.0");

            if (slider)
            {
                slider.maxValue = sliderMaxVal;
                slider.minValue = sliderMinVal;
                sliderVal = Mathf.Clamp(sliderVal, sliderMinVal, sliderMaxVal);
                slider.value = sliderVal;
                middleSliderLabel.text = sliderVal.ToString("0.0");

                slider.minValue = sliderMinVal;
            }
        }

        void setupGizmo()
        {
            rectGizmo = Resources.Load("rectGizmo") as GameObject;
            if (rectGizmo) Debug.Log("rectGizmo");

            Quaternion _rot = (boundsMode == BoundsOrientation.worldOriented) ? Quaternion.identity: model.transform.rotation;
            rectGizmo = Instantiate(rectGizmo, bounds.center + (-bounds.extents.y + (slider ? slider.value : 0) + zeroReferenceVector.y) * transform.up, _rot) as GameObject;

            RectGizmo rg = rectGizmo.GetComponent<RectGizmo>();

            rg.SetSizedGizmo(bounds.size, selectedAxis);
            /* Set rectangular gizmo size here: inner width, inner height, border width.
             */
            rectGizmo.SetActive(false);

        }

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            Shader.EnableKeyword("CLIP_PLANE");
            if (xyzSectionPanel) xyzSectionPanel.SetActive(true);
            if (slider)
            {
                Shader.SetGlobalVector("_SectionPoint", new Vector3(sectionX, sectionY, sectionZ));
            }
        }

        void OnDisable()
        {
            Shader.DisableKeyword("CLIP_PLANE");
            if (!Application.isPlaying) return;
            if (xyzSectionPanel) xyzSectionPanel.SetActive(false);
        }

        void OnApplicationQuit()
        {
            Shader.DisableKeyword("CLIP_PLANE");
        }

        void Update()
        {
            if (!Application.isPlaying) return;
#if ENABLE_INPUT_SYSTEM
            // New input system backends are enabled.
           if(Mouse.current.leftButton.isPressed)
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            // Old input backends are enabled.
            if (Input.GetMouseButtonDown(0))
#endif
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
                if (!model) return;
                Collider coll = model.GetComponent<Collider>();
                if (!coll) return;
                if (coll.Raycast(ray, out hit, 10000f))
                {
                    if (gizmoOn) rectGizmo.SetActive(true);
                    StartCoroutine(dragGizmo());
                }
                else
                {
                    rectGizmo.SetActive(false);
                }
            }
        }

        IEnumerator dragGizmo()
        {
            float cameraDistance = Vector3.Distance(bounds.center, Camera.main.transform.position);
            Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
                Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition.x, Input.mousePosition.y
#endif
                , cameraDistance));
            Vector3 startPos = rectGizmo.transform.position;
            Vector3 translation = Vector3.zero;
            Camera.main.GetComponent<MaxCamera>().enabled = false;
            if (slider) slider.onValueChanged.RemoveListener(SliderListener);
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
                Vector3 projectedTranslation = Vector3.Project(translation, sectionplane);
                Vector3 newPoint = startPos + projectedTranslation;
                /*if ((boundsMode == BoundsOrientation.worldOriented))
                {
                    switch (selectedAxis)
                    {
                        case ConstrainedAxis.X:
                            sectionX = Mathf.Clamp(newPoint.x, bounds.min.x, bounds.max.x); 
                            break;
                        case ConstrainedAxis.Y:
                            sectionY = Mathf.Clamp(newPoint.y, bounds.min.y, bounds.max.y);
                            break;
                        case ConstrainedAxis.Z:
                            sectionZ = Mathf.Clamp(newPoint.z, bounds.min.z, bounds.max.z);
                            break;
                    }
                }*/
                //else {
                    sectionX = newPoint.x; sectionY = newPoint.y; sectionZ = newPoint.z;
                //}
                setSection();
                yield return null;
            }
            Camera.main.GetComponent<MaxCamera>().enabled = true;
            if (slider) slider.onValueChanged.AddListener(SliderListener);
        }

        public void GizmoOn(bool val)
        {
            gizmoOn = val;
            if (rectGizmo) rectGizmo.SetActive(val);
        }

        public void Size(Bounds b, GameObject g, BoundsOrientation orientation)
        {
            boundsMode = orientation;
            model = g;
            bounds = b;
            enabled = false;
            enabled = true;
            if (!Application.isPlaying) return;
            SliderSetup();
            SetAxis(true, selectedAxis);
            if(rectGizmo)
                rectGizmo.transform.rotation = (boundsMode == BoundsOrientation.worldOriented) ? Quaternion.identity : model.transform.rotation;
        }
    }
}
