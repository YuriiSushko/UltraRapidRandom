using UnityEngine;

public class GridRepository
{
    public int Columns { get; }
    public int Rows { get; }
    public Tile[,] Tiles { get; }

    public GridRepository(
        int columns,
        int rows,
        Tile[,] tiles
    )
    {
        Columns = columns;
        Rows = rows;
        Tiles = tiles;
    }

    public bool IsTileValid(Vector2Int tile)
    {
        if (tile.x < 0 || tile.x >= Columns)
        {
            return false;
        }

        if (tile.y < 0 || tile.y >= Rows)
        {
            return false;
        }

        return Tiles[tile.x, tile.y] != null;
    }

    public Tile GetBoardTile(Vector2Int tile)
    {
        if (!IsTileValid(tile))
        {
            return null;
        }

        return Tiles[tile.x, tile.y];
    }

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        Tile boardTile = GetBoardTile(tile);

        if (boardTile == null)
        {
            Debug.LogError($"Invalid tile position: {tile}");
            return Vector3.zero;
        }

        return boardTile.transform.position;
    }

    public Vector2Int ClampToExistingTile(Vector2Int tile)
    {
        tile.x = Mathf.Clamp(tile.x, 0, Columns - 1);
        tile.y = Mathf.Clamp(tile.y, 0, Rows - 1);

        if (IsTileValid(tile))
        {
            return tile;
        }

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                Vector2Int fallbackTile = new Vector2Int(x, y);

                if (IsTileValid(fallbackTile))
                {
                    return fallbackTile;
                }
            }
        }

        return Vector2Int.zero;
    }
}