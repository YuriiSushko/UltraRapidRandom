public enum PlayerEffectType
{
    Hide,
    SwapTiles
}

public struct PlayerEffect
{
    public PlayerEffectType Type { get; }
    public PlayerController Source { get; }
    public PlayerController Target { get; }
    public float Duration { get; }

    private PlayerEffect(
        PlayerEffectType type,
        PlayerController source,
        PlayerController target,
        float duration
    )
    {
        Type = type;
        Source = source;
        Target = target;
        Duration = duration;
    }

    public static PlayerEffect Hide(
        PlayerController source,
        PlayerController target,
        float duration
    )
    {
        return new PlayerEffect(
            PlayerEffectType.Hide,
            source,
            target,
            duration
        );
    }

    public static PlayerEffect Swap(
        PlayerController source,
        PlayerController target
    )
    {
        return new PlayerEffect(
            PlayerEffectType.SwapTiles,
            source,
            target,
            0f
        );
    }
}
