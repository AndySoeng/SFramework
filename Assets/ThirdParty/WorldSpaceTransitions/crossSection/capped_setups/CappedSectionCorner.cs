//The purpose of this script is to manipulate the scale and position of the capped section corner gizmo object 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WorldSpaceTransitions
{
    public interface ISizedSection
    {
        void Size(Bounds _b, GameObject _g, BoundsOrientation _o);
    }

    [ExecuteInEditMode]
    public class CappedSectionCorner   : MonoBehaviour, ISizedSection
    {

        public LayerMask layerMask = (int)Mathf.Pow(2,13);
        //[Space(10)]
        public Collider xAxis;
        //[Space(10)]
        public Collider yAxis;
        //[Space(10)]
        public Collider zAxis;
        [Space(10)]
        public bool highlightWhenSmall = true;
        [Range(0,1)]
        public float highlightWhenSmallStrength = 0.5f;

        private enum GizmoAxis { X, Y, Z, XYRotate, XZRotate, YZRotate, none };
        private GizmoAxis selectedAxis;

        private float distance;
        private Vector3 startDrag, startPos, startScale;
        private Vector3 posMin, translationMin;
        private Vector3 posMax, translationMax;
        private bool dragging;

        private float clickTime = 0;

        private static Vector3 clearance = 0.01f * Vector3.one;

        [HideInInspector]
        public Bounds initialBounds;
        [HideInInspector]
        public bool limitToInitialBounds = true;

        #region GearVR/OculusGo VR vars
        //unhide for GearVR/OculusGo VR app
        private int axisCurrent = -1;
        [HideInInspector]
        public Transform laserPointer;
        [HideInInspector]
        public float reticleDistance = 10f;
        #endregion

        private float scaleM = 1;
        // Use this for initialization
        void Start()
        {
            scaleM = Vector3.Magnitude(transform.localScale);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
           
        }
#endif

        // Update is called once per frame
        void Update()
        {
            //if (OVRManager.isHmdPresent) return;//GearVR
            RaycastHit hit;
            Ray ray;
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
                Vector2 mPosition =
#if ENABLE_INPUT_SYSTEM
                    Mouse.current.position.ReadValue()
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                    Input.mousePosition 
#endif
                    ;
                ray = Camera.main.ScreenPointToRay(mPosition);

                if (Physics.Raycast(ray, out hit, 1000f, layerMask))
                {
                    distance = hit.distance;
                    startDrag = Camera.main.ScreenToWorldPoint(new Vector3(mPosition.x, mPosition.y, distance));
                    startPos = transform.position;
                    startScale = transform.localScale;

                    if (hit.collider == xAxis)
                    {
                        selectedAxis = GizmoAxis.X;
                        posMin = initialBounds.center - initialBounds.extents.x * transform.right;
                        posMax = initialBounds.center + initialBounds.extents.x * transform.right;
                        translationMin = Vector3.Project(posMin - startPos, transform.right);
                        Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                        translationMax = Vector3.Project(posMax - startPos, transform.right);
                        Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                    }
                    else if (hit.collider == yAxis)
                    {
                        selectedAxis = GizmoAxis.Y;
                        posMin = initialBounds.center - initialBounds.extents.y * transform.up;
                        posMax = initialBounds.center + initialBounds.extents.y * transform.up;
                        translationMin = Vector3.Project(posMin - startPos, transform.up);
                        Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                        translationMax = Vector3.Project(posMax - startPos, transform.up);
                        Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                    }
                    else if (hit.collider == zAxis)
                    {
                        selectedAxis = GizmoAxis.Z;
                        posMin = initialBounds.center - initialBounds.extents.z * transform.forward;
                        posMax = initialBounds.center + initialBounds.extents.z * transform.forward;
                        translationMin = Vector3.Project(posMin - startPos, transform.forward);
                        Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                        translationMax = Vector3.Project(posMax - startPos, transform.forward);
                        Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                    }
                    else
                    {
                        //Debug.Log(hit.collider.name);
                        return;
                    }
                    dragging = true;
                    if (Time.time - clickTime < 0.3f)
                    {
                        transform.position = startPos + translationMin;
                        Vector3 localScale = startScale;
                        if (hit.collider == xAxis)
                        {
                            localScale.x = initialBounds.size.x + 0.1f;
                        }
                        if (hit.collider == yAxis)
                        {
                            localScale.y = initialBounds.size.y + 0.1f;
                        }
                        if (hit.collider == zAxis)
                        {
                            localScale.z = initialBounds.size.z + 0.1f;
                        }
                        transform.localScale = localScale;
                        HighloghtBoxWhenGetsSmall();
                        //foreach (PlaneHover uvs in gameObject.GetComponentsInChildren<PlaneHover>()) uvs.SetColor(Vector3.Magnitude(transform.localScale) / scaleM);
                        dragging = false;

                    }
                    clickTime = Time.time;
                }
            }

            if (dragging)
            {
                Vector2 mPosition =
#if ENABLE_INPUT_SYSTEM
                    Mouse.current.position.ReadValue()
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                    Input.mousePosition 
#endif
                    ;
                ray = Camera.main.ScreenPointToRay(mPosition);
                Vector3 onDrag = Camera.main.ScreenToWorldPoint(new Vector3(mPosition.x, mPosition.y, distance));
                Vector3 translation = onDrag - startDrag;
                Vector3 projectedTranslation = Vector3.zero;

                float lsx = startScale.x;
                float lsy = startScale.y;
                float lsz = startScale.z;

                float magnitude = translation.magnitude;
                float magnitudeMax = 0;

                switch (selectedAxis)
                {
                    case GizmoAxis.X:
                        {
                            projectedTranslation = Vector3.Project(translation, transform.right);//

                            //the vectors compared below are supposed to be paralell, either equal or opposite
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMax.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMax);
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMin.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMin);

                            magnitude = Mathf.Clamp(magnitude,0, magnitudeMax);

                            transform.position = startPos + projectedTranslation.normalized * magnitude;
                            lsx -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.right));
                            break;
                        }
                    case GizmoAxis.Y:
                        {
                            projectedTranslation = Vector3.Project(translation, transform.up);

                            //the vectors compared below are supposed to be paralell, either equal or opposite
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMax.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMax);
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMin.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMin);

                            magnitude = Mathf.Clamp(magnitude, 0, magnitudeMax);

                            transform.position = startPos + projectedTranslation.normalized * magnitude;
                            lsy -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.up));
                            break;
                        }
                    case GizmoAxis.Z:
                        {
                            projectedTranslation = Vector3.Project(translation, transform.forward);

                            //the vectors compared below are supposed to be paralell, either equal or opposite
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMax.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMax);
                            if (Vector3.Magnitude(projectedTranslation.normalized - translationMin.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMin);

                            magnitude = Mathf.Clamp(magnitude, 0, magnitudeMax);

                            transform.position = startPos + projectedTranslation.normalized * magnitude;
                            lsz -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.forward));
                            break;
                        }
                }

                transform.localScale = new Vector3(Mathf.Clamp(lsx, 0.1f, Mathf.Infinity), Mathf.Clamp(lsy, 0.1f, Mathf.Infinity), Mathf.Clamp(lsz, 0.1f, Mathf.Infinity));

                //foreach (PlaneHover uvs in gameObject.GetComponentsInChildren<PlaneHover>()) uvs.SetColor(Vector3.Magnitude(transform.localScale) / scaleM);
                HighloghtBoxWhenGetsSmall();


                if (
#if ENABLE_INPUT_SYSTEM
                    Mouse.current.leftButton.wasReleasedThisFrame
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                    Input.GetMouseButtonUp(0) 
#endif
                    )
                {
                    dragging = false;
                }
            }
        }

        public void Size(Bounds b, GameObject g, BoundsOrientation orientation)
        {
            float scale = 0.5f;

            initialBounds = new Bounds(b.center, b.size + clearance);

            transform.localScale = scale * initialBounds.size;

            transform.position = b.center;

            // this makes the value saved with the scene
            enabled = false;
            enabled = true;
        }

        public void HideCage(bool val)
        {
            foreach (PlaneHover ph in gameObject.GetComponentsInChildren<PlaneHover>()) ph.Hide(val);
        }
        public void HideCaps(bool val)
        {
            foreach (Transform quad in gameObject.transform) if (quad.name.Contains("hatch")) quad.GetComponent<Renderer>().enabled = val;
        }

        void HighloghtBoxWhenGetsSmall()
        {
            if(!highlightWhenSmall) return;
            //float sc = Vector3.Magnitude(transform.localScale) / scaleM;
            float sc = Mathf.Min(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z)) / scaleM;
            float a = highlightWhenSmallStrength * 0.5f*Mathf.Clamp01(-4 * sc + 1.25f);
            Debug.Log("a " + a.ToString());
            foreach (PlaneHover uvs in gameObject.GetComponentsInChildren<PlaneHover>()) uvs.HighlightColor(a);
        }


        #region EasyInputVR

        public void MoveStart(int axis)
        {
            axisCurrent = axis;
            Ray ray = new Ray(laserPointer.position, laserPointer.forward);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, reticleDistance, layerMask))
            {
                startPos = transform.position;
                distance = rayHit.distance;
                startDrag = ray.GetPoint(distance);
                startScale = transform.localScale;
                dragging = true;
                if (axis == 0)
                {
                    posMin = initialBounds.center - initialBounds.extents.x * transform.right;
                    posMax = initialBounds.center + initialBounds.extents.x * transform.right;
                    translationMin = Vector3.Project(posMin - startPos, transform.right);
                    Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                    translationMax = Vector3.Project(posMax - startPos, transform.right);
                    Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                }
                if (axis == 1)
                {
                    posMin = initialBounds.center - initialBounds.extents.y * transform.up;
                    posMax = initialBounds.center + initialBounds.extents.y * transform.up;
                    translationMin = Vector3.Project(posMin - startPos, transform.up);
                    Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                    translationMax = Vector3.Project(posMax - startPos, transform.up);
                    Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                }
                if (axis == 2)
                {
                    posMin = initialBounds.center - initialBounds.extents.z * transform.forward;
                    posMax = initialBounds.center + initialBounds.extents.z * transform.forward;
                    translationMin = Vector3.Project(posMin - startPos, transform.forward);
                    Debug.DrawLine(startPos, startPos + translationMin, Color.red, 1);
                    translationMax = Vector3.Project(posMax - startPos, transform.forward);
                    Debug.DrawLine(startPos, startPos + translationMax, Color.green, 1);
                }

                if (Time.time - clickTime < 0.3f)
                {
                    transform.position = startPos + translationMin;
                    Vector3 localScale = startScale;
                    if (axis == 0)
                    {
                        localScale.x = initialBounds.size.x + 0.1f;
                    }
                    if (axis == 1)
                    {
                        localScale.y = initialBounds.size.y + 0.1f;
                    }
                    if (axis == 2)
                    {
                        localScale.z = initialBounds.size.z + 0.1f;
                    }
                    transform.localScale = localScale;
                    //foreach (PlaneHover uvs in gameObject.GetComponentsInChildren<PlaneHover>()) uvs.SetColor(Vector3.Magnitude(transform.localScale) / scaleM);
                    HighloghtBoxWhenGetsSmall();
                    axisCurrent = -1;
                    dragging = false;

                }
                clickTime = Time.time;
            }
        }
        public void Move(int axis)
        {
            if (axis != axisCurrent) return;
            if (!dragging) return;
            Vector3 localScale = startScale;
            Ray ray = new Ray(laserPointer.position, laserPointer.forward);
            Vector3 onDrag = ray.GetPoint(distance);
            Vector3 translation = onDrag - startDrag;
            float magnitude = translation.magnitude;
            float magnitudeMax = 0;
            Vector3[] direction = new Vector3[] {transform.right, transform.up, transform.forward };
            Vector3 projectedTranslation = Vector3.Project(translation, direction[axisCurrent]);

            //the vectors compared below are supposed to be paralell, either equal or opposite
            if (Vector3.Magnitude(projectedTranslation.normalized - translationMax.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMax);
            if (Vector3.Magnitude(projectedTranslation.normalized - translationMin.normalized) < 0.1f) magnitudeMax = Vector3.Magnitude(translationMin);

            magnitude = Mathf.Clamp(magnitude, 0, magnitudeMax);

            transform.position = startPos + projectedTranslation.normalized * magnitude;

            if (axisCurrent == 0)
            {
                localScale.x -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, direction[axisCurrent]));
                localScale.x = Mathf.Clamp(localScale.x, 0.1f, Mathf.Infinity);
            }
            if (axisCurrent == 1)
            {
                localScale.y -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, direction[axisCurrent]));
                localScale.y = Mathf.Clamp(localScale.y, 0.1f, Mathf.Infinity);
            }
            if (axisCurrent == 2)
            {
                localScale.z -= magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, direction[axisCurrent]));
                localScale.z = Mathf.Clamp(localScale.z, 0.1f, Mathf.Infinity);
            }
            transform.localScale = localScale;
            //foreach (PlaneHover uvs in gameObject.GetComponentsInChildren<PlaneHover>()) uvs.SetColor(Vector3.Magnitude(transform.localScale) / scaleM);
            HighloghtBoxWhenGetsSmall();
        }
        public void MoveEnd(int axis)
        {
            axisCurrent = -1;
            dragging = false;
        }
        #endregion
    }
}
