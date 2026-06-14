using UnityEngine;

public enum MovementDirectionRule
{
    CannotMoveDiagonally,
    CanMoveDiagonally,
    OnlyMoveDiagonally
}

public enum MovementTileRule
{
    AnyTile,
    CannotWalkOnTiles,
    CanOnlyWalkOnTiles
}

public class MovementRuleResolver : MonoBehaviour
{
    [Header("Passive Movement Rules")]
    public MovementDirectionRule directionRule = MovementDirectionRule.CannotMoveDiagonally;
    public MovementTileRule tileRule = MovementTileRule.AnyTile;

    [Tooltip("Used by Cannot Walk On Tiles and Can Only Walk On Tiles.")]
    public int[] affectedTileIDs = new int[0];
    public Material[] affectedTileMaterials = new Material[0];

    public MovementRuleData ResolveRules(Vector2Int currentTile, Vector2Int direction)
    {
        bool canUseDiagonal = directionRule != MovementDirectionRule.CannotMoveDiagonally;
        bool mustUseDiagonal = directionRule == MovementDirectionRule.OnlyMoveDiagonally;
        bool limitToWalkable = tileRule == MovementTileRule.CanOnlyWalkOnTiles;

        int[] resolvedWalkableTileIDs = tileRule == MovementTileRule.CanOnlyWalkOnTiles
            ? affectedTileIDs
            : null;
        Material[] resolvedWalkableTileMaterials = tileRule == MovementTileRule.CanOnlyWalkOnTiles
            ? affectedTileMaterials
            : null;
        int[] resolvedBlockedTileIDs = tileRule == MovementTileRule.CannotWalkOnTiles
            ? affectedTileIDs
            : null;
        Material[] resolvedBlockedTileMaterials = tileRule == MovementTileRule.CannotWalkOnTiles
            ? affectedTileMaterials
            : null;

        return new MovementRuleData(
            canUseDiagonal,
            mustUseDiagonal,
            limitToWalkable,
            resolvedWalkableTileIDs,
            resolvedBlockedTileIDs,
            resolvedWalkableTileMaterials,
            resolvedBlockedTileMaterials
        );
    }

    public string GetRuleSummary()
    {
        string directionText = GetDirectionRuleText();
        string tileText = GetTileRuleText();

        if (directionText.Length > 0 && tileText.Length > 0)
        {
            return $"{directionText}, {tileText}";
        }

        if (directionText.Length > 0)
        {
            return directionText;
        }

        return tileText;
    }

    private string GetDirectionRuleText()
    {
        if (directionRule == MovementDirectionRule.CanMoveDiagonally)
        {
            return "Diagonal ok";
        }

        if (directionRule == MovementDirectionRule.OnlyMoveDiagonally)
        {
            return "Diagonal only";
        }

        return "No diagonal";
    }

    private string GetTileRuleText()
    {
        if (tileRule == MovementTileRule.CannotWalkOnTiles)
        {
            return "Avoid marked tiles";
        }

        if (tileRule == MovementTileRule.CanOnlyWalkOnTiles)
        {
            return "Only marked tiles";
        }

        return string.Empty;
    }
}
