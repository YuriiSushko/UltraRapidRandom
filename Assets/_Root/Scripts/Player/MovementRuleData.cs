using UnityEngine;

public class MovementRuleData
{
    public static readonly MovementRuleData Default = new MovementRuleData(
        canMoveDiagonally: false,
        mustMoveDiagonally: false
    );

    public bool CanMoveDiagonally { get; }
    public bool MustMoveDiagonally { get; }

    public MovementRuleData(
        bool canMoveDiagonally,
        bool mustMoveDiagonally
    )
    {
        CanMoveDiagonally = canMoveDiagonally;
        MustMoveDiagonally = mustMoveDiagonally;
    }

    public bool CanUseDirection(Vector2Int direction)
    {
        bool isDiagonal = direction.x != 0 && direction.y != 0;

        if (MustMoveDiagonally && !isDiagonal)
        {
            return false;
        }

        if (isDiagonal && !CanMoveDiagonally)
        {
            return false;
        }

        return true;
    }
}
