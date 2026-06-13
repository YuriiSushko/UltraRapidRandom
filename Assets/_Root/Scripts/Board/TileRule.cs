using Assets._Root.Scripts;
using UnityEngine;

// Later support for object on tiles. Maybe as a separate class, idk

[System.Serializable]
public class TileRule
{
    public string displayName;

    [TextArea]
    public string description;

    public RuleCategory category;
    public RuleSubtype subtype;
    public RuleActivationType activationType;

    // Later should be factory or ScriptableObject-based
    // 
    public static TileRule Empty()
    {
        return new TileRule
        {
            displayName = "Empty Tile",
            description = "No special rule.",
            category = RuleCategory.Empty,
            subtype = RuleSubtype.None,
            activationType = RuleActivationType.Passive
        };
    }
}