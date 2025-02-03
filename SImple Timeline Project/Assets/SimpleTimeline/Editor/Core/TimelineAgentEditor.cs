using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TimelineAgent))]
public class TimelineAgentEditor : Editor
{
    private TimelineAgent agent;
    private SerializedProperty commandsDataProp;

    private void OnEnable()
    {
        agent = target as TimelineAgent;
        if (agent != null)
        {
            commandsDataProp = serializedObject.FindProperty("commandsData");
        }
    }

    public override void OnInspectorGUI()
    {
        if (agent == null || commandsDataProp == null)
        {
            EditorGUILayout.HelpBox("Timeline Agent not found", MessageType.Error);
            return;
        }

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Timeline Commands", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Move/Move To"), false, () => AddCommand<MoveToData>());
            menu.AddItem(new GUIContent("Move/Move Add"), false, () => AddCommand<MoveAddData>());
            menu.AddItem(new GUIContent("Rotate/Rotate To"), false, () => AddCommand<RotateToData>());
            menu.AddItem(new GUIContent("Rotate/Rotate Add"), false, () => AddCommand<RotateAddData>());
            menu.AddItem(new GUIContent("Color/Set Color"), false, () => AddCommand<SetColorData>());
            menu.AddItem(new GUIContent("Other/Send Message"), false, () => AddCommand<SendMessageData>());
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < commandsDataProp.arraySize; i++)
        {
            EditorGUILayout.PropertyField(commandsDataProp.GetArrayElementAtIndex(i));
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            TimelineManager.RecalculateAll();
        }
    }

    private void AddCommand<T>() where T : BCommandData, new()
    {
        var command = new T();
        Undo.RecordObject(agent, "Add Timeline Command");
        
        agent.commandsData.Add(command);
        EditorUtility.SetDirty(agent);
        
        TimelineManager.OnCommandAdded(agent, command);
        serializedObject.ApplyModifiedProperties();
    }
}