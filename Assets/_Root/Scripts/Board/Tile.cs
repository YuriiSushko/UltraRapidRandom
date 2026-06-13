using UnityEngine;

public class Tile : MonoBehaviour
{
    public int ID { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    [Header("Tile Visuals")]
    public Renderer tileRenderer;

    private void Awake()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }
    }

    public void InitializePosition(int id, Vector2Int gridPosition)
    {
        ID = id;
        GridPosition = gridPosition;
    }

    public void SetMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }

        if (tileRenderer == null)
        {
            return;
        }

        tileRenderer.material = material;
    }
}