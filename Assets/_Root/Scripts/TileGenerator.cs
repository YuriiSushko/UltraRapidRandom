using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform plane;
    public GameObject tilePrefab;

    [Header("Tile Settings")]
    [Min(1)] public int numberOfTiles = 25;
    public Vector2 tileSize = new Vector2(1f, 1f);
    public Vector2 spacing = new Vector2(0.1f, 0.1f);
    public float yOffset = 0.02f;

    [Header("Plane Fill Settings")]
    public bool fillFullPlane = false;

    [Tooltip("If true, tiles will be slightly resized so they fill the plane exactly.")]
    public bool scaleTilesToFitExactly = false;

    [Header("Generation")]
    public bool generateOnStart = true;
    public bool clearBeforeGenerate = true;

    private int columns;
    private int rows;
    private int tilesToGenerate;

    private Vector3[,] tilePositions;
    private bool[,] tileExists;

    public bool HasGenerated { get; private set; }

    public int Columns => columns;
    public int Rows => rows;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateTiles();
        }
    }

    [ContextMenu("Generate Tiles")]
    public void GenerateTiles()
    {
        if (plane == null)
        {
            Debug.LogError("Plane is not assigned.");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned.");
            return;
        }

        if (clearBeforeGenerate)
        {
            ClearTiles();
        }

        HasGenerated = false;

        Bounds planeBounds = GetPlaneBounds();

        float planeWidth = planeBounds.size.x;
        float planeDepth = planeBounds.size.z;

        Vector2 finalTileSize = tileSize;

        if (fillFullPlane)
        {
            columns = Mathf.Max(1, Mathf.FloorToInt((planeWidth + spacing.x) / (tileSize.x + spacing.x)));
            rows = Mathf.Max(1, Mathf.FloorToInt((planeDepth + spacing.y) / (tileSize.y + spacing.y)));

            tilesToGenerate = columns * rows;

            if (scaleTilesToFitExactly)
            {
                finalTileSize.x = (planeWidth - spacing.x * (columns - 1)) / columns;
                finalTileSize.y = (planeDepth - spacing.y * (rows - 1)) / rows;
            }
        }
        else
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfTiles));
            rows = Mathf.CeilToInt((float)numberOfTiles / columns);

            tilesToGenerate = numberOfTiles;
        }

        tilePositions = new Vector3[columns, rows];
        tileExists = new bool[columns, rows];

        float stepX = finalTileSize.x + spacing.x;
        float stepZ = finalTileSize.y + spacing.y;

        float gridWidth = (columns - 1) * stepX;
        float gridDepth = (rows - 1) * stepZ;

        Vector3 startPosition = planeBounds.center - new Vector3(gridWidth / 2f, 0f, gridDepth / 2f);

        for (int i = 0; i < tilesToGenerate; i++)
        {
            int x = i % columns;
            int z = i / columns;

            Vector3 position = startPosition + new Vector3(
                x * stepX,
                planeBounds.extents.y + yOffset,
                z * stepZ
            );

            tilePositions[x, z] = position;
            tileExists[x, z] = true;

            GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
            tile.name = $"Tile_{x}_{z}";

            if (scaleTilesToFitExactly)
            {
                tile.transform.localScale = new Vector3(
                    finalTileSize.x,
                    tile.transform.localScale.y,
                    finalTileSize.y
                );
            }
        }

        HasGenerated = true;

        Debug.Log($"Generated {tilesToGenerate} tiles. Grid: {columns} x {rows}");
    }

    public bool IsTileValid(Vector2Int tile)
    {
        if (!HasGenerated)
        {
            return false;
        }

        if (tile.x < 0 || tile.x >= columns)
        {
            return false;
        }

        if (tile.y < 0 || tile.y >= rows)
        {
            return false;
        }

        return tileExists[tile.x, tile.y];
    }

    public Vector3 GetTileWorldPosition(Vector2Int tile)
    {
        if (!IsTileValid(tile))
        {
            Debug.LogError($"Invalid tile position: {tile}");
            return Vector3.zero;
        }

        return tilePositions[tile.x, tile.y];
    }

    public Vector2Int ClampToExistingTile(Vector2Int tile)
    {
        if (!HasGenerated)
        {
            return Vector2Int.zero;
        }

        tile.x = Mathf.Clamp(tile.x, 0, columns - 1);
        tile.y = Mathf.Clamp(tile.y, 0, rows - 1);

        if (IsTileValid(tile))
        {
            return tile;
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
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

    private Bounds GetPlaneBounds()
    {
        Renderer renderer = plane.GetComponent<Renderer>();

        if (renderer != null)
        {
            return renderer.bounds;
        }

        return new Bounds(plane.position, new Vector3(10f, 0f, 10f));
    }

    [ContextMenu("Clear Tiles")]
    public void ClearTiles()
    {
        HasGenerated = false;

        tilePositions = null;
        tileExists = null;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}