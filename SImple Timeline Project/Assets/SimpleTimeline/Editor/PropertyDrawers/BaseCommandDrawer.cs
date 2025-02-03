using UnityEditor;
using UnityEngine;

public class BaseCommandDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
            TimelineManager.RecalculateAll();
        }
    }
} 