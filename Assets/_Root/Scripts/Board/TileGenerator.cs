using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform plane;
    public GameObject tilePrefab;

    [Header("Tile Settings")]
    [Min(1)]
    public int numberOfTiles = 25;
    public Vector2 spacing = new Vector2(0.1f, 0.1f);
    public float yOffset = 0.02f;

    [Header("Plane Fill Settings")]
    public bool fillFullPlane = false;

    [Header("Generation")]
    public bool clearBeforeGenerate = true;

    public GridRepository LastGeneratedGrid { get; private set; }

    [ContextMenu("Generate Tiles Only")]
    public GridRepository GenerateTiles()
    {
        if (plane == null)
        {
            Debug.LogError("Plane is not assigned.");
            return null;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned.");
            return null;
        }

        Vector2 prefabTileSize = GetPrefabTileSize();

        if (prefabTileSize.x <= 0f || prefabTileSize.y <= 0f)
        {
            Debug.LogError("Could not read valid tile size from prefab.");
            return null;
        }

        if (clearBeforeGenerate)
        {
            ClearTiles();
        }

        Bounds planeBounds = GetPlaneBounds();

        float planeWidth = planeBounds.size.x;
        float planeDepth = planeBounds.size.z;

        int columns;
        int rows;
        int tilesToGenerate;

        if (fillFullPlane)
        {
            columns = Mathf.Max(
                1,
                Mathf.FloorToInt((planeWidth + spacing.x) / (prefabTileSize.x + spacing.x))
            );

            rows = Mathf.Max(
                1,
                Mathf.FloorToInt((planeDepth + spacing.y) / (prefabTileSize.y + spacing.y))
            );

            tilesToGenerate = columns * rows;
        }
        else
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfTiles));
            rows = Mathf.CeilToInt((float)numberOfTiles / columns);

            tilesToGenerate = numberOfTiles;
        }

        Tile[,] tiles = new Tile[columns, rows];

        float stepX = prefabTileSize.x + spacing.x;
        float stepZ = prefabTileSize.y + spacing.y;

        float gridWidth = (columns - 1) * stepX;
        float gridDepth = (rows - 1) * stepZ;

        Vector3 startPosition = planeBounds.center - new Vector3(
            gridWidth / 2f,
            0f,
            gridDepth / 2f
        );

        for (int i = 0; i < tilesToGenerate; i++)
        {
            int x = i % columns;
            int z = i / columns;

            Vector3 offset = new Vector3(
                x * stepX,
                planeBounds.extents.y + yOffset,
                z * stepZ
            );
            Vector3 position = startPosition + offset;

            GameObject tileObject = Instantiate(
                tilePrefab,
                position,
                Quaternion.identity,
                transform
            );

            tileObject.name = $"Tile_{x}_{z}";

            Tile boardTile = tileObject.GetComponent<Tile>();

            if (boardTile == null)
            {
                Debug.LogWarning($"{tileObject.name} has no BoardTile component.");
                continue;
            }

            Vector2Int gridPosition = new Vector2Int(x, z);
            boardTile.InitializePosition(gridPosition);

            tiles[x, z] = boardTile;
        }

        LastGeneratedGrid = new GridRepository(
            columns,
            rows,
            tiles
        );

        Debug.Log($"Generated {tilesToGenerate} tiles. Grid: {columns} x {rows}");

        return LastGeneratedGrid;
    }

    private Vector2 GetPrefabTileSize()
    {
        Renderer renderer = tilePrefab.GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            Debug.LogError("Tile prefab has no Renderer.");
            return Vector2.zero;
        }

        Vector3 size = renderer.bounds.size;

        return new Vector2(size.x, size.z);
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
        LastGeneratedGrid = null;

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