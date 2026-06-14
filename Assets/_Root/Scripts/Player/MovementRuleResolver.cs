using UnityEngine;

public enum MovementDirectionRule
{
    CannotMoveDiagonally,
    CanMoveDiagonally,
    OnlyMoveDiagonally
}

public class MovementRuleResolver : MonoBehaviour
{
    [Header("Passive Movement Rules")]
    public MovementDirectionRule directionRule = MovementDirectionRule.CannotMoveDiagonally;

    public MovementRuleData ResolveRules(Vector2Int currentTile, Vector2Int direction)
    {
        bool canUseDiagonal = directionRule != MovementDirectionRule.CannotMoveDiagonally;
        bool mustUseDiagonal = directionRule == MovementDirectionRule.OnlyMoveDiagonally;

        return new MovementRuleData(
            canUseDiagonal,
            mustUseDiagonal
        );
    }

    public string GetRuleSummary()
    {
        return GetDirectionRuleText();
    }

    private string GetDirectionRuleText()
    {
        if (directionRule == MovementDirectionRule.CanMoveDiagonally)
        {
            return "Move all directions";
        }

        if (directionRule == MovementDirectionRule.OnlyMoveDiagonally)
        {
            return "Diagonal move only";
        }

        return "No diagonal";
    }
}
