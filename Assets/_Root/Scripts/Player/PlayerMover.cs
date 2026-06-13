using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [Header("Board")]
    public BoardController board;
    public Vector2Int startTile = Vector2Int.zero;

    [Header("Movement")]
    public float yOffset = 0.5f;
    public float moveCooldown = 0.5f;
    public float moveSpeed = 8f;

    [Tooltip("Small delay that allows diagonal input from two separate key presses.")]
    public float diagonalInputBuffer = 0.075f;

    [Header("Controls")]
    public Key upKey = Key.W;
    public Key downKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    private Vector2Int currentTile;
    private Vector3 targetPosition;
    private float nextMoveTime;

    private bool hasPendingKey;
    private Key pendingKey;
    private float pendingKeyTime;

    private IEnumerator Start()
    {
        if (board == null)
        {
            Debug.LogError($"{name}: BoardController is not assigned.");
            yield break;
        }

        while (!board.HasGenerated)
        {
            yield return null;
        }

        currentTile = board.ClampToExistingTile(startTile);
        targetPosition = GetWorldPosition(currentTile);
        transform.position = targetPosition;
    }

    private void Update()
    {
        if (board == null || !board.HasGenerated)
        {
            return;
        }

        HandleMovement();
        MoveSmoothly();
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Time.time < nextMoveTime)
        {
            return;
        }

        if (!hasPendingKey)
        {
            Key pressed = GetFirstPressedKey();

            if (pressed != Key.None)
            {
                pendingKey = pressed;
                pendingKeyTime = Time.time;
                hasPendingKey = true;
            }

            return;
        }

        if (Time.time - pendingKeyTime < diagonalInputBuffer)
        {
            return;
        }

        Vector2Int direction = GetDirectionFromPending();
        hasPendingKey = false;

        if (direction == Vector2Int.zero)
        {
            return;
        }

        Vector2Int targetTile = currentTile + direction;

        if (!board.IsTileValid(targetTile))
        {
            return;
        }

        currentTile = targetTile;
        targetPosition = GetWorldPosition(currentTile);
        nextMoveTime = Time.time + moveCooldown;
    }

    private void MoveSmoothly()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private Vector3 GetWorldPosition(Vector2Int tile)
    {
        Vector3 position = board.GetTileWorldPosition(tile);

        return new Vector3(
            position.x,
            position.y + yOffset,
            position.z
        );
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

        if (pendingKey == upKey)
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
        else if (pendingKey == downKey)
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
        else if (pendingKey == leftKey)
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
        else if (pendingKey == rightKey)
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