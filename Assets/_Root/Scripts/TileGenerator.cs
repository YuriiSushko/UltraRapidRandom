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

        Bounds planeBounds = GetPlaneBounds();

        float planeWidth = planeBounds.size.x;
        float planeDepth = planeBounds.size.z;

        int columns;
        int rows;
        int tilesToGenerate;

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

            GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
            tile.name = $"Tile_{i + 1}";

            if (scaleTilesToFitExactly)
            {
                tile.transform.localScale = new Vector3(
                    finalTileSize.x,
                    tile.transform.localScale.y,
                    finalTileSize.y
                );
            }
        }

        Debug.Log($"Generated {tilesToGenerate} tiles. Grid: {columns} x {rows}");
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