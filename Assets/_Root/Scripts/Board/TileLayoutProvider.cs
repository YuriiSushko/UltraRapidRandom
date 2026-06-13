public abstract class TileLayoutProvider : UnityEngine.MonoBehaviour
{
    public abstract TileRule[] GenerateTileRules(int columns, int rows, int tilesToGenerate);
}