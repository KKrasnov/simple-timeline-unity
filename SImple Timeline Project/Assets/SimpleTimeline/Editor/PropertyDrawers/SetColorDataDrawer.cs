using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SetColorData))]
public class SetColorDataDrawer : BaseCommandDrawer
{
    private Dictionary<string, bool> foldoutStates = new();

    private float GetBoxHeight(bool isFolded)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return isFolded ? lineHeight * 4 + spacing * 7 : lineHeight + spacing * 3;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        string propertyPath = property.propertyPath;
        if (!foldoutStates.ContainsKey(propertyPath))
        {
            foldoutStates[propertyPath] = true;
        }
        
        // Dark background box
        float boxHeight = GetBoxHeight(foldoutStates[propertyPath]);
        Rect boxRect = new Rect(position.x, position.y, position.width, boxHeight);
        EditorGUI.DrawRect(boxRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));
        
        // Properties inside box
        float boxPadding = 4f;
        Rect contentRect = new Rect(boxRect.x + boxPadding, boxRect.y + boxPadding, boxRect.width - boxPadding * 2, boxRect.height - boxPadding * 2);
        
        // Foldout, title and time inputs
        Rect foldoutRect = new Rect(contentRect.x + 12.5f, contentRect.y, 2.5f, lineHeight);
        Rect titleRect = new Rect(foldoutRect.xMax, contentRect.y, contentRect.width * 0.5f - foldoutRect.width, lineHeight);

        // Right side with time inputs and delete button
        float timeInputWidth = 40f;
        float dashWidth = 15f;
        float deleteButtonWidth = 20f;
        float headerSpacing = 2f;

        // Delete button
        var deleteButtonRect = new Rect(contentRect.xMax - deleteButtonWidth, contentRect.y, deleteButtonWidth, lineHeight);

        // Time inputs
        float rightSideX = titleRect.xMax;

        // Start time input
        var startTimeRect = new Rect(rightSideX, contentRect.y, timeInputWidth, lineHeight);
        var startTimeProp = property.FindPropertyRelative("startTime");
        startTimeProp.floatValue = float.Parse(EditorGUI.TextField(
            startTimeRect, 
            startTimeProp.floatValue.ToString("F2")
        ));

        // Dash between inputs
        var dashRect = new Rect(startTimeRect.xMax + headerSpacing, contentRect.y, dashWidth, lineHeight);
        EditorGUI.LabelField(dashRect, "-");

        // End time input
        var endTimeRect = new Rect(dashRect.xMax + headerSpacing, contentRect.y, timeInputWidth, lineHeight);
        var endTimeProp = property.FindPropertyRelative("endTime");
        endTimeProp.floatValue = float.Parse(EditorGUI.TextField(
            endTimeRect, 
            endTimeProp.floatValue.ToString("F2")
        ));

        // Delete button
        if (GUI.Button(deleteButtonRect, "-", EditorStyles.miniButton))
        {
            var commandsList = property.serializedObject.FindProperty("commandsData");
            for (int i = 0; i < commandsList.arraySize; i++)
            {
                if (commandsList.GetArrayElementAtIndex(i).propertyPath == property.propertyPath)
                {
                    var agent = property.serializedObject.targetObject as TimelineAgent;
                    var command = agent.commandsData[i];
                    
                    Undo.RecordObject(agent, "Remove Timeline Command");
                    TimelineManager.OnCommandRemoved(agent, command);
                    
                    commandsList.DeleteArrayElementAtIndex(i);
                    property.serializedObject.ApplyModifiedProperties();
                    TimelineManager.RecalculateAll();
                    return;
                }
            }
        }

        foldoutStates[propertyPath] = EditorGUI.Foldout(foldoutRect, foldoutStates[propertyPath], "", true);
        EditorGUI.LabelField(titleRect, "Set Color Command", EditorStyles.boldLabel);

        if (foldoutStates[propertyPath])
        {
            Rect timeRect = new Rect(contentRect.x, titleRect.y + lineHeight + spacing, contentRect.width, lineHeight);
            Rect colorRect = new Rect(contentRect.x, timeRect.y + lineHeight + spacing, contentRect.width - 50, lineHeight);
            Rect setButtonRect = new Rect(colorRect.xMax + 5, colorRect.y, 45, lineHeight);

            EditorGUI.BeginChangeCheck();
            
            float startTime = startTimeProp.floatValue;
            float endTime = endTimeProp.floatValue;
            EditorGUI.MinMaxSlider(timeRect, ref startTime, ref endTime, 0f, 10f);
            startTimeProp.floatValue = startTime;
            endTimeProp.floatValue = endTime;
            
            EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("targetColor"));

            if (GUI.Button(setButtonRect, "Set"))
            {
                var agent = property.serializedObject.targetObject as TimelineAgent;
                if (agent != null)
                {
                    var renderer = agent.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var propertyBlock = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(propertyBlock);
                        var targetColorProp = property.FindPropertyRelative("targetColor");
                        targetColorProp.colorValue = propertyBlock.GetColor("_Color");
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string propertyPath = property.propertyPath;
        if (!foldoutStates.ContainsKey(propertyPath))
        {
            foldoutStates[propertyPath] = true;
        }

        return GetBoxHeight(foldoutStates[propertyPath]);
    }
} 