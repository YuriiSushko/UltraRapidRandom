using System.Collections.Generic;

public class PlayerActionResult
{
    public static readonly PlayerActionResult Empty = new PlayerActionResult(
        BoardMutationData.Empty,
        new List<PlayerEffect>()
    );

    public BoardMutationData BoardMutations { get; }
    public List<PlayerEffect> PlayerEffects { get; }
    public bool HasAnyResult => BoardMutations.HasMutations || PlayerEffects.Count > 0;

    public PlayerActionResult(
        BoardMutationData boardMutations,
        List<PlayerEffect> playerEffects
    )
    {
        BoardMutations = boardMutations != null
            ? boardMutations
            : BoardMutationData.Empty;
        PlayerEffects = playerEffects != null
            ? playerEffects
            : new List<PlayerEffect>();
    }
}
