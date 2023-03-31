using UnityEditor;
using UnityEngine;

public class SelectedToGuid : EditorWindow
{
    [MenuItem("AssetDatabase/ShowGUID")]

    static void Init()
    {
        if (Selection.objects.Length == 0) return;
        SelectedToGuid window = ScriptableObject.CreateInstance<SelectedToGuid>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 82);
        window.ShowPopup();
    }

    void OnGUI()
    {
        //string[] sel = Selection.assetGUIDs;
        EditorGUILayout.LabelField(Selection.objects[0].name, EditorStyles.wordWrappedLabel);
        EditorGUILayout.TextField(Selection.assetGUIDs[0], EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        if (GUILayout.Button("O.K.")) this.Close();
    }
}
