using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pickupPrefab;

    [Header("Spawn Rules")]
    [SerializeField] private int rulePickupCount = 4;
    [SerializeField] private int minDistanceFromPlayers = 2;
    [SerializeField] private float yOffset = 0.45f;
    [SerializeField] private int maxSpawnAttempts = 100;

    private readonly List<PickupObject> activePickups_ = new List<PickupObject>();
    private BoardController board_;

    public void Initialize(BoardController board)
    {
        board_ = board;
    }

    public void StartRound(PlayerController[] players, bool needsCollectablePickup)
    {
        ClearPickups();
        SpawnRulePickups(players);

        if (needsCollectablePickup)
        {
            SpawnCollectablePickup(players);
        }
    }

    public PickupCollectionResult TryCollectAtTile(PlayerController player, int tileID)
    {
        if (player == null || tileID < 0)
        {
            return null;
        }

        for (int i = activePickups_.Count - 1; i >= 0; i--)
        {
            PickupObject pickup = activePickups_[i];

            if (pickup == null)
            {
                activePickups_.RemoveAt(i);
                continue;
            }

            if (pickup.TileID != tileID)
            {
                continue;
            }

            pickup.ApplyTo(player);
            PickupCollectionResult result = new PickupCollectionResult(pickup.kind, pickup.TileID);

            if (pickup.consumeOnPickup)
            {
                activePickups_.RemoveAt(i);
                Destroy(pickup.gameObject);
            }

            return result;
        }

        return null;
    }

    public void EnsureCollectablePickup(PlayerController[] players)
    {
        for (int i = 0; i < activePickups_.Count; i++)
        {
            PickupObject pickup = activePickups_[i];

            if (pickup != null && pickup.IsCollectableOnly)
            {
                return;
            }
        }

        SpawnCollectablePickup(players);
    }

    private void SpawnRulePickups(PlayerController[] players)
    {
        for (int i = 0; i < rulePickupCount; i++)
        {
            PickupObject pickup = SpawnPickup(players, GetRandomRulePickupKind());

            if (pickup == null)
            {
                continue;
            }

            ConfigureRandomRulePickup(pickup);
        }
    }

    private void SpawnCollectablePickup(PlayerController[] players)
    {
        SpawnPickup(players, PickupKind.CollectableOnly);
    }

    private PickupObject SpawnPickup(PlayerController[] players, PickupKind kind)
    {
        if (board_ == null || !board_.HasGenerated)
        {
            return null;
        }

        if (!TryGetRandomSpawnTile(players, out int tileID))
        {
            return null;
        }

        Vector3 worldPosition = board_.GetTileWorldPosition(tileID) + new Vector3(0f, yOffset, 0f);
        GameObject pickupObject = pickupPrefab != null
            ? Instantiate(pickupPrefab, worldPosition, Quaternion.identity, transform)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (pickupPrefab == null)
        {
            pickupObject.transform.SetParent(transform);
            pickupObject.transform.position = worldPosition;
            pickupObject.transform.localScale = Vector3.one * 0.35f;
        }

        pickupObject.name = $"{kind}_Pickup_{tileID}";

        PickupObject pickup = pickupObject.GetComponent<PickupObject>();

        if (pickup == null)
        {
            pickup = pickupObject.AddComponent<PickupObject>();
        }

        pickup.kind = kind;
        pickup.Initialize(tileID);
        ApplyPickupVisual(pickup);
        activePickups_.Add(pickup);

        return pickup;
    }

    private PickupKind GetRandomRulePickupKind()
    {
        int roll = Random.Range(0, 3);

        if (roll == 0)
        {
            return PickupKind.MovementRule;
        }

        if (roll == 1)
        {
            return PickupKind.PassiveAbility;
        }

        return PickupKind.ActiveAbility;
    }

    private void ConfigureRandomRulePickup(PickupObject pickup)
    {
        if (pickup.kind == PickupKind.MovementRule)
        {
            int firstNonDefaultRule = 1;
            int movementRuleCount = System.Enum.GetValues(typeof(MovementDirectionRule)).Length;
            pickup.movementDirectionRule = (MovementDirectionRule)Random.Range(
                firstNonDefaultRule,
                movementRuleCount
            );
            pickup.movementTileRule = MovementTileRule.AnyTile;
        }
        else if (pickup.kind == PickupKind.PassiveAbility)
        {
            pickup.passiveAbility = PassivePlayerAbility.PaintCurrentTile;
        }
        else if (pickup.kind == PickupKind.ActiveAbility)
        {
            int firstActiveAbility = 1;
            int activeAbilityCount = System.Enum.GetValues(typeof(ActivePlayerAbility)).Length;
            pickup.activeAbility = (ActivePlayerAbility)Random.Range(
                firstActiveAbility,
                activeAbilityCount
            );
        }

        ApplyPickupVisual(pickup);
    }

    private void ApplyPickupVisual(PickupObject pickup)
    {
        Renderer pickupRenderer = pickup.GetComponentInChildren<Renderer>();

        if (pickupRenderer == null)
        {
            return;
        }

        if (pickup.kind == PickupKind.CollectableOnly)
        {
            pickupRenderer.material.color = Color.yellow;
        }
        else if (pickup.kind == PickupKind.MovementRule)
        {
            pickupRenderer.material.color = Color.cyan;
        }
        else if (pickup.kind == PickupKind.PassiveAbility)
        {
            pickupRenderer.material.color = Color.green;
        }
        else if (pickup.kind == PickupKind.ActiveAbility)
        {
            pickupRenderer.material.color = Color.magenta;
        }
    }

    private bool TryGetRandomSpawnTile(PlayerController[] players, out int tileID)
    {
        tileID = -1;

        if (board_ == null || board_.Columns <= 0 || board_.Rows <= 0)
        {
            return false;
        }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2Int tile = new Vector2Int(
                Random.Range(0, board_.Columns),
                Random.Range(0, board_.Rows)
            );

            int candidateID = board_.GetTileID(tile);

            if (candidateID < 0)
            {
                continue;
            }

            if (IsPickupOnTile(candidateID) || IsTooCloseToPlayer(tile, players))
            {
                continue;
            }

            tileID = candidateID;
            return true;
        }

        return false;
    }

    private bool IsPickupOnTile(int tileID)
    {
        for (int i = 0; i < activePickups_.Count; i++)
        {
            PickupObject pickup = activePickups_[i];

            if (pickup != null && pickup.TileID == tileID)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTooCloseToPlayer(Vector2Int tile, PlayerController[] players)
    {
        if (players == null)
        {
            return false;
        }

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];

            if (player == null || !player.HasInitialized)
            {
                continue;
            }

            int distance = Mathf.Abs(tile.x - player.CurrentTile.x)
                + Mathf.Abs(tile.y - player.CurrentTile.y);

            if (distance < minDistanceFromPlayers)
            {
                return true;
            }
        }

        return false;
    }

    private void ClearPickups()
    {
        for (int i = activePickups_.Count - 1; i >= 0; i--)
        {
            PickupObject pickup = activePickups_[i];

            if (pickup != null)
            {
                Destroy(pickup.gameObject);
            }
        }

        activePickups_.Clear();
    }
}
