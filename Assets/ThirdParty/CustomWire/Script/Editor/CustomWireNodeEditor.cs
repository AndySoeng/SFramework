/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWireNodeEditor.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: CustomWireNode类的自定义面板
************************************************************/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomWireNode))]
public class CustomWireNodeEditor : Editor{

    public override void OnInspectorGUI()
    {
        CustomWireNode nodeComponent = target as CustomWireNode;
        
        Undo.RecordObject(nodeComponent.wire, "CustomWire");

        //节点不在列表中，避免在新建、复制或删除后出现错误
        if (nodeComponent.IndexAtNodes == -1)
        {
            GUILayout.Space(10);
            GUILayout.Label("This node is not in the node list!");
            
            if (GUILayout.Button(new GUIContent("Add To Node List", "在该节点前添加新节点")))
            {
                nodeComponent.wire.AddNode(nodeComponent);
            }
        }
        else
        {
            //Front Node
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            bool bHasFrontNode = nodeComponent.FrontNode != null;
            GUILayout.Label("Front Node : " + (bHasFrontNode ? nodeComponent.FrontNode.name : "null"));
            if (bHasFrontNode)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("<<", CustomWireGUI.ButtonStyle()))
                {
                    Selection.activeTransform = nodeComponent.FrontNode.transform;
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            //Back Node
            EditorGUILayout.BeginHorizontal();

            bool bHasBackNode = nodeComponent.BackNode != null;
            GUILayout.Label("Back Node : " + (bHasBackNode ? nodeComponent.BackNode.name : "null"));
            if (bHasBackNode)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(">>", CustomWireGUI.ButtonStyle()))
                {
                    Selection.activeTransform = nodeComponent.BackNode.transform;
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Add Node At Front", "在该节点前添加新节点")))
            {
                nodeComponent.AddNode(true);
                Undo.RegisterCreatedObjectUndo(nodeComponent.FrontNode.gameObject, "Create Node");
                Selection.activeGameObject = nodeComponent.FrontNode.gameObject;
                Undo.RecordObject(Selection.activeGameObject, "Select Front Node");
            }

            if (GUILayout.Button(new GUIContent("Add Node At Back", "在该节点后添加新节点")))
            {
                nodeComponent.AddNode(false);
                Undo.RegisterCreatedObjectUndo(nodeComponent.BackNode.gameObject, "Create Node");
                Selection.activeGameObject = nodeComponent.BackNode.gameObject;
                Undo.RecordObject(Selection.activeGameObject, "Select Back Node");
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Go To Wire Object", "选择控制物体")))
            {
                Selection.activeTransform = nodeComponent.wire.transform;
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button(new GUIContent("Remove This Node", "删除当前节点")))
            {
                nodeComponent.wire.RemoveNode(nodeComponent, false);
                Undo.DestroyObjectImmediate(nodeComponent.gameObject);
                Selection.activeTransform = nodeComponent.wire.transform;
                Undo.RecordObject(Selection.activeTransform, "Remove Node");
            }
            GUI.backgroundColor = Color.white;
        }        
    }
}
