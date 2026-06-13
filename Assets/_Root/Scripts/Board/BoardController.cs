using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Generation")]
    public bool generateOnStart = true;

    [Header("References")]
    public TileGenerator tileGenerator;
    public TileLayoutProvider tileLayoutProvider;

    private TileGrid grid;

    public bool HasGenerated => grid != null;

    public int Columns => grid != null ? grid.Columns : 0;
    public int Rows => grid != null ? grid.Rows : 0;

    private void Awake()
    {
        if (tileGenerator == null)
        {
            tileGenerator = GetComponent<TileGenerator>();
        }
    }

    private void Start()
    {
        if (generateOnStart)
        {
            BuildBoard();
        }
    }

    [ContextMenu("Build Board")]
    public void BuildBoard()
    {
        if (tileGenerator == null)
        {
            Debug.LogError("TileGenerator is not assigned.");
            return;
        }

        grid = tileGenerator.GenerateTiles();

        if (grid == null)
        {
            Debug.LogError("Tile generation failed.");
            return;
        }

        ApplyTileRules();

        Debug.Log("Board built successfully.");
    }

    private void ApplyTileRules()
    {
        TileRule[] rules = GenerateRules();

        for (int i = 0; i < grid.TilesToGenerate; i++)
        {
            int x = i % grid.Columns;
            int y = i / grid.Columns;

            BoardTile boardTile = grid.Tiles[x, y];

            if (boardTile == null)
            {
                continue;
            }

            boardTile.SetRule(rules[i]);
        }
    }

    private TileRule[] GenerateRules()
    {
        if (tileLayoutProvider != null)
        {
            TileRule[] generatedRules = tileLayoutProvider.GenerateTileRules(
                grid.Columns,
                grid.Rows,
                grid.TilesToGenerate
            );

            if (generatedRules != null && generatedRules.Length >= grid.TilesToGenerate)
            {
                return generatedRules;
            }

            Debug.LogWarning("TileLayoutProvider returned invalid rules. Falling back to empty rules.");
        }

        TileRule[] emptyRules = new TileRule[grid.TilesToGenerate];

        for (int i = 0; i < emptyRules.Length; i++)
        {
            emptyRules[i] = TileRule.Empty();
        }

        return emptyRules;
    }

    public bool IsTileValid(Vector2Int tile)
    {
        if (grid == null)
        {
            return false;
        }

        return grid.IsTileValid(tile);
    }

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        if (grid == null)
        {
            Debug.LogError("Board has not been generated yet.");
            return Vector3.zero;
        }

        return grid.GetTileWorldPosition(tile);
    }

    public BoardTile GetBoardTile(Vector2Int tile)
    {
        if (grid == null)
        {
            return null;
        }

        return grid.GetBoardTile(tile);
    }

    public Vector2Int ClampToExistingTile(Vector2Int tile)
    {
        if (grid == null)
        {
            return Vector2Int.zero;
        }

        return grid.ClampToExistingTile(tile);
    }

    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        grid = null;

        if (tileGenerator != null)
        {
            tileGenerator.ClearTiles();
        }
    }
}