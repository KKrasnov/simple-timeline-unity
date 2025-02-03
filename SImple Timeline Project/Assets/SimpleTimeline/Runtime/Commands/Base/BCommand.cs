using UnityEngine;
using System.Collections.Generic;

public interface IBCommand
{
    void Set(float currentTime);
    void Setup(Dictionary<string, object> startStates);
    void Recalculate(Dictionary<string, object> state);
    BCommandData BData { get; }
}

public abstract class BCommand<T> : IBCommand where T : BCommandData
{
    protected readonly TimelineAgent agent;
    protected readonly T data;

    public T Data => data;
    public BCommandData BData => data;

    protected BCommand(TimelineAgent agent, T data)
    {
        this.agent = agent;
        this.data = data;
    }

    public abstract void Set(float currentTime);
    public abstract void Setup(Dictionary<string, object> startStates);
    public abstract void Recalculate(Dictionary<string, object> state);
} 