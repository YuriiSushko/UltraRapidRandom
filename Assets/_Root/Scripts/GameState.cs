using UnityEngine;

public class GameState
{
    private readonly BoardController board_;
    private readonly PlayerController playerController_;
    private readonly PlayerMover playerMover_;
    private readonly MovementRuleResolver movementRuleResolver_;
    private readonly PlayerActionResolver playerActionResolver_;
    private readonly MovementValidator movementValidator_ = new MovementValidator();
    private readonly BoardMutator boardMutator_ = new BoardMutator();

    private bool hasInitialized_;
    private bool hasLoggedMissingReferences_;

    public GameState(
        BoardController board,
        PlayerController playerController,
        PlayerMover playerMover,
        MovementRuleResolver movementRuleResolver,
        PlayerActionResolver playerActionResolver
    )
    {
        board_ = board;
        playerController_ = playerController;
        playerMover_ = playerMover;
        movementRuleResolver_ = movementRuleResolver;
        playerActionResolver_ = playerActionResolver;
    }

    public void AdvanceTick(float deltaTime)
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        TryInitialize();

        if (!hasInitialized_)
        {
            return;
        }

        playerController_.TickTimers(deltaTime);

        TryHandlePlayerMovement();
        TryHandlePlayerActions();

        playerMover_.Tick(deltaTime);
    }

    private bool HasRequiredReferences()
    {
        if (board_ != null && playerController_ != null && playerMover_ != null)
        {
            return true;
        }

        if (!hasLoggedMissingReferences_)
        {
            Debug.LogError("GameState is missing BoardController, PlayerController, or PlayerMover.");
            hasLoggedMissingReferences_ = true;
        }

        return false;
    }

    private void TryInitialize()
    {
        if (hasInitialized_ || !board_.HasGenerated)
        {
            return;
        }

        playerController_.Initialize(board_);
        playerMover_.WarpTo(board_.GetTileWorldPosition(playerController_.CurrentTile));
        hasInitialized_ = true;
    }

    private void TryHandlePlayerMovement()
    {
        if (playerMover_.IsMoving || !playerController_.CanTryMove())
        {
            return;
        }

        Vector2Int direction = playerController_.GatherMovementInput();

        if (direction == Vector2Int.zero)
        {
            return;
        }

        MovementRuleData ruleData = movementRuleResolver_ != null
            ? movementRuleResolver_.ResolveRules(playerController_.CurrentTile, direction)
            : MovementRuleData.Default;

        MovementValidationResult result = movementValidator_.Validate(
            board_,
            playerController_.CurrentTile,
            direction,
            ruleData
        );

        if (!result.IsValid)
        {
            return;
        }

        playerController_.ApplyMovementResult(result);
        playerMover_.MoveTo(result.TargetWorldPosition);
        playerController_.ResetMovementCooldown();
    }

    private void TryHandlePlayerActions()
    {
        PlayerActionInput actionInput = playerController_.GatherActionInput();

        if (!actionInput.HasAnyAction)
        {
            return;
        }

        if (playerActionResolver_ == null)
        {
            return;
        }

        int currentTileID = board_.GetTileID(playerController_.CurrentTile);
        BoardMutationData mutationData = playerActionResolver_.ResolveActions(
            actionInput,
            currentTileID
        );

        boardMutator_.Apply(board_, mutationData);
    }
}
