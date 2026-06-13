using UnityEngine;

public abstract class GameGoal
{
    public string GoalDescription { get; protected set; }

    public abstract bool CheckCompletion(PlayerController player);

    public abstract void OnAssigned(PlayerController player);
    public abstract void OnAchieved(PlayerController player);
}