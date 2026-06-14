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
        if (!HasRequiredReferences())
        {
            return;
        }

        for (int i = 0; i < players_.Length; i++)
        {
            TryInitializePlayer(players_[i]);
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

    private void TryInitializePlayer(PlayerController playerController)
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

        if (IsTileOccupiedByOtherPlayer(playerController, result.TargetTile))
        {
            return;
        }

        playerController.ApplyMovementResult(result);
        playerMover.MoveTo(result.TargetWorldPosition);
        playerController.ResetMovementCooldown();
    }

    private bool IsTileOccupiedByOtherPlayer(
        PlayerController movingPlayer,
        Vector2Int targetTile
    )
    {
        for (int i = 0; i < players_.Length; i++)
        {
            PlayerController otherPlayer = players_[i];

            if (otherPlayer == null || otherPlayer == movingPlayer)
            {
                continue;
            }

            if (!otherPlayer.HasInitialized)
            {
                continue;
            }

            if (otherPlayer.CurrentTile == targetTile)
            {
                return true;
            }
        }

        return false;
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

        PlayerActionContext context = new PlayerActionContext(
            board_,
            playerController,
            players_,
            board_.GetTileID(playerController.CurrentTile)
        );
        PlayerActionResult actionResult = playerController.PlayerActionResolver.ResolveActions(
            actionInput,
            context
        );

        ApplyActionResult(actionResult);
    }

    private void ApplyActionResult(PlayerActionResult actionResult)
    {
        if (actionResult == null || !actionResult.HasAnyResult)
        {
            return;
        }

        boardMutator_.Apply(board_, actionResult.BoardMutations);

        for (int i = 0; i < actionResult.PlayerEffects.Count; i++)
        {
            ApplyPlayerEffect(actionResult.PlayerEffects[i]);
        }
    }

    private void ApplyPlayerEffect(PlayerEffect playerEffect)
    {
        if (playerEffect.Type == PlayerEffectType.Hide)
        {
            if (playerEffect.Target != null)
            {
                playerEffect.Target.Hide(playerEffect.Duration);
            }

            return;
        }

        if (playerEffect.Type == PlayerEffectType.SwapTiles)
        {
            SwapPlayers(playerEffect.Source, playerEffect.Target);
        }
    }

    private void SwapPlayers(
        PlayerController firstPlayer,
        PlayerController secondPlayer
    )
    {
        if (firstPlayer == null || secondPlayer == null)
        {
            return;
        }

        Vector2Int firstTile = firstPlayer.CurrentTile;
        Vector2Int secondTile = secondPlayer.CurrentTile;

        firstPlayer.SetCurrentTile(secondTile);
        secondPlayer.SetCurrentTile(firstTile);

        WarpPlayerToCurrentTile(firstPlayer);
        WarpPlayerToCurrentTile(secondPlayer);
    }

    private void WarpPlayerToCurrentTile(PlayerController playerController)
    {
        PlayerMover playerMover = playerController.GetComponent<PlayerMover>();

        if (playerMover == null)
        {
            return;
        }

        playerMover.WarpTo(board_.GetTileWorldPosition(playerController.CurrentTile));
    }
}
