using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SetLocalKeyword : EditorWindow
{
    string kwd = "CLIP_PLANE";
    static Material m = null;

    [MenuItem("Tools/Material/Set Local Keyword")]

    static void Init()
    {
        if (!Selection.activeObject) return;
        if (Selection.activeObject.GetType() == typeof(Material))
        {
            m = (Material)Selection.activeObject;
        }
        else if (Selection.activeObject.GetType() == typeof(GameObject))
        {
            GameObject g = (GameObject)Selection.activeObject;
            if (!g.GetComponent<Renderer>()) return;
            m = g.GetComponent<Renderer>().sharedMaterial;
        }
        else
        {
            return;
        }
        SetLocalKeyword window = ScriptableObject.CreateInstance<SetLocalKeyword>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 320, 82);
        window.ShowPopup();
    }

    void OnGUI()
    {
        kwd = GUILayout.TextField(kwd);
        List<string> kwdList = m.shader.keywordSpace.keywordNames.ToList();
        string msg = "";
        if (!kwdList.Contains(kwd))
        {
            msg = "the material does not contain " + kwd + " keyword";
            EditorGUILayout.LabelField(msg, EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("O.K.")) this.Close();
        }
        else
        {
            bool en = m.IsKeywordEnabled(kwd);
            msg = (en ? "disable " : "enable ") + kwd + " keyword";
            EditorGUILayout.LabelField(msg, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("O.K."))
            {
                if (en) m.DisableKeyword(kwd); else m.EnableKeyword(kwd);
                this.Close();
            }
        }
    }

}
