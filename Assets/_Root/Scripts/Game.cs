using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("References")]
    public BoardController boardController;
    public PlayerController playerController;
    public PlayerMover playerMover;
    public MovementRuleResolver movementRuleResolver;
    public PlayerActionResolver playerActionResolver;

    private GameState gameState_;

    private void Awake()
    {
        gameState_ = new GameState(
            boardController,
            playerController,
            playerMover,
            movementRuleResolver,
            playerActionResolver
        );
    }

    private void Update()
    {
        gameState_.AdvanceTick(Time.deltaTime);
    }
}
