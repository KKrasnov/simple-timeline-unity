using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RotateToData : BCommandData
{
    public Vector3 startRotation;
    public Vector3 targetRotation;

    public RotateToData() : base() 
    {
        startRotation = Vector3.zero;
        targetRotation = Vector3.zero;
    }

    public RotateToData(float start, float end, Vector3 target) : base(start, end)
    {
        startRotation = Vector3.zero;
        targetRotation = target;
    }
}

public class RotateToCommand : BCommand<RotateToData>
{
    public RotateToCommand(TimelineAgent agent, RotateToData data) : base(agent, data) { }

    public override void Setup(Dictionary<string, object> startStates)
    {
        if (!startStates.ContainsKey("rotation"))
        {
            startStates["rotation"] = agent.transform.eulerAngles;
        }
    }

    public override void Recalculate(Dictionary<string, object> state)
    {
        if (state.TryGetValue("rotation", out object rotObj))
        {
            data.startRotation = (Vector3)rotObj;
        }
        else
        {
            data.startRotation = agent.transform.eulerAngles;
        }
        
        state["rotation"] = data.targetRotation;
    }

    public override void Set(float currentTime)
    {
        if (currentTime < data.startTime)
            return;

        if (currentTime >= data.endTime)
        {
            agent.transform.eulerAngles = data.targetRotation;
            return;
        }

        float t = (currentTime - data.startTime) / (data.endTime - data.startTime);
        agent.transform.eulerAngles = Vector3.Lerp(data.startRotation, data.targetRotation, t);
    }
} 