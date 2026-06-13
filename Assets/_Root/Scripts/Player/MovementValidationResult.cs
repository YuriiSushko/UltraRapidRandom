using UnityEngine;

public struct MovementValidationResult
{
    public bool IsValid { get; }
    public Vector2Int TargetTile { get; }
    public int TargetTileID { get; }
    public Vector3 TargetWorldPosition { get; }

    private MovementValidationResult(
        bool isValid,
        Vector2Int targetTile,
        int targetTileID,
        Vector3 targetWorldPosition
    )
    {
        IsValid = isValid;
        TargetTile = targetTile;
        TargetTileID = targetTileID;
        TargetWorldPosition = targetWorldPosition;
    }

    public static MovementValidationResult Invalid()
    {
        return new MovementValidationResult(
            false,
            new Vector2Int(-1, -1),
            -1,
            Vector3.zero
        );
    }

    public static MovementValidationResult Valid(
        Vector2Int targetTile,
        int targetTileID,
        Vector3 targetWorldPosition
    )
    {
        return new MovementValidationResult(
            true,
            targetTile,
            targetTileID,
            targetWorldPosition
        );
    }
}
