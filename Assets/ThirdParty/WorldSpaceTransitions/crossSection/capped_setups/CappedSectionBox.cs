//The purpose of this script is to manipulate the scale and position of the capped section box gizmo object 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WorldSpaceTransitions
{
    public class CappedSectionBox : MonoBehaviour, ISizedSection
    {

        //public LayerMask layer = 13;
        //[Space(10)]
        public Collider xAxis, xAxisNeg;
        [Space(10)]
        public Collider yAxis, yAxisNeg;
        [Space(10)]
        public Collider zAxis, zAxisNeg;

        private enum GizmoAxis { X, Y, Z, Xneg, Yneg, Zneg, XYRotate, XZRotate, YZRotate, none };
        private GizmoAxis selectedAxis;

        private RaycastHit hit;
        private Ray ray, ray1;
        private Plane dragplane;
        private float rayDistance, newRotY, rayDistancePrev, distance;
        private Vector3 lookCamera, startDrag, startPos, startDragRot, lookHitPoint, startScale;
        private bool dragging;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
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
                dragplane = new Plane();

                RaycastHit hit;
                if (xAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.X;
                    dragplane.SetNormalAndPosition(transform.up, transform.position);
                }
                else if (xAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Xneg;
                    dragplane.SetNormalAndPosition(-transform.up, transform.position);
                }
                else if (yAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Y;
                    dragplane.SetNormalAndPosition(transform.forward, transform.position);
                }
                else if (yAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Yneg;
                    dragplane.SetNormalAndPosition(-transform.forward, transform.position);
                }
                else if (zAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Z;
                    dragplane.SetNormalAndPosition(transform.up, transform.position);
                }
                else if (zAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Zneg;
                    dragplane.SetNormalAndPosition(-transform.up, transform.position);
                }
                else
                {
                    //Debug.Log(hit.collider.name);
                    return;
                }
                distance = hit.distance;
                startDrag = Camera.main.ScreenToWorldPoint(new Vector3(mPosition.x, mPosition.y, distance));
                startPos = transform.position;
                startScale = transform.localScale;
                dragging = true;
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

                if (dragging)
                {
                    float lsx = startScale.x;
                    float lsy = startScale.y;
                    float lsz = startScale.z;

                    switch (selectedAxis)
                    {
                        case GizmoAxis.X:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.right);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsx += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.right));
                                break;
                            }
                        case GizmoAxis.Xneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.right);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsx += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.right));
                                break;
                            }
                        case GizmoAxis.Y:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.up);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsy += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.up));
                                break;
                            }
                        case GizmoAxis.Yneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.up);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsy += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.up));
                                break;
                            }
                        case GizmoAxis.Z:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.forward);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsz += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.forward));
                                break;
                            }
                        case GizmoAxis.Zneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.forward);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsz += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.forward));
                                break;
                            }

                    }

                    transform.localScale = new Vector3(Mathf.Clamp(lsx, 0.01f, Mathf.Infinity), Mathf.Clamp(lsy, 0.01f, Mathf.Infinity), Mathf.Clamp(lsz, 0.01f, Mathf.Infinity));

                    //foreach (UVScaler uvs in gameObject.GetComponentsInChildren<UVScaler>()) uvs.SetUV();
                }

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

        public void HideCage(bool val)
        {
            foreach (PlaneHover ph in gameObject.GetComponentsInChildren<PlaneHover>()) ph.Hide (val);
        }
        public void HideCaps(bool val)
        {
            foreach (Transform quad in gameObject.transform) if (quad.name.Contains("hatch")) quad.GetComponent<Renderer>().enabled = val;
        }

        public void Size(Bounds b, GameObject g, BoundsOrientation orientation)
        {
            //Debug.Log(b.ToString());
            float scale = 1f;

            Vector3 clearance = 0.01f * Vector3.one;

            transform.localScale = Vector3.one;

            transform.localScale = scale * b.size + clearance;

            transform.position = b.center;
        }
    }
}