/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWireMenuTool.cs
  Author: 万剑飞       Version :1.1          Date: 2017年5月5日
  Description: 菜单工具
  ---2017/8/2---
  1、创建时为LineRenderer赋值默认材质
  2、初始创建三个节点
  3、当前选择的物体默认作为父物体
************************************************************/

using UnityEngine;
using UnityEditor;

public class CustomWireMenuTool {
    
    [MenuItem("CustomWire/Create A New Wire")]
    private static void CreateNewCustomWire()
    {
        GameObject wire = new GameObject("CustomWire");
        Undo.RegisterCreatedObjectUndo(wire, "Create A New Wire");

        if (Selection.activeTransform)
        {
            wire.transform.parent = Selection.activeTransform;
            wire.transform.localPosition = Vector3.zero;
            wire.transform.localEulerAngles = Vector3.zero;
            wire.transform.localScale = Vector3.one;
        }

        GameObject line = new GameObject("Line");
        line.transform.parent = wire.transform;
        line.transform.localPosition = Vector3.zero;
        LineRenderer lineComponent = line.AddComponent<LineRenderer>();
        //lineComponent.SetVertexCount(0);
        lineComponent.positionCount = 0;
        //lineComponent.SetWidth(0.01f, 0.01f);
        lineComponent.startWidth = 0.01f;
        lineComponent.endWidth = 0.01f;
        lineComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineComponent.receiveShadows = false;
        lineComponent.material = Resources.Load("Material/CustomWire_DefaultMaterial") as Material;

        CustomWire wireComponent = wire.AddComponent<CustomWire>();
        wireComponent.lineRenderer = lineComponent;
        wireComponent.setOnUpdate = true;
        wireComponent.drawNodes = true;
        wireComponent.AddNode();
        wireComponent.AddNode();
        wireComponent.AddNode();

        Selection.activeGameObject = wire;
    }
}
