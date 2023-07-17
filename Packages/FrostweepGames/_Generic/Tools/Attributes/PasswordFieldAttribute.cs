﻿#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FrostweepGames.Plugins
{
    public class PasswordFieldAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PasswordFieldAttribute))]
    public class PasswordFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.stringValue = EditorGUI.PasswordField(position, label, property.stringValue);
        }
    }
#endif
}