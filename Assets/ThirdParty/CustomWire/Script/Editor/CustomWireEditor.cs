/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWireEditor.cs
  Author: 万剑飞       Version :1.1          Date: 2017年5月4日
  Description: CustomWire类的自定义面板
  ---2017/8/2---
  1、增加展开项中文字的缩进
************************************************************/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomWire))]
public class CustomWireEditor : Editor {

    CustomWire wireComponent;

    SerializedProperty propLineRenderer;
    SerializedProperty propWireType;
    SerializedProperty propSetOnUpdate;
    SerializedProperty propCloseWire;
    SerializedProperty propDrawNodes;
    SerializedProperty propNodeRadius;

    bool expandConfiguration = true;
    bool expandNodeManager   = true;
    bool expandNodesList     = true;
    bool bCreateNode         = false;
    bool bRemoveNodes        = false;

    void OnEnable()
    {
        propLineRenderer = serializedObject.FindProperty("lineRenderer");
        propWireType     = serializedObject.FindProperty("wireType");
        propSetOnUpdate  = serializedObject.FindProperty("setOnUpdate");
        propCloseWire    = serializedObject.FindProperty("closeWire");
        propDrawNodes    = serializedObject.FindProperty("drawNodes");
        propNodeRadius   = serializedObject.FindProperty("nodeRadius");
        
        wireComponent    = target as CustomWire;
    }

    public override void OnInspectorGUI()
    {
        Color originContentColor = GUI.contentColor;

        serializedObject.Update();
        
        Undo.RecordObject(wireComponent, "CustomWire");

        EditorGUILayout.Space();
        GUI.contentColor = propLineRenderer.objectReferenceValue == null ? Color.red : GUI.contentColor;
        GUI.enabled = true;
        propLineRenderer.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Line Renderer", "设置目标线段"), propLineRenderer.objectReferenceValue, typeof(LineRenderer), true);
        GUI.enabled = propLineRenderer.objectReferenceValue != null;
        GUI.contentColor = originContentColor;

        EditorGUILayout.Space();
        expandConfiguration = EditorGUILayout.Foldout(expandConfiguration, "Wire Configuration");
        if (expandConfiguration)
        {
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(propWireType, new GUIContent("Wire Type", "Curve：曲线\nLinear：折线"));
            propSetOnUpdate.boolValue = EditorGUILayout.Toggle(new GUIContent("Set On Update", "实时更新操作"), propSetOnUpdate.boolValue);
            propCloseWire.boolValue   = EditorGUILayout.Toggle(new GUIContent("Close Wire", "是否闭合"), propCloseWire.boolValue);
            EditorGUI.indentLevel -= 1;
        }
        
        EditorGUILayout.Space();
        expandNodeManager = EditorGUILayout.Foldout(expandNodeManager, "Node Manager");
        if (expandNodeManager)
        {
            EditorGUI.indentLevel += 1;
            propDrawNodes.boolValue = EditorGUILayout.Toggle(new GUIContent("Draw Nodes", "绘制节点"), propDrawNodes.boolValue);
            GUI.enabled = propDrawNodes.boolValue;
            propNodeRadius.floatValue = EditorGUILayout.FloatField(new GUIContent("Node Radius", "节点半径"), propNodeRadius.floatValue);
            if (propNodeRadius.floatValue < 0)
                propNodeRadius.floatValue = 0;

            GUI.enabled = true;
            EditorGUILayout.BeginHorizontal();

            bCreateNode = GUILayout.Button(new GUIContent("Create Node", "创建一个新节点"));
            if (bCreateNode)
            {                
                wireComponent.AddNode();
                Undo.RegisterCreatedObjectUndo(wireComponent.Nodes[wireComponent.Nodes.Count - 1].gameObject, "Create Node");
            }

            GUI.enabled = wireComponent.Nodes.Count <= 0 ? false : true;
            GUI.backgroundColor = Color.red;
            bRemoveNodes = GUILayout.Button(new GUIContent("Remove All Nodes", "删除所有节点"));
            if (bRemoveNodes)
            {
                System.Collections.Generic.List<GameObject> nodeGOs = new System.Collections.Generic.List<GameObject>();
                for (int i = 0; i < wireComponent.Nodes.Count; i++)
                    nodeGOs.Add(wireComponent.GetNode(i).gameObject);

                wireComponent.RemoveAllNodes(false);

                for (int i = 0; i < nodeGOs.Count; i++)
                    Undo.DestroyObjectImmediate(nodeGOs[i]);
                nodeGOs.Clear();
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            
            if (wireComponent.Nodes.Count > 0)
            {
                expandNodesList = EditorGUILayout.Foldout(expandNodesList, "Node List");
                if (expandNodesList)
                {
                    for (int i = 0; i < wireComponent.Nodes.Count; i++)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));

                        GUILayout.Space(15);
                        GUILayout.Label(wireComponent.Nodes[i].name);

                        GUI.enabled = i == 0 ? false : true;
                        if (GUILayout.Button("▲", CustomWireGUI.ButtonStyle()))
                        {
                            wireComponent.NodeMoveUp(i);
                        }
                        GUI.enabled = true;

                        GUILayout.Space(3);
                        
                        GUI.enabled = i == wireComponent.Nodes.Count - 1 ? false : true;
                        if (GUILayout.Button("▼", CustomWireGUI.ButtonStyle()))
                        {
                            wireComponent.NodeMoveDown(i);
                        }
                        GUI.enabled = true;

                        GUILayout.Space(3);
                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("√", CustomWireGUI.ButtonStyle()))
                        {
                            Selection.activeTransform = wireComponent.Nodes[i].transform;
                        }

                        GUILayout.Space(3);
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", CustomWireGUI.ButtonStyle()))
                        {
                            GameObject nodeGameObject = wireComponent.GetNode(i).gameObject;
                            wireComponent.RemoveNode(i, false);
                            Undo.DestroyObjectImmediate(nodeGameObject);                            
                        }
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUI.indentLevel -= 1;
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        CustomWire wireComponent = target as CustomWire;

        if (!wireComponent.drawNodes)
            return;

        for (int i = 0; i < wireComponent.Nodes.Count; i++)
        {
            //3D Button
            Handles.color = new Color(0.5f, 0, 0, 0);
            float size = wireComponent.nodeRadius == 0 ? 0.05f : wireComponent.nodeRadius;
            bool click = Handles.Button(wireComponent.Nodes[i].transform.position, Quaternion.identity, size * 2, size, Handles.SphereHandleCap);
            if (click)
                Selection.activeTransform = wireComponent.Nodes[i].transform;

            //3D Label
            float yOffset = 0.01f;
            Vector3 lblPos = wireComponent.Nodes[i].transform.position + Vector3.up * yOffset;
            Handles.Label(lblPos, wireComponent.Nodes[i].name);
        }
    }
}
