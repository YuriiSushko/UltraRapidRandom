using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(GoalManager))]
public class Game : MonoBehaviour
{
    [Header("References")]
    public BoardController boardController;
    public PlayerController[] players = new PlayerController[0];

    [SerializeField] private UIManager uiManager_;
    [SerializeField] private GoalManager goalManager_;
    [SerializeField] private PickupController pickupController_;

    [Header("Events")]
    public UnityEvent<int> roundCompleted = new UnityEvent<int>();

    private int currentRound_ = 1;
    private ActiveGoal player1Goal_;
    private ActiveGoal player2Goal_;

    private List<string> player1Rules_ = new List<string>();
    private List<string> player2Rules_ = new List<string>();

    private GameState gameState_;

    private void Awake()
    {
        ResolveReferences();
        gameState_ = new GameState(this, boardController, goalManager_, pickupController_, players);
        gameState_.RoundCompleted += HandleGoalManagerRoundEnd;
    }

    private void Start()
    {
        if (boardController != null && !boardController.HasGenerated)
        {
            boardController.BuildBoard();
        }

        InitializePlayers();

        StartNewRound();
    }

    private void Update()
    {
        if (gameState_ != null)
        {
            gameState_.AdvanceTick(Time.deltaTime);
        }

        if (goalManager_ == null)
        {
            EvaluateGoalCompletion();
        }
    }

    public void StartNewRound()
    {
        string player1GoalDescription;
        string player2GoalDescription;

        if (goalManager_ != null && boardController != null && boardController.HasGenerated)
        {
            goalManager_.RollNewGoals(out player1GoalDescription, out player2GoalDescription);
            goalManager_.InitializeMapObjectives(boardController);

            player1Goal_ = goalManager_.GetGoal(0);
            player2Goal_ = goalManager_.GetGoal(1);
        }
        else
        {
            DetermineSafeAsymmetricGoals(out player1Goal_, out player2Goal_);

            player1Goal_.CurrentCount = 0;
            player2Goal_.CurrentCount = 0;

            player1GoalDescription = player1Goal_.Description;
            player2GoalDescription = player2Goal_.Description;
        }

        ConfigurePlayersForGoals();
        ConfigurePickupsForRound();

        if (uiManager_ != null)
        {
            uiManager_.UpdateRoundUI(currentRound_);
            uiManager_.UpdatePlayer1UI(GetPlayerRuleList(0), GetPlayerGoalText(0));
            uiManager_.UpdatePlayer2UI(GetPlayerRuleList(1), GetPlayerGoalText(1));
        }
    }

    private void DetermineSafeAsymmetricGoals(out ActiveGoal p1Goal, out ActiveGoal p2Goal)
    {
        bool playersAreTouching = (players != null && players.Length >= 2 && players[0].CurrentTile == players[1].CurrentTile);

        p1Goal = GenerateRandomGoal();

        while (playersAreTouching && p1Goal.Type == GoalType.CatchOpponent)
        {
            p1Goal = GenerateRandomGoal();
        }

        p2Goal = GenerateRandomGoal();

        while (true)
        {
            bool standardFail = false;

            if (p1Goal.Type == GoalType.CatchOpponent && p2Goal.Type == GoalType.CatchOpponent)
            {
                standardFail = true;
            }

            if (playersAreTouching && p2Goal.Type == GoalType.CatchOpponent)
            {
                standardFail = true;
            }

            if (!standardFail)
            {
                break;
            }

            p2Goal = GenerateRandomGoal();
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
                return new ActiveGoal(selected, $"Pick up all {target}!", target);
            case GoalType.PaintTiles:
                return new ActiveGoal(selected, $"Paint {target} tiles!", target);
            case GoalType.StepOnSpecialTiles:
                return new ActiveGoal(selected, $"Step on all {target}!", target);
            default:
                return new ActiveGoal(GoalType.CatchOpponent, "Catch your opponent!");
        }
    }

    private List<string> GetPlayerRuleList(int playerIndex)
    {
        PlayerController player = GetPlayer(playerIndex);
        List<string> rules = player != null
            ? SplitRuleSummary(player.GetRuleSummary())
            : new List<string>();

        List<string> extraRules = GetStoredPlayerRules(playerIndex);

        for (int i = 0; i < extraRules.Count; i++)
        {
            if (!rules.Contains(extraRules[i]))
            {
                rules.Add(extraRules[i]);
            }
        }

        return rules;
    }

    private List<string> GetStoredPlayerRules(int playerIndex)
    {
        if (playerIndex == 0)
        {
            return player1Rules_;
        }

        if (playerIndex == 1)
        {
            return player2Rules_;
        }

        return new List<string>();
    }

    private string GetPlayerGoalText(int playerIndex)
    {
        ActiveGoal goal = GetPlayerGoal(playerIndex);

        if (goal == null)
        {
            return string.Empty;
        }

        if (goal.TargetCount <= 0)
        {
            return goal.Description;
        }

        return $"{goal.Description} ({goal.CurrentCount}/{goal.TargetCount})";
    }

    private ActiveGoal GetPlayerGoal(int playerIndex)
    {
        if (playerIndex == 0)
        {
            return player1Goal_;
        }

        if (playerIndex == 1)
        {
            return player2Goal_;
        }

        return null;
    }

    private List<string> SplitRuleSummary(string summary)
    {
        List<string> rules = new List<string>();

        if (string.IsNullOrWhiteSpace(summary) || summary == "None")
        {
            return rules;
        }

        string[] parts = summary.Split(',');

        for (int i = 0; i < parts.Length; i++)
        {
            string rule = parts[i].Trim();

            if (rule.Length > 0)
            {
                rules.Add(rule);
            }
        }

        return rules;
    }

    private PlayerController GetPlayer(int playerIndex)
    {
        if (players == null || playerIndex < 0 || playerIndex >= players.Length)
        {
            return null;
        }

        return players[playerIndex];
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
        roundCompleted.Invoke(winningPlayerNumber);

        if (uiManager_ != null)
        {
            uiManager_.TriggerNewRoundPopup(winningPlayerNumber);
        }

        StartNewRound();
    }

    public void HandleGoalManagerRoundEnd(int winningPlayerNumber)
    {
        TriggerRoundEnd(winningPlayerNumber);
    }

    public void IncrementPlayerGoalCounter(int playerIndex, int amount = 1)
    {
        if (playerIndex == 0) player1Goal_.CurrentCount += amount;
        if (playerIndex == 1) player2Goal_.CurrentCount += amount;
    }

    public void SetPlayerRules(int playerIndex, List<string> newRulesList)
    {
        List<string> rules = newRulesList != null
            ? new List<string>(newRulesList)
            : new List<string>();

        if (playerIndex == 0)
        {
            player1Rules_ = rules;
            if (uiManager_ != null && player1Goal_ != null)
                uiManager_.UpdatePlayer1UI(GetPlayerRuleList(0), GetPlayerGoalText(0));
        }
        else if (playerIndex == 1)
        {
            player2Rules_ = rules;
            if (uiManager_ != null && player2Goal_ != null)
                uiManager_.UpdatePlayer2UI(GetPlayerRuleList(1), GetPlayerGoalText(1));
        }
    }

    public void RefreshPlayerUI(int playerIndex)
    {
        if (uiManager_ == null)
        {
            return;
        }

        if (playerIndex == 0 && player1Goal_ != null)
        {
            uiManager_.UpdatePlayer1UI(GetPlayerRuleList(0), GetPlayerGoalText(0));
        }
        else if (playerIndex == 1 && player2Goal_ != null)
        {
            uiManager_.UpdatePlayer2UI(GetPlayerRuleList(1), GetPlayerGoalText(1));
        }
    }

    private void ConfigurePlayersForGoals()
    {
        player1Rules_.Clear();
        player2Rules_.Clear();

        ConfigurePlayerForGoal(GetPlayer(0), player1Goal_);
        ConfigurePlayerForGoal(GetPlayer(1), player2Goal_);
    }

    private void ConfigurePickupsForRound()
    {
        if (pickupController_ == null || boardController == null || !boardController.HasGenerated)
        {
            return;
        }

        pickupController_.Initialize(boardController);
        pickupController_.StartRound(players, HasCollectableGoal());
    }

    private bool HasCollectableGoal()
    {
        return IsCollectableGoal(player1Goal_) || IsCollectableGoal(player2Goal_);
    }

    private bool IsCollectableGoal(ActiveGoal goal)
    {
        return goal != null && goal.Type == GoalType.PickUpObjects;
    }

    private void ConfigurePlayerForGoal(PlayerController player, ActiveGoal goal)
    {
        if (player == null)
        {
            return;
        }

        player.ResolveReferences();

        ResetPlayerRules(player);
        AssignRandomMovementRule(player);

        if (goal != null && goal.Type == GoalType.PaintTiles && player.PlayerActionResolver != null)
        {
            player.PlayerActionResolver.passiveAbility = PassivePlayerAbility.PaintCurrentTile;
        }
    }

    private void ResetPlayerRules(PlayerController player)
    {
        if (player.MovementRuleResolver != null)
        {
            player.MovementRuleResolver.directionRule = MovementDirectionRule.CannotMoveDiagonally;
            player.MovementRuleResolver.tileRule = MovementTileRule.AnyTile;
        }

        if (player.PlayerActionResolver != null)
        {
            player.PlayerActionResolver.passiveAbility = PassivePlayerAbility.None;
            player.PlayerActionResolver.activeAbility = ActivePlayerAbility.None;
        }
    }

    private void AssignRandomMovementRule(PlayerController player)
    {
        if (player.MovementRuleResolver == null)
        {
            return;
        }

        int movementRuleCount = System.Enum.GetValues(typeof(MovementDirectionRule)).Length;
        int firstNonDefaultRule = 1;
        player.MovementRuleResolver.directionRule = (MovementDirectionRule)Random.Range(
            firstNonDefaultRule,
            movementRuleCount
        );
        player.MovementRuleResolver.tileRule = MovementTileRule.AnyTile;
    }

    private void ResolveReferences()
    {
        if (boardController == null) boardController = GetComponent<BoardController>();
        if (goalManager_ == null) goalManager_ = GetComponent<GoalManager>();
        if (goalManager_ == null) goalManager_ = FindFirstObjectByType<GoalManager>();
        if (goalManager_ == null) goalManager_ = gameObject.AddComponent<GoalManager>();
        if (pickupController_ == null) pickupController_ = GetComponent<PickupController>();
        if (pickupController_ == null) pickupController_ = FindFirstObjectByType<PickupController>();
        if (pickupController_ == null) pickupController_ = gameObject.AddComponent<PickupController>();
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        }

        if (uiManager_ == null)
        {
            uiManager_ = FindFirstObjectByType<UIManager>();
        }
    }

    private void InitializePlayers()
    {
        if (boardController == null || !boardController.HasGenerated || players == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];

            if (player == null)
            {
                continue;
            }

            player.Initialize(boardController);
        }

        gameState_?.SnapPlayersToCurrentTiles();
    }
}
