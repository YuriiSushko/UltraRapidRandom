using UnityEngine;

public class BoardController : MonoBehaviour
{
    private TileGenerator tileGenerator_;
    private TileLayoutProvider tileLayoutProvider_;

    private GridRepository grid_;

    public bool HasGenerated => grid_ != null;

    public int Columns => grid_ != null ? grid_.Columns : 0;
    public int Rows => grid_ != null ? grid_.Rows : 0;

    private void Start()
    {
        tileGenerator_ = GetComponent<TileGenerator>();
        BuildBoard();
    }

    [ContextMenu("Build Board")]
    public void BuildBoard()
    {
        if (tileGenerator_ == null)
        {
            Debug.LogError("TileGenerator is not assigned.");
            return;
        }

        grid_ = tileGenerator_.GenerateTiles();

        if (grid_ == null)
        {
            Debug.LogError("Tile generation failed.");
            return;
        }

        Debug.Log("Board built successfully.");
    }

    public bool IsTileValid(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return false;
        }

        return grid_.IsTileValid(tile);
    }

    public bool IsTileValid(int tileID)
    {
        if (grid_ == null)
        {
            return false;
        }

        return grid_.IsTileValid(tileID);
    }

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        if (grid_ == null)
        {
            Debug.LogError("Board has not been generated yet.");
            return Vector3.zero;
        }

        return grid_.GetTileWorldPosition(tile);
    }

    public Vector3 GetTileWorldPosition(int tileID)
    {
        if (grid_ == null)
        {
            Debug.LogError("Board has not been generated yet.");
            return Vector3.zero;
        }

        return grid_.GetTileWorldPosition(tileID);
    }

    public Tile GetBoardTile(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return null;
        }

        return grid_.GetBoardTile(tile);
    }

    public Tile GetBoardTile(int tileID)
    {
        if (grid_ == null)
        {
            return null;
        }

        return grid_.GetBoardTile(tileID);
    }

    public int GetTileID(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return -1;
        }

        return grid_.GetTileID(tile);
    }

    public Vector2Int GetTilePosition(int tileID)
    {
        if (grid_ == null)
        {
            return new Vector2Int(-1, -1);
        }

        return grid_.GetTilePosition(tileID);
    }

    public Vector2Int ClampToExistingTile(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return Vector2Int.zero;
        }

        return grid_.ClampToExistingTile(tile);
    }

    public void ApplyMutation(BoardMutation mutation)
    {
        SetTileMaterial(mutation.TileID, mutation.TileMaterial);
    }

    public void SetTileMaterial(int tileID, Material material)
    {
        Tile tile = GetBoardTile(tileID);

        if (tile == null)
        {
            return;
        }

        tile.SetMaterial(material);
    }

    public Material GetTileMaterial(int tileID)
    {
        Tile tile = GetBoardTile(tileID);

        return tile != null
            ? tile.GetMaterial()
            : null;
    }

    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        grid_ = null;

        if (tileGenerator_ != null)
        {
            tileGenerator_.ClearTiles();
        }
    }
}
