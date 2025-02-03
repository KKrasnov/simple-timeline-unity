using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Creates command instances based on command data type.
/// Maps BCommandData types to their corresponding BCommand implementations.
/// </summary>
public static class CommandFactory
{
    private static readonly Dictionary<Type, Func<TimelineAgent, BCommandData, IBCommand>> commandCreators = new()
    {
        { typeof(MoveToData), (agent, data) => new MoveToCommand(agent, (MoveToData)data) },
        { typeof(MoveAddData), (agent, data) => new MoveAddCommand(agent, (MoveAddData)data) },
        { typeof(RotateToData), (agent, data) => new RotateToCommand(agent, (RotateToData)data) },
        { typeof(RotateAddData), (agent, data) => new RotateAddCommand(agent, (RotateAddData)data) },
        { typeof(SetColorData), (agent, data) => new SetColorCommand(agent, (SetColorData)data) },
        { typeof(SendMessageData), (agent, data) => new SendMessageCommand(agent, (SendMessageData)data) }
    };

    public static IBCommand CreateCommand(TimelineAgent agent, BCommandData data)
    {
        if (commandCreators.TryGetValue(data.GetType(), out var creator))
        {
            return creator(agent, data);
        }
        return null;
    }
} 