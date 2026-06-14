using UnityEngine;

public class MovementRuleResolver : MonoBehaviour
{
    [Header("Mock Movement Rules")]
    public bool canMoveDiagonally = false;

    [Tooltip("When enabled, movement is valid only if the target tile ID is in walkableTileIDs.")]
    public bool limitMovementToWalkableTiles = false;

    public int[] walkableTileIDs = new int[0];

    public MovementRuleData ResolveRules(Vector2Int currentTile, Vector2Int direction)
    {
        return new MovementRuleData(
            canMoveDiagonally,
            limitMovementToWalkableTiles,
            walkableTileIDs
        );
    }
}
