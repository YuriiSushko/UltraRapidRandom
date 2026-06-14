using UnityEngine;

public class GameState
{
    private readonly BoardController board_;
    private readonly PlayerController[] players_;
    private readonly MovementValidator movementValidator_ = new MovementValidator();
    private readonly BoardMutator boardMutator_ = new BoardMutator();

    private bool hasLoggedMissingReferences_;

    public GameState(
        BoardController board,
        PlayerController[] players
    )
    {
        board_ = board;
        players_ = players != null
            ? players
            : new PlayerController[0];
    }

    public void AdvanceTick(float deltaTime)
    {
        if (!HasRequiredReferences()) return;

        for (int i = 0; i < players_.Length; i++)
        {
            TickPlayer(players_[i], deltaTime);
        }

        EvaluateRoundStatus();
    }

    private bool HasRequiredReferences()
    {
        if (board_ != null && players_.Length > 0)
        {
            return true;
        }

        if (!hasLoggedMissingReferences_)
        {
            Debug.LogError("GameState is missing BoardController or player controllers.");
            hasLoggedMissingReferences_ = true;
        }

        return false;
    }

    private void TickPlayer(PlayerController playerController, float deltaTime)
    {
        if (playerController == null)
        {
            return;
        }

        PlayerMover playerMover = playerController.GetComponent<PlayerMover>();

        if (playerMover == null)
        {
            Debug.LogError($"{playerController.name}: PlayerMover is missing.");
            return;
        }

        TryInitialize(playerController, playerMover);

        if (!playerController.HasInitialized)
        {
            return;
        }

        playerController.TickTimers(deltaTime);

        TryHandlePlayerMovement(playerController, playerMover);
        TryHandlePlayerActions(playerController);

        playerMover.Tick(deltaTime);
    }

    private void TryInitialize(PlayerController playerController, PlayerMover playerMover)
    {
        if (playerController.HasInitialized || !board_.HasGenerated)
        {
            return;
        }

        playerController.Initialize(board_);
        playerMover.WarpTo(board_.GetTileWorldPosition(playerController.CurrentTile));
    }

    private void TryHandlePlayerMovement(
        PlayerController playerController,
        PlayerMover playerMover
    )
    {
        if (playerMover.IsMoving || !playerController.CanTryMove())
        {
            return;
        }

        Vector2Int direction = playerController.GatherMovementInput();

        if (direction == Vector2Int.zero)
        {
            return;
        }

        MovementRuleData ruleData = playerController.MovementRuleResolver != null
            ? playerController.MovementRuleResolver.ResolveRules(playerController.CurrentTile, direction)
            : MovementRuleData.Default;

        MovementValidationResult result = movementValidator_.Validate(
            board_,
            playerController.CurrentTile,
            direction,
            ruleData
        );

        if (!result.IsValid)
        {
            return;
        }

        playerController.ApplyMovementResult(result);
        playerMover.MoveTo(result.TargetWorldPosition);
        playerController.ResetMovementCooldown();
    }

    private void TryHandlePlayerActions(PlayerController playerController)
    {
        PlayerActionInput actionInput = playerController.GatherActionInput();

        if (!actionInput.HasAnyAction)
        {
            return;
        }

        if (playerController.PlayerActionResolver == null)
        {
            return;
        }

        int currentTileID = board_.GetTileID(playerController.CurrentTile);
        BoardMutationData mutationData = playerController.PlayerActionResolver.ResolveActions(
            actionInput,
            currentTileID
        );

        boardMutator_.Apply(board_, mutationData);
    }
    
    private void EvaluateRoundStatus()
    {
        if (players_.Length < 2) return;

        var goalManager = Object.FindFirstObjectByType<GoalManager>();
        var ui = Object.FindFirstObjectByType<UIManager>();
        var game = Object.FindFirstObjectByType<Game>();
    
        if (goalManager == null || game == null) return;

        int resultP1 = goalManager.ProcessStepEvaluations(
            0, 
            players_[0].CurrentTile, 
            board_.GetTileID(players_[0].CurrentTile), 
            players_[1].CurrentTile
        );

        int resultP2 = goalManager.ProcessStepEvaluations(
            1, 
            players_[1].CurrentTile, 
            board_.GetTileID(players_[1].CurrentTile), 
            players_[0].CurrentTile
        );

        if (resultP1 != 0 || resultP2 != 0)
        {
            int winner = (resultP1 != 0) ? 1 : 2;
        
            if (ui != null) ui.TriggerNewRoundPopup(winner);

            goalManager.RollNewGoals(out string newP1Desc, out string newP2Desc);
            goalManager.InitializeMapObjectives(board_);

            if (ui != null)
            {
                ui.UpdatePlayer1UI("Keep current rule active", newP1Desc);
                ui.UpdatePlayer2UI("Keep current rule active", newP2Desc);
            }
        }
    }
}


