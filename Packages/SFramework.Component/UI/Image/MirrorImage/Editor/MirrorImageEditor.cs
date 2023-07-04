#if  UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace SFramework
{
    [CustomEditor(typeof(MirrorImage))]
    [CanEditMultipleObjects]
    public class MirrorImageEditor : Editor
    {
        private MirrorImage _mirrorImage;

        private void OnEnable()
        {
            _mirrorImage = serializedObject.targetObject as MirrorImage;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            _mirrorImage._mirrorType = (MirrorType) EditorGUILayout.EnumPopup("镜像类型", _mirrorImage._mirrorType);
            if (GUILayout.Button("设置默认图片大小"))
            {
                Undo.RecordObject(_mirrorImage.transform.GetComponent<RectTransform>(), "");
                _mirrorImage.SetNativeSize();
            }



            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                _mirrorImage.SetVerticesDirty();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
