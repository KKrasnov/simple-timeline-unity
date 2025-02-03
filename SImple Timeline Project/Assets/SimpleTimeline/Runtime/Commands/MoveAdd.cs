using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MoveAddData : BCommandData
{
    public Vector3 startPosition;
    public Vector3 offset;

    public MoveAddData() : base() 
    {
        startPosition = Vector3.zero;
        offset = Vector3.zero;
    }

    public MoveAddData(float start, float end, Vector3 offset) : base(start, end)
    {
        startPosition = Vector3.zero;
        this.offset = offset;
    }
}

public class MoveAddCommand : BCommand<MoveAddData>
{
    public MoveAddCommand(TimelineAgent agent, MoveAddData data) : base(agent, data) { }

    public override void Setup(Dictionary<string, object> startStates)
    {
        if (!startStates.ContainsKey("position"))
        {
            startStates["position"] = agent.transform.position;
        }
    }

    public override void Recalculate(Dictionary<string, object> state)
    {
        if (state.TryGetValue("position", out object posObj))
        {
            data.startPosition = (Vector3)posObj;//TODO: unboxing
        }
        else
        {
            data.startPosition = agent.transform.position;
        }
        
        state["position"] = data.startPosition + data.offset;
    }

    public override void Set(float currentTime)
    {
        if (currentTime < data.startTime)
            return;

        if (currentTime >= data.endTime)
        {
            agent.transform.position = data.startPosition + data.offset;
            return;
        }

        float t = (currentTime - data.startTime) / (data.endTime - data.startTime);
        agent.transform.position = Vector3.Lerp(data.startPosition, data.startPosition + data.offset, t);
    }
} 