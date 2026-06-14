public class ActiveGoal
{
    public GoalType Type;
    public string Description;
    public int TargetCount;

    public int CurrentCount;

    public ActiveGoal(GoalType type, string description, int targetCount = 0)
    {
        Type = type;
        Description = description;
        TargetCount = targetCount;
        CurrentCount = 0;
    }
}