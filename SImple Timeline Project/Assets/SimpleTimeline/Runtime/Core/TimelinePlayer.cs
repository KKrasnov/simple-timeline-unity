using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles timeline playback during runtime and system initialization.
/// Created automatically when entering play mode.
/// </summary>
public class TimelinePlayer : MonoBehaviour
{
    private static TimelinePlayer instance;
    private float currentTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        TimelineManager.Initialize(agents);
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        var go = new GameObject("[Timeline Player]");
        instance = go.AddComponent<TimelinePlayer>();
        DontDestroyOnLoad(go);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        TimelineManager.Initialize(agents);
    }

    private void Start()
    {
        currentTime = 0f;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        TimelineManager.SetTime(currentTime);
    }
} 