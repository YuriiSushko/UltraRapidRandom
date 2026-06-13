using Assets._Root.Scripts;

public enum RuleCategory
{
    Move,
    Environment,
    WildCard
}

public abstract class GameRule
{
    public string RuleName { get; protected set; }
    public RuleCategory Category { get; protected set; }

    public abstract void OnEquip(PlayerController player);

    public abstract void OnRemove(PlayerController player);

    public virtual void OnUpdate(PlayerController player) { }
}