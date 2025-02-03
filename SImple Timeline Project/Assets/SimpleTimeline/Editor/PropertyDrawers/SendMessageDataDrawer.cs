using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SendMessageData))]
public class SendMessageDataDrawer : BaseCommandDrawer
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
        
        // Foldout, title and time input
        Rect foldoutRect = new Rect(contentRect.x + 12.5f, contentRect.y, 2.5f, lineHeight);
        Rect titleRect = new Rect(foldoutRect.xMax, contentRect.y, contentRect.width * 0.5f - foldoutRect.width, lineHeight);

        // Right side with time input and delete button
        float timeInputWidth = 40f;
        float deleteButtonWidth = 20f;
        float headerSpacing = 2f;

        // Delete button
        var deleteButtonRect = new Rect(contentRect.xMax - deleteButtonWidth, contentRect.y, deleteButtonWidth, lineHeight);

        // Time input
        float rightSideX = titleRect.xMax;

        // Time input
        var timeRect = new Rect(rightSideX, contentRect.y, timeInputWidth, lineHeight);
        var startTimeProp = property.FindPropertyRelative("startTime");
        var endTimeProp = property.FindPropertyRelative("endTime");
        startTimeProp.floatValue = float.Parse(EditorGUI.TextField(
            timeRect, 
            startTimeProp.floatValue.ToString("F2")
        ));
        endTimeProp.floatValue = startTimeProp.floatValue; // Instant command, end time equals start time

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
        EditorGUI.LabelField(titleRect, "Send Message Command", EditorStyles.boldLabel);

        if (foldoutStates[propertyPath])
        {
            Rect sliderRect = new Rect(contentRect.x, titleRect.y + lineHeight + spacing, contentRect.width, lineHeight);
            Rect messageRect = new Rect(contentRect.x, sliderRect.y + lineHeight + spacing, contentRect.width, lineHeight);

            EditorGUI.BeginChangeCheck();
            
            float time = startTimeProp.floatValue;
            time = EditorGUI.Slider(sliderRect, "Time", time, 0f, 10f);
            startTimeProp.floatValue = time;
            endTimeProp.floatValue = time; // Instant command, end time equals start time
            
            EditorGUI.PropertyField(messageRect, property.FindPropertyRelative("message"));

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