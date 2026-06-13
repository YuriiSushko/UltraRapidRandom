using UnityEngine;

public class MovementRuleResolver : MonoBehaviour
{
    public MovementRuleData ResolveRules(PlayerController player, Vector2Int direction)
    {
        if (player == null)
        {
            return MovementRuleData.Default;
        }

        return new MovementRuleData(
            player.canMoveDiagonally,
            player.limitMovementToWalkableTiles,
            player.walkableTileIDs
        );
    }
}
