using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public BoardController board;
    public PlayerMover mover;
    public MovementRuleResolver ruleResolver;

    [Header("Starting Tile")]
    public Vector2Int startTile = Vector2Int.zero;

    [Header("Input")]
    [Tooltip("Small delay that allows diagonal input from two separate key presses.")]
    public float diagonalInputBuffer = 0.075f;
    public float moveCooldown = 0.5f;

    [Header("Controls")]
    public Key upKey = Key.W;
    public Key downKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    private readonly MovementValidator movementValidator_ = new MovementValidator();

    private Vector2Int currentTile_;
    private float nextMoveTime_;

    private bool hasPendingKey_;
    private Key pendingKey_;
    private float pendingKeyTime_;

    private IEnumerator Start()
    {
        if (mover == null)
        {
            mover = GetComponent<PlayerMover>();
        }

        if (board == null)
        {
            Debug.LogError($"{name}: BoardController is not assigned.");
            yield break;
        }

        if (mover == null)
        {
            Debug.LogError($"{name}: PlayerMover is not assigned.");
            yield break;
        }

        while (!board.HasGenerated)
        {
            yield return null;
        }

        currentTile_ = board.ClampToExistingTile(startTile);
        mover.WarpTo(board.GetTileWorldPosition(currentTile_));
    }

    private void Update()
    {
        if (board == null || mover == null || !board.HasGenerated)
        {
            return;
        }

        if (mover.IsMoving || Time.time < nextMoveTime_)
        {
            return;
        }

        Vector2Int direction = GatherMovementDirection();

        if (direction == Vector2Int.zero)
        {
            return;
        }

        MovementRuleData ruleData = ruleResolver != null
            ? ruleResolver.ResolveRules(currentTile_, direction)
            : MovementRuleData.Default;

        MovementValidationResult result = movementValidator_.Validate(
            board,
            currentTile_,
            direction,
            ruleData
        );

        if (!result.IsValid)
        {
            return;
        }

        currentTile_ = result.TargetTile;
        mover.MoveTo(result.TargetWorldPosition);
        nextMoveTime_ = Time.time + moveCooldown;
    }

    private Vector2Int GatherMovementDirection()
    {
        if (Keyboard.current == null)
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
