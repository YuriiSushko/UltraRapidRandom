public class PickupCollectionResult
{
    public PickupKind Kind { get; }
    public int TileID { get; }

    public PickupCollectionResult(PickupKind kind, int tileID)
    {
        Kind = kind;
        TileID = tileID;
    }
}
