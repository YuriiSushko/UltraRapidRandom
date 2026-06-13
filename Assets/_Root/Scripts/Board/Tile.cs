using Assets._Root.Scripts;
using UnityEngine;

public class Tile : MonoBehaviour
{
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

    public void InitializePosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
}