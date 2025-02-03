using UnityEngine;

[System.Serializable]
public class BCommandData
{
    public float startTime;
    public float endTime;

    public BCommandData()
    {
        startTime = 0f;
        endTime = 1f;
    }

    public BCommandData(float start, float end)
    {
        startTime = start;
        endTime = end;
    }

    public virtual bool isInstant => false;

    public virtual bool IsValid()
    {
        if (isInstant)
        {
            return true;
        }
        return endTime >= startTime;
    }
} 