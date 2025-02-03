public interface ICommand
{
    void Initialize(TimelineAgent agent);
    void Execute(float currentTime);
    bool IsValid();
} 