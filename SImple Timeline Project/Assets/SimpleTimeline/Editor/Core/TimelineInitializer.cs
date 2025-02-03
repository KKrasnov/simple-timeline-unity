using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles TimelineManager initialization for both editor and play mode.
/// </summary>
[InitializeOnLoad]
public static class TimelineInitializer
{
    static TimelineInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        
        // Initial initialization
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        TimelineManager.Initialize(agents);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            var agents = Object.FindObjectsOfType<TimelineAgent>();
            TimelineManager.Initialize(agents);
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        TimelineManager.Initialize(agents);
    }
} 