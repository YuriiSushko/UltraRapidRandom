using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Movement")]
    public float yOffset = 0.5f;
    public float moveSpeed = 8f;

    public bool IsMoving => Vector3.Distance(transform.position, targetPosition_) > 0.001f;

    private Vector3 targetPosition_;

    private void Awake()
    {
        targetPosition_ = transform.position;
    }

    public void Tick(float deltaTime)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition_,
            moveSpeed * deltaTime
        );
    }

    public void WarpTo(Vector3 worldPosition)
    {
        targetPosition_ = ApplyYOffset(worldPosition);
        transform.position = targetPosition_;
    }

    public void MoveTo(Vector3 worldPosition)
    {
        targetPosition_ = ApplyYOffset(worldPosition);
    }

    private Vector3 ApplyYOffset(Vector3 worldPosition)
    {
        return new Vector3(
            worldPosition.x,
            worldPosition.y + yOffset,
            worldPosition.z
        );
    }
}
