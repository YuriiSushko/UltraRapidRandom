using UnityEngine;

public class Tile : MonoBehaviour
{
    public int ID { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    [Header("Tile Visuals")]
    public Renderer tileRenderer;

    private Material defaultMaterial_;

    private void Awake()
    {
        ResolveRenderer();
        CaptureDefaultMaterial();
    }

    public void InitializePosition(int id, Vector2Int gridPosition)
    {
        ID = id;
        GridPosition = gridPosition;
        CaptureDefaultMaterial();
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

        tileRenderer.sharedMaterial = material;
    }

    public Material GetMaterial()
    {
        ResolveRenderer();

        return tileRenderer != null
            ? tileRenderer.sharedMaterial
            : null;
    }

    public void ResetMaterial()
    {
        if (defaultMaterial_ == null)
        {
            return;
        }

        SetMaterial(defaultMaterial_);
    }

    private void CaptureDefaultMaterial()
    {
        ResolveRenderer();

        if (tileRenderer != null && defaultMaterial_ == null)
        {
            defaultMaterial_ = tileRenderer.sharedMaterial;
        }
    }

    private void ResolveRenderer()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }
    }
}
