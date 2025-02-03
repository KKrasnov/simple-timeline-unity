using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MoveToData : BCommandData
{
    public Vector3 startPosition;
    public Vector3 targetPosition;

    public MoveToData() : base() 
    {
        startPosition = Vector3.zero;
        targetPosition = Vector3.zero;
    }

    public MoveToData(float start, float end, Vector3 target) : base(start, end)
    {
        startPosition = Vector3.zero;
        targetPosition = target;
    }
}

public class MoveToCommand : BCommand<MoveToData>
{
    public MoveToCommand(TimelineAgent agent, MoveToData data) : base(agent, data) { }

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
        
        state["position"] = data.targetPosition;
    }

    public override void Set(float currentTime)
    {
        if (currentTime < data.startTime)
            return;

        if (currentTime >= data.endTime)
        {
            agent.transform.position = data.targetPosition;
            return;
        }

        float t = (currentTime - data.startTime) / (data.endTime - data.startTime);
        agent.transform.position = Vector3.Lerp(data.startPosition, data.targetPosition, t);
    }
} 