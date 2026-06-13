public struct PlayerActionInput
{
    public static readonly PlayerActionInput None = new PlayerActionInput(
        hasPassiveAction: false,
        hasActiveAction: false
    );

    public bool HasPassiveAction { get; }
    public bool HasActiveAction { get; }
    public bool HasAnyAction => HasPassiveAction || HasActiveAction;

    public PlayerActionInput(bool hasPassiveAction, bool hasActiveAction)
    {
        HasPassiveAction = hasPassiveAction;
        HasActiveAction = hasActiveAction;
    }
}
