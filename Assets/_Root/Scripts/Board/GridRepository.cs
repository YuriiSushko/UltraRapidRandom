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

    public bool IsTileValid(int tileID)
    {
        if (tileID < 0 || Columns <= 0)
        {
            return false;
        }

        Vector2Int tile = new Vector2Int(
            tileID % Columns,
            tileID / Columns
        );

        return IsTileValid(tile);
    }

    public Tile GetBoardTile(Vector2Int tile)
    {
        if (!IsTileValid(tile))
        {
            return null;
        }

        return Tiles[tile.x, tile.y];
    }

    public Tile GetBoardTile(int tileID)
    {
        return GetBoardTile(GetTilePosition(tileID));
    }

    public int GetTileID(Vector2Int tile)
    {
        if (!IsTileValid(tile))
        {
            return -1;
        }

        return tile.x + tile.y * Columns;
    }

    public Vector2Int GetTilePosition(int tileID)
    {
        if (tileID < 0 || Columns <= 0)
        {
            return new Vector2Int(-1, -1);
        }

        Vector2Int tile = new Vector2Int(
            tileID % Columns,
            tileID / Columns
        );

        if (!IsTileValid(tileID))
        {
            return new Vector2Int(-1, -1);
        }

        return tile;
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

    public Vector3 GetTileWorldPosition(int tileID)
    {
        return GetTileWorldPosition(GetTilePosition(tileID));
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