using UnitaskXNode.Base;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomEditor(typeof(StepGraphComponent))]
 public class StepGraphComponentEditor : Editor
 {
     public override void OnInspectorGUI()
     {
         base.OnInspectorGUI();
         StepGraphComponent step = target as StepGraphComponent;
         if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
         {
             if (step.graph==null)
             {
                 step.graph = CreateInstance(typeof(StepGraph)) as StepGraph;
             }
             NodeEditorWindow.Open((target as StepGraphComponent).graph as XNode.NodeGraph);
         }
     }
     
     
 }