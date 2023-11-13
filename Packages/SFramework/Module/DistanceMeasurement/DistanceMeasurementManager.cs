using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceMeasurementManager : MonoBehaviour
{
    public DistanceMeasurementObject prefab_DM;

    public bool isEnable = true;

    private DistanceMeasurementObject cur_DM;

    void Update()
    {
        if (!isEnable) return;

        if (cur_DM == null && Input.GetMouseButtonDown(0))
        {
            OnRaycastStart(); //加一个球的半径
        }
        

        if (cur_DM != null)
        {
            OnRaycast();
        }

        if (Input.GetMouseButtonUp(0))
            OnRaycastEnd();
    }

    public (bool, RaycastHit) Raycast()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool rayCast = Physics.Raycast(cameraRay, out RaycastHit hitInfo, 1000);
        return (rayCast, hitInfo);
    }

    private void OnRaycastStart()
    {
        (bool isHit, RaycastHit hitInfo) = Raycast();
        if (isHit)
        {
            cur_DM = Instantiate(prefab_DM, transform);
            cur_DM.StartSpherePos = hitInfo.point + hitInfo.normal * 0.025f;
            cur_DM.startRealePos = hitInfo.point;
        }
    }

    private void OnRaycast()
    {
        (bool isHit, RaycastHit hitInfo) = Raycast();
        if (isHit)
            cur_DM.EndSpherePos = hitInfo.point + hitInfo.normal * 0.025f; //加一个球的半径
    }

    private void OnRaycastEnd()
    {
        (bool isHit, RaycastHit hitInfo) = Raycast();
        if (isHit)
        {
            cur_DM.EndSpherePos = hitInfo.point + hitInfo.normal * 0.025f;
            cur_DM.endRealePos = hitInfo.point;
        }

        cur_DM = null;
    }
}