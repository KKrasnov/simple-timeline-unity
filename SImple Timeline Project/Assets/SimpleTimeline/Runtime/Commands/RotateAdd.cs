using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RotateAddData : BCommandData
{
    public Vector3 startRotation;
    public Vector3 offset;

    public RotateAddData() : base() 
    {
        startRotation = Vector3.zero;
        offset = Vector3.zero;
    }

    public RotateAddData(float start, float end, Vector3 offset) : base(start, end)
    {
        startRotation = Vector3.zero;
        this.offset = offset;
    }
}

public class RotateAddCommand : BCommand<RotateAddData>
{
    public RotateAddCommand(TimelineAgent agent, RotateAddData data) : base(agent, data) { }

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
        
        state["rotation"] = data.startRotation + data.offset;
    }

    public override void Set(float currentTime)
    {
        if (currentTime < data.startTime)
            return;

        if (currentTime >= data.endTime)
        {
            agent.transform.eulerAngles = data.startRotation + data.offset;
            return;
        }

        float t = (currentTime - data.startTime) / (data.endTime - data.startTime);
        agent.transform.eulerAngles = Vector3.Lerp(data.startRotation, data.startRotation + data.offset, t);
    }
} 