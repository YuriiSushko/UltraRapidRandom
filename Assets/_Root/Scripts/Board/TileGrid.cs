using UnityEngine;

public class TileGrid
{
    public int Columns { get; }
    public int Rows { get; }
    public int TilesToGenerate { get; }

    public BoardTile[,] Tiles { get; }

    public TileGrid(
        int columns,
        int rows,
        int tilesToGenerate,
        BoardTile[,] tiles
    )
    {
        Columns = columns;
        Rows = rows;
        TilesToGenerate = tilesToGenerate;
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

    public BoardTile GetBoardTile(Vector2Int tile)
    {
        if (!IsTileValid(tile))
        {
            return null;
        }

        return Tiles[tile.x, tile.y];
    }

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        BoardTile boardTile = GetBoardTile(tile);

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