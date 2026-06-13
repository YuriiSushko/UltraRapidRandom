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

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        if (grid_ == null)
        {
            Debug.LogError("Board has not been generated yet.");
            return Vector3.zero;
        }

        return grid_.GetTileWorldPosition(tile);
    }

    public Tile GetBoardTile(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return null;
        }

        return grid_.GetBoardTile(tile);
    }

    public Vector2Int ClampToExistingTile(Vector2Int tile)
    {
        if (grid_ == null)
        {
            return Vector2Int.zero;
        }

        return grid_.ClampToExistingTile(tile);
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