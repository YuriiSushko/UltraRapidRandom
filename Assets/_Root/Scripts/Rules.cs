namespace Assets._Root.Scripts
{
    public enum RuleCategory
    {
        Empty,
        Move,
        Environment,
        Chaotic,
        Goal
    }

    public enum RuleSubtype
    {
        None,
        MoveDir,
        MoveSpd,
        MoveDist
    }

    public enum RuleActivationType
    {
        Passive,
        Active
    }
}
