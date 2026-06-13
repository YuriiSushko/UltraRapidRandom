using UnityEngine;

public struct BoardMutation
{
    public int TileID { get; }
    public Material TileMaterial { get; }

    public BoardMutation(int tileID, Material tileMaterial)
    {
        TileID = tileID;
        TileMaterial = tileMaterial;
    }
}
