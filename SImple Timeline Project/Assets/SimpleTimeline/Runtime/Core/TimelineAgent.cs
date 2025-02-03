using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// MonoBehaviour component that connects GameObject to the timeline system.
/// Stores command configuration data and provides access to the GameObject components.
/// </summary>
public class TimelineAgent : MonoBehaviour
{ 
    [SerializeReference] public List<BCommandData> commandsData = new();

    private void Awake()
    {
        TimelineManager.RegisterAgent(this);
    }

    private void OnValidate()
    {
        TimelineManager.RegisterAgent(this);
    }

    private void OnDestroy()
    {
        TimelineManager.UnregisterAgent(this);
    }

    private void OnEnable()
    {
        TimelineManager.RegisterAgent(this);
    }

    private void OnDisable()
    {
        TimelineManager.UnregisterAgent(this);
    }
} 