using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[CustomPropertyDrawer(typeof(ScenesSelection))]
public class ScenesSelectionDrawer : PropertyDrawer
{
    float h = 0;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty m_scenes = property.FindPropertyRelative("scenes");
        if (m_scenes != null) h = m_scenes.isExpanded ? 70 + m_scenes.arraySize*20: 36;
        BuildSceneSelections bss = property.serializedObject.targetObject as BuildSceneSelections;
        return h;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;
        // Draw label
        //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        EditorGUI.indentLevel++;
        SerializedProperty m_scenes = property.FindPropertyRelative("scenes");

        // Calculate rects
        //Debug.Log(h.ToString());
        float h = m_scenes.isExpanded ? 70 + m_scenes.arraySize * 20 : 36;
        var scenesRect = new Rect(position.x, position.y + 2, position.width, h); 
        var fileBtnRect = new Rect(position.x + position.width/2 - 54, position.y + h-40, 108, 20); 

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(scenesRect, m_scenes, GUIContent.none);
        if(m_scenes.isExpanded)
        if(GUI.Button(fileBtnRect, "to Build Scenes"))
        {
            int i = int.Parse(label.text.Replace("Element ", ""));
            BuildSceneSelections buildSceneSelections = property.serializedObject.targetObject as BuildSceneSelections;
            List<SceneAsset> scenes = buildSceneSelections.sceneSelections[i].scenes;
            // Find valid Scene paths and make a list of EditorBuildSettingsScene
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneAsset in scenes)
            {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(scenePath))
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();

    }
}
