using UnityEngine;

public class GameState
{
    private readonly BoardController board_;
    private readonly PlayerController[] players_;
    private readonly MovementRuleResolver movementRuleResolver_;
    private readonly PlayerActionResolver playerActionResolver_;
    private readonly MovementValidator movementValidator_ = new MovementValidator();
    private readonly BoardMutator boardMutator_ = new BoardMutator();

    private bool hasLoggedMissingReferences_;

    public GameState(
        BoardController board,
        PlayerController[] players,
        MovementRuleResolver movementRuleResolver,
        PlayerActionResolver playerActionResolver
    )
    {
        board_ = board;
        players_ = players != null
            ? players
            : new PlayerController[0];
        movementRuleResolver_ = movementRuleResolver;
        playerActionResolver_ = playerActionResolver;
    }

    public void AdvanceTick(float deltaTime)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        for (int i = 0; i < players_.Length; i++)
        {
            TickPlayer(players_[i], deltaTime);
        }
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

        MovementRuleData ruleData = movementRuleResolver_ != null
            ? movementRuleResolver_.ResolveRules(playerController, direction)
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

        if (playerActionResolver_ == null)
        {
            return;
        }

        int currentTileID = board_.GetTileID(playerController.CurrentTile);
        BoardMutationData mutationData = playerActionResolver_.ResolveActions(
            actionInput,
            currentTileID
        );

        boardMutator_.Apply(board_, mutationData);
    }
}
