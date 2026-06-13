using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [Header("Board")]
    public TileGenerator tileGenerator;
    public Vector2Int startTile = new Vector2Int(0, 0);

    [Header("Movement")]
    public float yOffset = 0.5f;
    public float moveCooldown = 0.25f;
    public bool allowDiagonalMovement = true;

    [Header("Controls")]
    public Key upKey = Key.W;
    public Key downKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;

    private Vector2Int currentTile;
    private float nextMoveTime;

    private IEnumerator Start()
    {
        if (tileGenerator == null)
        {
            Debug.LogError($"{name}: TileGenerator is not assigned.");
            yield break;
        }

        while (!tileGenerator.HasGenerated)
        {
            yield return null;
        }

        currentTile = tileGenerator.ClampToExistingTile(startTile);
        MoveToTile(currentTile);
    }

    private void Update()
    {
        if (tileGenerator == null || !tileGenerator.HasGenerated)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Time.time < nextMoveTime)
        {
            return;
        }

        Vector2Int direction = ReadDirection();

        if (direction == Vector2Int.zero)
        {
            return;
        }

        if (!allowDiagonalMovement && direction.x != 0 && direction.y != 0)
        {
            return;
        }

        Vector2Int targetTile = currentTile + direction;

        if (!tileGenerator.IsTileValid(targetTile))
        {
            return;
        }

        currentTile = targetTile;
        MoveToTile(currentTile);

        nextMoveTime = Time.time + moveCooldown;
    }

    private Vector2Int ReadDirection()
    {
        int x = 0;
        int y = 0;

        if (IsKeyPressed(rightKey)) x += 1;
        if (IsKeyPressed(leftKey)) x -= 1;
        if (IsKeyPressed(upKey)) y += 1;
        if (IsKeyPressed(downKey)) y -= 1;

        return new Vector2Int(x, y);
    }

    private bool IsKeyPressed(Key key)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(Key), key))
        {
            Debug.LogWarning($"{name}: Invalid key value detected: {key}. Reassign keys in the Inspector.");
            return false;
        }

        return Keyboard.current[key].isPressed;
    }

    private void MoveToTile(Vector2Int tile)
    {
        Vector3 tilePosition = tileGenerator.GetTileWorldPosition(tile);

        transform.position = new Vector3(
            tilePosition.x,
            tilePosition.y + yOffset,
            tilePosition.z
        );
    }
}