/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: DragNode.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月5日
  Description: 拖拽节点
************************************************************/

using UnityEngine;
using System.Collections;

public class DragNode : MonoBehaviour {

    public Camera cam;

	// Use this for initialization
	void Start () {
        StartCoroutine(Drag());
	}
	
	// Update is called once per frame
	void Update () {

	}

    private IEnumerator Drag()
    {
        GameObject hitGo = null;

        Vector3 vec3TargetScreen = Vector3.zero;
        Vector3 vec3TargetWorld = Vector3.zero;
        Vector3 vec3MouseScreen = Vector3.zero;
        Vector3 vec3Offset = Vector3.zero;

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    hitGo = hit.collider.gameObject;

                    vec3TargetScreen = cam.WorldToScreenPoint(hit.transform.position);
                    vec3MouseScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, vec3TargetScreen.z);
                    vec3Offset = hitGo.transform.position - cam.ScreenToWorldPoint(vec3MouseScreen);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (hitGo)
                {
                    vec3MouseScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, vec3TargetScreen.z);
                    vec3TargetWorld = cam.ScreenToWorldPoint(vec3MouseScreen) + vec3Offset;
                    hitGo.transform.position = vec3TargetWorld;                    
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                hitGo = null;
            }

            yield return null;
        }
    }
}
