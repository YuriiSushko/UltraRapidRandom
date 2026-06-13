using UnityEngine;

public enum TileCategory
{
    Empty,
    Move,
    Environment,
    Chaotic,
    Goal
}

public enum TileSubtype
{
    None,
    MoveDir,
    MoveSpd,
    MoveDist
}

public enum TileActivationType
{
    Passive,
    Active
}

[System.Serializable]
public class TileRule
{
    public string displayName;

    [TextArea]
    public string description;

    public TileCategory category;
    public TileSubtype subtype;
    public TileActivationType activationType;

    // Later should be factory or ScriptableObject-based
    // 
    public static TileRule Empty()
    {
        return new TileRule
        {
            displayName = "Empty Tile",
            description = "No special rule.",
            category = TileCategory.Empty,
            subtype = TileSubtype.None,
            activationType = TileActivationType.Passive
        };
    }
}