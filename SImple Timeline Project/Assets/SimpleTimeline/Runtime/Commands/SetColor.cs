using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SetColorData : BCommandData
{
    public Color startColor;
    public Color targetColor;

    public SetColorData() : base() 
    {
        startColor = Color.white;
        targetColor = Color.white;
    }

    public SetColorData(float start, float end, Color target) : base(start, end)
    {
        startColor = Color.white;
        targetColor = target;
    }
}

public class SetColorCommand : BCommand<SetColorData>
{
    private Renderer renderer;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    public SetColorCommand(TimelineAgent agent, SetColorData data) : base(agent, data) 
    {
        renderer = agent.GetComponent<Renderer>();
        if (renderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    public override void Setup(Dictionary<string, object> startStates)
    {
        if (renderer == null) return;

        if (!startStates.ContainsKey("color"))
        {
            startStates["color"] = renderer.sharedMaterial.GetColor(ColorProperty);
        }
    }

    public override void Recalculate(Dictionary<string, object> state)
    {
        if (renderer == null) return;

        if (state.TryGetValue("color", out object colorObj))
        {
            data.startColor = (Color)colorObj;
        }
        else
        {
            data.startColor = renderer.sharedMaterial.GetColor(ColorProperty);
        }
        
        state["color"] = data.targetColor;
    }

    public override void Set(float currentTime)
    {
        if (renderer == null) return;
        if (currentTime < data.startTime) return;

        renderer.GetPropertyBlock(propertyBlock);

        if (currentTime >= data.endTime)
        {
            propertyBlock.SetColor(ColorProperty, data.targetColor);
            renderer.SetPropertyBlock(propertyBlock);
            return;
        }

        float t = (currentTime - data.startTime) / (data.endTime - data.startTime);
        propertyBlock.SetColor(ColorProperty, Color.Lerp(data.startColor, data.targetColor, t));
        renderer.SetPropertyBlock(propertyBlock);
    }
} 