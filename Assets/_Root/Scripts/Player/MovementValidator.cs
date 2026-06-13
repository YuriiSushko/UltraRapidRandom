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

        if (IsDiagonal(direction) && !ruleData.CanMoveDiagonally)
        {
            return MovementValidationResult.Invalid();
        }

        Vector2Int targetTile = currentTile + direction;

        if (!board.IsTileValid(targetTile))
        {
            return MovementValidationResult.Invalid();
        }

        int targetTileID = board.GetTileID(targetTile);

        if (!ruleData.CanWalkOnTile(targetTileID))
        {
            return MovementValidationResult.Invalid();
        }

        return MovementValidationResult.Valid(
            targetTile,
            targetTileID,
            board.GetTileWorldPosition(targetTile)
        );
    }

    private bool IsDiagonal(Vector2Int direction)
    {
        return direction.x != 0 && direction.y != 0;
    }
}
