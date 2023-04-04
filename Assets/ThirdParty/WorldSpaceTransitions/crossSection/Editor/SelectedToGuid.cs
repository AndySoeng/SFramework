using UnityEditor;
using UnityEngine;

public class SelectedToGuid : EditorWindow
{
    private static SelectedToGuid window;
    [MenuItem("Tools/WorldSpaceTransitions/AssetDatabase/ShowGUID")]
    static void Init()
    {
        if (window != null) return;
        if (Selection.objects.Length == 0) return;
        window = ScriptableObject.CreateInstance<SelectedToGuid>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 82);
        window.ShowPopup();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Close")) this.Close();
        if (Selection.objects.Length == 0) return;
        if (Selection.assetGUIDs.Length == 0) return;
        //string[] sel = Selection.assetGUIDs;
        EditorGUILayout.LabelField(Selection.objects[0].name, EditorStyles.wordWrappedLabel);
        EditorGUILayout.TextField(Selection.assetGUIDs[0], EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
    }
}
