using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("References")]
    public BoardController boardController;
    public PlayerController[] players = new PlayerController[0];
    public MovementRuleResolver movementRuleResolver;
    public PlayerActionResolver playerActionResolver;

    [SerializeField] private UIManager uiManager_;

    private GameState gameState_;

    private void Awake()
    {
        ResolveReferences();

        gameState_ = new GameState(
            boardController,
            players,
            movementRuleResolver,
            playerActionResolver
        );
    }

    private void Start()
    {
        if (uiManager_ != null)
        {
            uiManager_.UpdatePlayer1UI("P1 rule", "P1 goal");
            uiManager_.UpdatePlayer2UI("P2 rule", "P2 goal");
        }
        else
        {
            Debug.LogError($"UIManager reference is missing on {gameObject.name}! Drag your UI Canvas or Manager object into the Inspector slot.");
        }
    }

    private void Update()
    {
        gameState_.AdvanceTick(Time.deltaTime);
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController = GetComponent<BoardController>();
        }

        if (players == null || players.Length == 0)
        {
            players = FindObjectsOfType<PlayerController>();
        }

        if (movementRuleResolver == null)
        {
            movementRuleResolver = FindFirstObjectByType<MovementRuleResolver>();
        }

        if (playerActionResolver == null)
        {
            playerActionResolver = FindFirstObjectByType<PlayerActionResolver>();
        }
    }
}
