using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("References")]
    public BoardController boardController;
    public PlayerController[] players = new PlayerController[0];

    [SerializeField] private UIManager uiManager_;

    // Rules & Goals State
    private int currentRound_ = 1;
    private ActiveGoal player1Goal_;
    private ActiveGoal player2Goal_;
    
    private string player1Rule_ = "Standard Movement";
    private string player2Rule_ = "Standard Movement";

    private GameState gameState_;

    private void Awake()
    {
        ResolveReferences();
        gameState_ = new GameState(boardController, players);
    }

    private void Start()
    {
        if (boardController != null && !boardController.HasGenerated)
        {
            boardController.BuildBoard();
        }

        foreach (var player in players)
        {
            if (player != null) player.Initialize(boardController);
        }

        StartNewRound();
    }

    private void Update()
    {
        if (gameState_ != null)
        {
            gameState_.AdvanceTick(Time.deltaTime);
        }

        EvaluateGoalCompletion();
    }

    public void StartNewRound()
    {
        player1Goal_ = GenerateRandomGoal();
        player2Goal_ = GenerateRandomGoal();

        if (uiManager_ != null)
        {
            uiManager_.UpdateRoundUI(currentRound_);
            uiManager_.UpdatePlayer1UI(player1Rule_, player1Goal_.Description);
            uiManager_.UpdatePlayer2UI(player2Rule_, player2Goal_.Description);
        }
    }

    private ActiveGoal GenerateRandomGoal()
    {
        GoalType selected = (GoalType)Random.Range(0, 4);
        int target = Random.Range(3, 7);

        switch (selected)
        {
            case GoalType.CatchOpponent:
                return new ActiveGoal(selected, "Catch your opponent!");
            case GoalType.PickUpObjects:
                return new ActiveGoal(selected, $"Pick up {target} objects on the map!", target);
            case GoalType.PaintTiles:
                return new ActiveGoal(selected, $"Paint {target} tiles total!", target);
            case GoalType.StepOnSpecialTiles:
                return new ActiveGoal(selected, $"Step on {target} special tiles!", target);
            default:
                return new ActiveGoal(GoalType.CatchOpponent, "Catch your opponent!");
        }
    }

    private void EvaluateGoalCompletion()
    {
        if (players == null || players.Length < 2) return;

        if (player1Goal_.Type == GoalType.CatchOpponent && players[0].CurrentTile == players[1].CurrentTile)
        {
            TriggerRoundEnd(1);
            return;
        }
        if (player2Goal_.Type == GoalType.CatchOpponent && players[1].CurrentTile == players[0].CurrentTile)
        {
            TriggerRoundEnd(2);
            return;
        }

        if (player1Goal_.CurrentCount >= player1Goal_.TargetCount && player1Goal_.TargetCount > 0)
        {
            TriggerRoundEnd(1);
            return;
        }
        if (player2Goal_.CurrentCount >= player2Goal_.TargetCount && player2Goal_.TargetCount > 0)
        {
            TriggerRoundEnd(2);
            return;
        }
    }

    private void TriggerRoundEnd(int winningPlayerNumber)
    {
        currentRound_++;

        if (uiManager_ != null)
        {
            uiManager_.TriggerNewRoundPopup(winningPlayerNumber);
        }

        StartNewRound();
    }

    public void IncrementPlayerGoalCounter(int playerIndex, int amount = 1)
    {
        if (playerIndex == 0) player1Goal_.CurrentCount += amount;
        if (playerIndex == 1) player2Goal_.CurrentCount += amount;
    }

    private void ResolveReferences()
    {
        if (boardController == null) boardController = GetComponent<BoardController>();
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        }
    }
}