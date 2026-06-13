using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [Header("Board")]
    public TileGenerator tileGenerator;
    public Vector2Int startTile = Vector2Int.zero;

    [Header("Movement")]
    public float yOffset = 0.5f;
    public float moveCooldown = 0.5f;
    public float moveSpeed = 8f;

    [Tooltip("Diagonal input wait")]
    public float diagonalInputBuffer = 0.075f;

    [Header("Controls")]
    public Key upKey = Key.W;
    public Key downKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    private Vector2Int currentTile;
    private Vector3 targetPosition;
    private float nextMoveTime;

    private bool hasPendingKey; // f0r diagonal
    private Key pendingKey;
    private float pendingKeyTime;

    private IEnumerator Start()
    {
        if (tileGenerator == null)
        {
            Debug.LogError($"{name}: TileGenerator is not assigned.");
            yield break;
        }

        while (!tileGenerator.HasGenerated)
            yield return null;

        currentTile = tileGenerator.ClampToExistingTile(startTile);
        targetPosition = GetWorldPosition(currentTile);
        transform.position = targetPosition;
    }

    private void Update()
    {
        if (tileGenerator == null || !tileGenerator.HasGenerated)
            return;

        HandleMovement();
        MoveSmoothly();
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
            return;

        if (Time.time < nextMoveTime)
            return;

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
            return;

        Vector2Int direction = GetDirectionFromPending();
        hasPendingKey = false;

        if (direction == Vector2Int.zero)
            return;

        Vector2Int targetTile = currentTile + direction;

        if (!tileGenerator.IsTileValid(targetTile))
            return;

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
        Vector3 pos = tileGenerator.GetTileWorldPosition(tile);

        return new Vector3(
            pos.x,
            pos.y + yOffset,
            pos.z
        );
    }

    private Key GetFirstPressedKey()
    {
        if (Keyboard.current[upKey].wasPressedThisFrame)
            return upKey;

        if (Keyboard.current[downKey].wasPressedThisFrame)
            return downKey;

        if (Keyboard.current[leftKey].wasPressedThisFrame)
            return leftKey;

        if (Keyboard.current[rightKey].wasPressedThisFrame)
            return rightKey;

        return Key.None;
    }

    private Vector2Int GetDirectionFromPending()
    {
        int x = 0;
        int y = 0;

        if (pendingKey == upKey)
        {
            y = 1;
            if (Keyboard.current[rightKey].isPressed) x = 1;
            else if (Keyboard.current[leftKey].isPressed) x = -1;
        }
        else if (pendingKey == downKey)
        {
            y = -1;
            if (Keyboard.current[rightKey].isPressed) x = 1;
            else if (Keyboard.current[leftKey].isPressed) x = -1;
        }
        else if (pendingKey == leftKey)
        {
            x = -1;
            if (Keyboard.current[upKey].isPressed) y = 1;
            else if (Keyboard.current[downKey].isPressed) y = -1;
        }
        else if (pendingKey == rightKey)
        {
            x = 1;
            if (Keyboard.current[upKey].isPressed) y = 1;
            else if (Keyboard.current[downKey].isPressed) y = -1;
        }

        return new Vector2Int(x, y);
    }
}