using UnityEngine;
using UnityEditor;

public static class TimelineMenu
{
    [MenuItem("GameObject/Create Timeline Agent", false, 10)]
    private static void CreateTimelineAgent(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Timeline Agent");
        go.AddComponent<TimelineAgent>();
        
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create Timeline Agent");
        Selection.activeObject = go;
    }
} 