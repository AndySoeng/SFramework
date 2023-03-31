 using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(BuildSceneSelection))]
public class BuildSceneSelectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BuildSceneSelection selectionScript = (BuildSceneSelection)target;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("to Build Scenes", GUILayout.Width(240)))
        {
            List<SceneAsset> scenes = selectionScript.scenes;
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
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("from Build Scenes", GUILayout.Width(240)))
        {
            List<SceneAsset> scenes = selectionScript.scenes;
            EditorBuildSettingsScene[] editorscenes = EditorBuildSettings.scenes;
            foreach (var buildSceneAsset in editorscenes)
            {
                string path = buildSceneAsset.path;
                SceneAsset s = (SceneAsset)AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset));
                scenes.Add(s);
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }
}


