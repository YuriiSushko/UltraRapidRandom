using System.Collections.Generic;

public class MovementRuleData
{
    public static readonly MovementRuleData Default = new MovementRuleData(
        canMoveDiagonally: false,
        limitMovementToWalkableTiles: false,
        walkableTileIDs: null
    );

    private readonly HashSet<int> walkableTileIDs_;

    public bool CanMoveDiagonally { get; }
    public bool LimitMovementToWalkableTiles { get; }

    public MovementRuleData(
        bool canMoveDiagonally,
        bool limitMovementToWalkableTiles,
        IEnumerable<int> walkableTileIDs
    )
    {
        CanMoveDiagonally = canMoveDiagonally;
        LimitMovementToWalkableTiles = limitMovementToWalkableTiles;
        walkableTileIDs_ = walkableTileIDs != null
            ? new HashSet<int>(walkableTileIDs)
            : new HashSet<int>();
    }

    public bool CanWalkOnTile(int tileID)
    {
        if (!LimitMovementToWalkableTiles)
        {
            return true;
        }

        return walkableTileIDs_.Contains(tileID);
    }
}
