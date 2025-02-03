using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SendMessageData : BCommandData
{
    public string message;

    public SendMessageData() : base() 
    {
        message = string.Empty;
    }

    public SendMessageData(float start, float end, string message) : base(start, end)
    {
        this.message = message;
    }
}

/// <summary>
/// Basic command that demonstrates how to call a method on an agent.
/// Uses Unity's SendMessage to invoke a method by name at a specific time.
/// </summary>
public class SendMessageCommand : BCommand<SendMessageData>
{
    private bool executed;

    public SendMessageCommand(TimelineAgent agent, SendMessageData data) : base(agent, data) 
    {
        executed = false;
    }

    public override void Setup(Dictionary<string, object> startStates)
    {
        // No initial state needed for instant message command
    }

    public override void Recalculate(Dictionary<string, object> state)
    {
        executed = false;
    }

    public override void Set(float currentTime)
    {
        if (currentTime < data.startTime || executed)
            return;

        if (currentTime >= data.startTime)
        {
            agent.SendMessage(data.message, SendMessageOptions.DontRequireReceiver);
            executed = true;
        }
    }
} 