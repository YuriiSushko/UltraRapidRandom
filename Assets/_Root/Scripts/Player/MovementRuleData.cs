using System.Collections.Generic;
using UnityEngine;

public class MovementRuleData
{
    public static readonly MovementRuleData Default = new MovementRuleData(
        canMoveDiagonally: false,
        mustMoveDiagonally: false,
        limitMovementToWalkableTiles: false,
        walkableTileIDs: null,
        blockedTileIDs: null,
        walkableTileMaterials: null,
        blockedTileMaterials: null
    );

    private readonly HashSet<int> walkableTileIDs_;
    private readonly HashSet<int> blockedTileIDs_;
    private readonly HashSet<Material> walkableTileMaterials_;
    private readonly HashSet<Material> blockedTileMaterials_;

    public bool CanMoveDiagonally { get; }
    public bool MustMoveDiagonally { get; }
    public bool LimitMovementToWalkableTiles { get; }

    public MovementRuleData(
        bool canMoveDiagonally,
        bool mustMoveDiagonally,
        bool limitMovementToWalkableTiles,
        IEnumerable<int> walkableTileIDs,
        IEnumerable<int> blockedTileIDs,
        IEnumerable<Material> walkableTileMaterials,
        IEnumerable<Material> blockedTileMaterials
    )
    {
        CanMoveDiagonally = canMoveDiagonally;
        MustMoveDiagonally = mustMoveDiagonally;
        LimitMovementToWalkableTiles = limitMovementToWalkableTiles;
        walkableTileIDs_ = walkableTileIDs != null
            ? new HashSet<int>(walkableTileIDs)
            : new HashSet<int>();
        blockedTileIDs_ = blockedTileIDs != null
            ? new HashSet<int>(blockedTileIDs)
            : new HashSet<int>();
        walkableTileMaterials_ = walkableTileMaterials != null
            ? new HashSet<Material>(walkableTileMaterials)
            : new HashSet<Material>();
        blockedTileMaterials_ = blockedTileMaterials != null
            ? new HashSet<Material>(blockedTileMaterials)
            : new HashSet<Material>();
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

    public bool CanWalkOnTile(int tileID)
    {
        return CanWalkOnTile(tileID, null);
    }

    public bool CanWalkOnTile(int tileID, Material tileMaterial)
    {
        if (blockedTileIDs_.Contains(tileID))
        {
            return false;
        }

        if (tileMaterial != null && blockedTileMaterials_.Contains(tileMaterial))
        {
            return false;
        }

        if (!LimitMovementToWalkableTiles)
        {
            return true;
        }

        return walkableTileIDs_.Contains(tileID)
            || (tileMaterial != null && walkableTileMaterials_.Contains(tileMaterial));
    }
}
