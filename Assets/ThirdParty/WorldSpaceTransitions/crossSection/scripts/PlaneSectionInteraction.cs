using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneSectionInteraction : MonoBehaviour
{
    Collider[] objectColliders;
    public Transform gizmo;
    public Text info;
    public Image mouseImg;
    Color imgColor;

    void Start()
    {
        objectColliders = GetComponentsInChildren<Collider>();
        imgColor = mouseImg.color;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            mouseImg.rectTransform.position = Input.mousePosition;
            imgColor.a = 1;
            mouseImg.color = imgColor;
            //Physics.queriesHitBackfaces = true;
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            Plane sectionPlane = new Plane(gizmo.forward, gizmo.position);

            if (hits.Length > 0)
            {
                List<RaycastHit> hitList = new List<RaycastHit>(hits);
                hitList.Sort((x, y) => x.distance.CompareTo(y.distance));//if you need descending sort, swap x and y on the right-hand side of the arrow =>.
                RaycastHit farthesttHit = hitList[hitList.Count - 1];

                //how big can be the farthest object? we need to cast the back ray from behind it
                GameObject farthestGo = farthesttHit.transform.gameObject;
                Collider farthestColl = farthestGo.GetComponent<Collider>();
                float maxsize = Vector3.Magnitude(farthestColl.bounds.size);

                float raycastBackDistance = farthesttHit.distance + 1 + maxsize;
                Ray backRay = new Ray(ray.GetPoint(raycastBackDistance), -ray.direction);
                RaycastHit[] backHits = Physics.RaycastAll(backRay, raycastBackDistance - Camera.main.nearClipPlane);

                //recalculate distances in relation to the camera
                for (int i = 0; i< backHits.Length; i++) backHits[i].distance = raycastBackDistance - backHits[i].distance;

                hitList.AddRange(backHits);
                hitList.Sort((x, y) => x.distance.CompareTo(y.distance));
                List<RaycastHit> objectHitList = new List<RaycastHit>();
                for (int i = 0; i < hitList.Count; i++)
                {
                    //Debug.Log(hitList[i].distance.ToString());
                    //Sort out hits, objectColliders only 
                    if (Array.IndexOf(objectColliders, hitList[i].collider) == -1) continue;
                    //
                    //Only hits behind the plane
                    if (sectionPlane.GetDistanceToPoint(hitList[i].point) > 0) continue;
                    objectHitList.Add(hitList[i]);
                }

                if (objectHitList.Count == 0)
                {
                    info.text = hitList[0].transform.name + " was hit";
                    return; // object is outside raycasts
                }

                if (Array.IndexOf(objectColliders, hitList[0].collider) == -1)
                {
                    Debug.Log("the object is hidden by " + hitList[0].collider.name);
                    info.text = "the object is hidden by " + hitList[0].collider.name;
                    return; // object is hidden
                }

                bool sectionWasHit = (Vector3.Dot(ray.direction, objectHitList[0].normal) > 0);

                info.text = objectHitList[0].transform.name + " was hit " + (sectionWasHit ? "on the section plane" : "on the outside");      
            }
            else
            {
                info.text = "no hits";
            }
        }
        imgColor.a -= 0.02f;
        mouseImg.color = imgColor;
    }
}
