using UnityEngine;

public class BoardTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public TileRule Rule { get; private set; }

    [Header("Tile Visuals")]
    public Renderer tileRenderer;

    [Header("Icon Visuals")]
    public SpriteRenderer iconRenderer;

    public Sprite moveIcon;
    public Sprite environmentIcon;
    public Sprite chaoticIcon;
    public Sprite goalIcon;
    public Sprite emptyIcon;

    public bool IsRuleIconHidden => isRuleIconHidden;

    private bool isRuleIconHidden = true;

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

    public void SetRule(TileRule rule)
    {
        Rule = rule ?? TileRule.Empty();

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (Rule == null || iconRenderer == null)
        {
            return;
        }

        Sprite iconToUse = Rule.category switch
        {
            TileCategory.Move => moveIcon,
            TileCategory.Environment => environmentIcon,
            TileCategory.Chaotic => chaoticIcon,
            TileCategory.Goal => goalIcon,
            _ => emptyIcon
        };

        iconRenderer.sprite = iconToUse;
        iconRenderer.gameObject.SetActive(iconToUse != null && !isRuleIconHidden);
    }

    public void RevealRuleIcon()
    {
        isRuleIconHidden = false;
        ApplyVisuals();
    }

    public void HideRuleIcon()
    {
        isRuleIconHidden = true;
        ApplyVisuals();
    }
}