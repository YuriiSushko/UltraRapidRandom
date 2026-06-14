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
        bool canMoveDiagonally = directionRule != MovementDirectionRule.CannotMoveDiagonally;
        bool mustMoveDiagonally = directionRule == MovementDirectionRule.OnlyMoveDiagonally;
        bool limitMovementToWalkableTiles = tileRule == MovementTileRule.CanOnlyWalkOnTiles;
        int[] walkableTileIDs = limitMovementToWalkableTiles
            ? affectedTileIDs
            : null;
        int[] blockedTileIDs = tileRule == MovementTileRule.CannotWalkOnTiles
            ? affectedTileIDs
            : null;
        Material[] walkableTileMaterials = limitMovementToWalkableTiles
            ? affectedTileMaterials
            : null;
        Material[] blockedTileMaterials = tileRule == MovementTileRule.CannotWalkOnTiles
            ? affectedTileMaterials
            : null;

        return new MovementRuleData(
            canMoveDiagonally,
            mustMoveDiagonally,
            limitMovementToWalkableTiles,
            walkableTileIDs,
            blockedTileIDs,
            walkableTileMaterials,
            blockedTileMaterials
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
            return $"Avoid {GetAffectedTilesText()}";
        }

        if (tileRule == MovementTileRule.CanOnlyWalkOnTiles)
        {
            return $"Only {GetAffectedTilesText()}";
        }

        return string.Empty;
    }

    private string GetAffectedTilesText()
    {
        string tileIDs = GetTileIDsText();
        string materials = GetMaterialsText();

        if (tileIDs.Length > 0 && materials.Length > 0)
        {
            return $"{tileIDs}; {materials}";
        }

        if (tileIDs.Length > 0)
        {
            return tileIDs;
        }

        if (materials.Length > 0)
        {
            return materials;
        }

        return "selected";
    }

    private string GetTileIDsText()
    {
        if (affectedTileIDs == null || affectedTileIDs.Length == 0)
        {
            return string.Empty;
        }

        string text = "#";

        for (int i = 0; i < affectedTileIDs.Length; i++)
        {
            if (i > 0)
            {
                text += "/";
            }

            text += affectedTileIDs[i].ToString();
        }

        return text;
    }

    private string GetMaterialsText()
    {
        if (affectedTileMaterials == null || affectedTileMaterials.Length == 0)
        {
            return string.Empty;
        }

        string text = string.Empty;
        bool hasAnyMaterial = false;

        for (int i = 0; i < affectedTileMaterials.Length; i++)
        {
            Material material = affectedTileMaterials[i];

            if (material == null)
            {
                continue;
            }

            if (hasAnyMaterial)
            {
                text += "/";
            }

            text += material.name;
            hasAnyMaterial = true;
        }

        return hasAnyMaterial
            ? text
            : string.Empty;
    }
}
