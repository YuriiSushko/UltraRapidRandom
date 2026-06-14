using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Pickup")]
    public PickupKind kind = PickupKind.CollectableOnly;
    public bool consumeOnPickup = true;

    [Header("Movement Rule")]
    public MovementDirectionRule movementDirectionRule = MovementDirectionRule.CanMoveDiagonally;
    public MovementTileRule movementTileRule = MovementTileRule.AnyTile;

    [Header("Abilities")]
    public PassivePlayerAbility passiveAbility = PassivePlayerAbility.None;
    public ActivePlayerAbility activeAbility = ActivePlayerAbility.None;

    public int TileID { get; private set; } = -1;
    public bool IsCollectableOnly => kind == PickupKind.CollectableOnly;

    public void Initialize(int tileID)
    {
        TileID = tileID;
    }

    public void ApplyTo(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        player.ResolveReferences();

        if (kind == PickupKind.MovementRule && player.MovementRuleResolver != null)
        {
            player.MovementRuleResolver.directionRule = movementDirectionRule;
            player.MovementRuleResolver.tileRule = movementTileRule;
        }
        else if (kind == PickupKind.PassiveAbility && player.PlayerActionResolver != null)
        {
            player.PlayerActionResolver.passiveAbility = passiveAbility;
        }
        else if (kind == PickupKind.ActiveAbility && player.PlayerActionResolver != null)
        {
            player.PlayerActionResolver.activeAbility = activeAbility;
        }
    }
}
