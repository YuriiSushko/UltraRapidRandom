using UnityEngine;

public class MovementValidator
{
    public MovementValidationResult Validate(
        BoardController board,
        Vector2Int currentTile,
        Vector2Int direction,
        MovementRuleData ruleData
    )
    {
        if (board == null || ruleData == null || direction == Vector2Int.zero)
        {
            return MovementValidationResult.Invalid();
        }

        if (!ruleData.CanUseDirection(direction))
        {
            return MovementValidationResult.Invalid();
        }

        Vector2Int targetTile = currentTile + direction;

        if (!board.IsTileValid(targetTile))
        {
            return MovementValidationResult.Invalid();
        }

        int targetTileID = board.GetTileID(targetTile);

        if (!ruleData.CanWalkOnTile(targetTileID, board.GetTileMaterial(targetTileID)))
        {
            return MovementValidationResult.Invalid();
        }

        return MovementValidationResult.Valid(
            targetTile,
            targetTileID,
            board.GetTileWorldPosition(targetTile)
        );
    }
}
