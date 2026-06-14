public class PickupCollectionResult
{
    public PickupKind Kind { get; }
    public int TileID { get; }
    public bool IsCollectableOnly => Kind == PickupKind.CollectableOnly;

    public PickupCollectionResult(PickupKind kind, int tileID)
    {
        Kind = kind;
        TileID = tileID;
    }
}
