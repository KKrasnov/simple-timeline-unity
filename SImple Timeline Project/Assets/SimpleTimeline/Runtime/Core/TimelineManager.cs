using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages timeline system execution and agent registration.
/// Creates and executes commands based on agent configurations.
/// Reinitializes with scene/play mode changes.
/// </summary>
public class TimelineManager
{
    private static TimelineManager instance;
    public static TimelineManager Instance => instance ??= new TimelineManager();

    private readonly HashSet<TimelineAgent> agents = new();
    private readonly Dictionary<TimelineAgent, List<IBCommand>> commands = new();
    private readonly Dictionary<TimelineAgent, Dictionary<string, object>> startStates = new();
    private readonly Dictionary<TimelineAgent, Dictionary<string, object>> currentStates = new();

    public static void Initialize(IEnumerable<TimelineAgent> sceneAgents)
    {
        instance = new TimelineManager();
        foreach (var agent in sceneAgents)
        {
            RegisterAgent(agent);
        }
    }

    // Most frequent: Recalculate all commands when their parameters change
    public static void RecalculateAll()
    {
        foreach (var agent in Instance.agents)
        {
            Instance.RecalculateAgentCommands(agent);
        }
    }

    // Less frequent: Handle command add/remove in edit mode
    public static void OnCommandAdded(TimelineAgent agent, BCommandData data)
    {
        #if UNITY_EDITOR
        if (Application.isPlaying) return;

        var command = CommandFactory.CreateCommand(agent, data);
        if (command == null) return;

        var agentCommands = Instance.commands[agent];
        agentCommands.Add(command);
        agentCommands.Sort((a, b) => a.BData.startTime.CompareTo(b.BData.startTime));
        
        Instance.RecalculateAgentCommands(agent);
        #endif
    }

    public static void OnCommandRemoved(TimelineAgent agent, BCommandData data)
    {
        #if UNITY_EDITOR
        if (Application.isPlaying) return;

        var agentCommands = Instance.commands[agent];
        agentCommands.RemoveAll(cmd => cmd.BData == data);
        Instance.RecalculateAgentCommands(agent);
        #endif
    }

    // Agent registration
    public static void RegisterAgent(TimelineAgent agent)
    {
        if (Instance.agents.Contains(agent))
            return;
            
        Instance.agents.Add(agent);
        Instance.commands[agent] = new List<IBCommand>();
        Instance.startStates[agent] = new Dictionary<string, object>();
        Instance.currentStates[agent] = new Dictionary<string, object>();
        Instance.RecreateCommands(agent);
    }

    public static void UnregisterAgent(TimelineAgent agent)
    {
        Instance.agents.Remove(agent);
        Instance.commands.Remove(agent);
        Instance.startStates.Remove(agent);
        Instance.currentStates.Remove(agent);
    }

    private void RecreateCommands(TimelineAgent agent)
    {
        if (!commands.TryGetValue(agent, out var agentCommands))
        {
            agentCommands = new List<IBCommand>();
            commands[agent] = agentCommands;
        }
        
        agentCommands.Clear();

        var sortedData = agent.commandsData
            .OrderBy(data => data.startTime)
            .ToList();

        foreach (var data in sortedData)
        {
            var command = CommandFactory.CreateCommand(agent, data);
            if (command != null)
            {
                agentCommands.Add(command);
            }
        }

        var agentStartStates = new Dictionary<string, object>();
        
        foreach (var command in agentCommands)
        {
            command.Setup(agentStartStates);
        }

        startStates[agent] = new Dictionary<string, object>(agentStartStates);
        currentStates[agent] = agentStartStates;
        
        RecalculateAgentCommands(agent);
    }

    private void RecalculateAgentCommands(TimelineAgent agent)
    {
        if (currentStates.TryGetValue(agent, out var states)) states.Clear();
        
        
        foreach (var kvp in startStates[agent])
        {
            states[kvp.Key] = kvp.Value;
        }

        foreach (var command in commands[agent])
        {
            command.Recalculate(states);
        }
    }

    public static void SetTime(float time)
    {
        foreach (var agentCommands in Instance.commands.Values)
        {
            foreach (var command in agentCommands)
            {
                command.Set(time);
            }
        }
    }
} 