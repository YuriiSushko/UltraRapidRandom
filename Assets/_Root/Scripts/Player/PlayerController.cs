using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour
{
    [Header("Starting Tile")]
    public Vector2Int startTile = Vector2Int.zero;

    [Header("Movement Input")]
    [Tooltip("Small delay that allows diagonal input from two separate key presses.")]
    public float diagonalInputBuffer = 0.075f;
    public float moveCooldown = 0.5f;

    [Header("Movement Controls")]
    public Key upKey = Key.W;
    public Key downKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    [Header("Action Controls")]
    public Key activeActionKey = Key.Space;

    [Header("Resolvers")]
    public MovementRuleResolver movementRuleResolver;
    public PlayerActionResolver playerActionResolver;

    public bool HasInitialized { get; private set; }
    public Vector2Int CurrentTile => currentTile_;
    public MovementRuleResolver MovementRuleResolver => movementRuleResolver;
    public PlayerActionResolver PlayerActionResolver => playerActionResolver;

    private Vector2Int currentTile_;
    private float movementCooldownRemaining_;
    private float hiddenRemaining_;
    private bool isHidden_;
    private Renderer[] renderers_;

    private bool hasPendingKey_;
    private Key pendingKey_;
    private float pendingKeyTime_;

    public void Initialize(BoardController board)
    {
        if (board == null || !board.HasGenerated)
        {
            return;
        }

        ResolveReferences();

        currentTile_ = board.ClampToExistingTile(startTile);
        movementCooldownRemaining_ = 0f;
        hasPendingKey_ = false;
        HasInitialized = true;
    }

    public void ResolveReferences()
    {
        if (movementRuleResolver == null)
        {
            movementRuleResolver = GetComponent<MovementRuleResolver>();
        }

        if (playerActionResolver == null)
        {
            playerActionResolver = GetComponent<PlayerActionResolver>();
        }

        renderers_ = GetComponentsInChildren<Renderer>();
    }

    public string GetRuleSummary()
    {
        ResolveReferences();

        string movementRules = movementRuleResolver != null
            ? movementRuleResolver.GetRuleSummary()
            : string.Empty;
        string actionRules = playerActionResolver != null
            ? playerActionResolver.GetAbilitySummary()
            : string.Empty;

        if (movementRules.Length > 0 && actionRules.Length > 0)
        {
            return $"{movementRules}, {actionRules}";
        }

        if (movementRules.Length > 0)
        {
            return movementRules;
        }

        if (actionRules.Length > 0)
        {
            return actionRules;
        }

        return "None";
    }

    public void TickTimers(float deltaTime)
    {
        TickHidden(deltaTime);

        if (movementCooldownRemaining_ <= 0f)
        {
            movementCooldownRemaining_ = 0f;
            return;
        }

        movementCooldownRemaining_ = Mathf.Max(
            0f,
            movementCooldownRemaining_ - deltaTime
        );
    }

    public bool CanTryMove()
    {
        return HasInitialized && movementCooldownRemaining_ <= 0f;
    }

    public Vector2Int GatherMovementInput()
    {
        if (!CanTryMove() || Keyboard.current == null)
        {
            return Vector2Int.zero;
        }

        if (!hasPendingKey_)
        {
            Key pressed = GetFirstPressedKey();

            if (pressed != Key.None)
            {
                pendingKey_ = pressed;
                pendingKeyTime_ = Time.time;
                hasPendingKey_ = true;
            }

            return Vector2Int.zero;
        }

        if (Time.time - pendingKeyTime_ < diagonalInputBuffer)
        {
            return Vector2Int.zero;
        }

        Vector2Int direction = GetDirectionFromPending();
        hasPendingKey_ = false;

        return direction;
    }

    public PlayerActionInput GatherActionInput()
    {
        if (!HasInitialized)
        {
            return PlayerActionInput.None;
        }

        return new PlayerActionInput(
            hasPassiveAction: true,
            hasActiveAction: WasKeyPressedThisFrame(activeActionKey)
        );
    }

    public void ApplyMovementResult(MovementValidationResult result)
    {
        if (!result.IsValid)
        {
            return;
        }

        currentTile_ = result.TargetTile;
    }

    public void SetCurrentTile(Vector2Int tile)
    {
        currentTile_ = tile;
    }

    public void Hide(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        hiddenRemaining_ = Mathf.Max(hiddenRemaining_, duration);
        SetHidden(true);
    }

    public void ResetMovementCooldown()
    {
        movementCooldownRemaining_ = moveCooldown;
    }

    private void TickHidden(float deltaTime)
    {
        if (hiddenRemaining_ <= 0f)
        {
            return;
        }

        hiddenRemaining_ = Mathf.Max(
            0f,
            hiddenRemaining_ - deltaTime
        );

        if (hiddenRemaining_ <= 0f)
        {
            SetHidden(false);
        }
    }

    private void SetHidden(bool hidden)
    {
        if (isHidden_ == hidden)
        {
            return;
        }

        isHidden_ = hidden;

        if (renderers_ == null)
        {
            renderers_ = GetComponentsInChildren<Renderer>();
        }

        for (int i = 0; i < renderers_.Length; i++)
        {
            if (renderers_[i] != null)
            {
                renderers_[i].enabled = !hidden;
            }
        }
    }

    private Key GetFirstPressedKey()
    {
        if (WasKeyPressedThisFrame(upKey))
        {
            return upKey;
        }

        if (WasKeyPressedThisFrame(downKey))
        {
            return downKey;
        }

        if (WasKeyPressedThisFrame(leftKey))
        {
            return leftKey;
        }

        if (WasKeyPressedThisFrame(rightKey))
        {
            return rightKey;
        }

        return Key.None;
    }

    private Vector2Int GetDirectionFromPending()
    {
        int x = 0;
        int y = 0;

        if (pendingKey_ == upKey)
        {
            y = 1;

            if (IsKeyPressed(rightKey))
            {
                x = 1;
            }
            else if (IsKeyPressed(leftKey))
            {
                x = -1;
            }
        }
        else if (pendingKey_ == downKey)
        {
            y = -1;

            if (IsKeyPressed(rightKey))
            {
                x = 1;
            }
            else if (IsKeyPressed(leftKey))
            {
                x = -1;
            }
        }
        else if (pendingKey_ == leftKey)
        {
            x = -1;

            if (IsKeyPressed(upKey))
            {
                y = 1;
            }
            else if (IsKeyPressed(downKey))
            {
                y = -1;
            }
        }
        else if (pendingKey_ == rightKey)
        {
            x = 1;

            if (IsKeyPressed(upKey))
            {
                y = 1;
            }
            else if (IsKeyPressed(downKey))
            {
                y = -1;
            }
        }

        return new Vector2Int(x, y);
    }

    private bool WasKeyPressedThisFrame(Key key)
    {
        if (!IsKeyValid(key))
        {
            return false;
        }

        return Keyboard.current[key].wasPressedThisFrame;
    }

    private bool IsKeyPressed(Key key)
    {
        if (!IsKeyValid(key))
        {
            return false;
        }

        return Keyboard.current[key].isPressed;
    }

    private bool IsKeyValid(Key key)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        if (key == Key.None)
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(Key), key))
        {
            Debug.LogWarning($"{name}: Invalid key value: {key}. Reassign keys in Inspector.");
            return false;
        }

        return true;
    }
}
