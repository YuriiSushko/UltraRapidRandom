using System;
using UnityEngine;

public class GameState
{
    private const float SharedTileVisualOffset = 0.75f;

    private readonly Game game_;
    private readonly BoardController board_;
    private readonly GoalManager goalManager_;
    private readonly PickupController pickupController_;
    private readonly PlayerController[] players_;
    private readonly MovementValidator movementValidator_ = new MovementValidator();
    private readonly BoardMutator boardMutator_ = new BoardMutator();

    public event Action<int> RoundCompleted;

    private bool hasLoggedMissingReferences_;
    private int pendingRoundWinner_;

    public GameState(
        Game game,
        BoardController board,
        GoalManager goalManager,
        PickupController pickupController,
        PlayerController[] players
    )
    {
        game_ = game;
        board_ = board;
        goalManager_ = goalManager;
        pickupController_ = pickupController;
        players_ = players != null
            ? players
            : new PlayerController[0];
    }

    public void AdvanceTick(float deltaTime)
    {
        if (!HasRequiredReferences()) return;

        pendingRoundWinner_ = 0;

        for (int i = 0; i < players_.Length; i++)
        {
            TryInitializePlayer(players_[i]);
        }

        for (int i = 0; i < players_.Length; i++)
        {
            TickPlayer(players_[i], i, deltaTime);

            if (HasPendingRoundCompletion())
            {
                break;
            }
        }

        RefreshPlayerVisualLayout(false);

        for (int i = 0; i < players_.Length; i++)
        {
            TickPlayerMover(players_[i], deltaTime);
        }

        TickPickups(deltaTime);
        EvaluateRoundStatus();
        DispatchRoundCompletion();
    }

    public void SnapPlayersToCurrentTiles()
    {
        RefreshPlayerVisualLayout(true);
    }

    private bool HasRequiredReferences()
    {
        if (game_ != null && board_ != null && players_.Length > 0)
        {
            return true;
        }

        if (!hasLoggedMissingReferences_)
        {
            Debug.LogError("GameState is missing Game, BoardController, or player controllers.");
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

    private void TickPlayer(PlayerController playerController, int playerIndex, float deltaTime)
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

        if (HasPendingRoundCompletion())
        {
            return;
        }

        TryHandlePlayerActions(playerController, playerIndex);
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
        playerController.ResetMovementCooldown();
        TryHandlePickup(playerController, result.TargetTileID);
    }

    private void TryHandlePickup(PlayerController playerController, int tileID)
    {
        if (pickupController_ == null || playerController == null)
        {
            return;
        }

        int playerIndex = GetPlayerIndex(playerController);

        if (playerIndex < 0)
        {
            return;
        }

        PickupCollectionResult pickup = pickupController_.TryCollectAtTile(playerController, tileID);

        if (pickup == null)
        {
            return;
        }

        RefreshPlayerRulesUI(playerIndex);
    }

    private int GetPlayerIndex(PlayerController playerController)
    {
        for (int i = 0; i < players_.Length; i++)
        {
            if (players_[i] == playerController)
            {
                return i;
            }
        }

        return -1;
    }

    private void TickPlayerMover(PlayerController playerController, float deltaTime)
    {
        if (playerController == null)
        {
            return;
        }

        PlayerMover playerMover = playerController.GetComponent<PlayerMover>();

        if (playerMover != null)
        {
            playerMover.Tick(deltaTime);
        }
    }

    private void TickPickups(float deltaTime)
    {
        if (pickupController_ == null)
        {
            return;
        }

        pickupController_.Tick(deltaTime, players_);
    }

    private void RefreshPlayerVisualLayout(bool instant)
    {
        for (int i = 0; i < players_.Length; i++)
        {
            PlayerController player = players_[i];

            if (player == null || !player.HasInitialized)
            {
                continue;
            }

            PlayerMover playerMover = player.GetComponent<PlayerMover>();

            if (playerMover == null)
            {
                continue;
            }

            Vector3 visualPosition = GetPlayerVisualPosition(player, i);

            if (instant)
            {
                playerMover.WarpTo(visualPosition);
            }
            else
            {
                playerMover.MoveTo(visualPosition);
            }
        }
    }

    private Vector3 GetPlayerVisualPosition(PlayerController player, int playerIndex)
    {
        Vector3 tilePosition = board_.GetTileWorldPosition(player.CurrentTile);
        int sameTileCount = CountPlayersOnTile(player.CurrentTile);

        if (sameTileCount <= 1)
        {
            return tilePosition;
        }

        int sameTileOrder = GetPlayerOrderOnTile(player, playerIndex);
        float startOffset = -((sameTileCount - 1) * SharedTileVisualOffset) / 2f;
        float offset = startOffset + sameTileOrder * SharedTileVisualOffset;

        return tilePosition + new Vector3(offset, 0f, 0f);
    }

    private int CountPlayersOnTile(Vector2Int tile)
    {
        int count = 0;

        for (int i = 0; i < players_.Length; i++)
        {
            PlayerController player = players_[i];

            if (player != null && player.HasInitialized && player.CurrentTile == tile)
            {
                count++;
            }
        }

        return count;
    }

    private int GetPlayerOrderOnTile(PlayerController targetPlayer, int targetPlayerIndex)
    {
        int order = 0;

        for (int i = 0; i < players_.Length; i++)
        {
            PlayerController player = players_[i];

            if (player == null || !player.HasInitialized)
            {
                continue;
            }

            if (player.CurrentTile != targetPlayer.CurrentTile)
            {
                continue;
            }

            if (player == targetPlayer && i == targetPlayerIndex)
            {
                return order;
            }

            order++;
        }

        return order;
    }

    private void TryHandlePlayerActions(PlayerController playerController, int playerIndex)
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
        TryCompleteActionGoal(playerIndex, actionResult);

        if (actionResult != null && actionResult.HasAnyResult)
        {
            RefreshPlayerRulesUI(playerIndex);
        }
    }

    private void TryCompleteActionGoal(int playerIndex, PlayerActionResult actionResult)
    {
        if (goalManager_ == null)
        {
            return;
        }

        int winner = goalManager_.ProcessActionEvaluations(playerIndex, actionResult);
        TryRequestRoundCompletion(winner);
    }

    private void RefreshPlayerRulesUI(int playerIndex)
    {
        if (game_ != null)
        {
            game_.RefreshPlayerUI(playerIndex);
        }
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

    private void EvaluateRoundStatus()
    {
        if (HasPendingRoundCompletion())
        {
            return;
        }

        if (players_.Length < 2) return;

        if (goalManager_ == null) return;

        int resultP1 = goalManager_.ProcessStepEvaluations(
            0,
            players_[0].CurrentTile,
            players_[1].CurrentTile
        );

        int resultP2 = goalManager_.ProcessStepEvaluations(
            1,
            players_[1].CurrentTile,
            players_[0].CurrentTile
        );

        if (resultP1 != 0 || resultP2 != 0)
        {
            int winner = (resultP1 != 0) ? 1 : 2;
            TryRequestRoundCompletion(winner);
        }
    }

    private bool TryRequestRoundCompletion(int winner)
    {
        if (winner == 0 || pendingRoundWinner_ != 0)
        {
            return false;
        }

        pendingRoundWinner_ = winner;
        return true;
    }

    private bool HasPendingRoundCompletion()
    {
        return pendingRoundWinner_ != 0;
    }

    private void DispatchRoundCompletion()
    {
        if (pendingRoundWinner_ == 0)
        {
            return;
        }

        int winner = pendingRoundWinner_;
        pendingRoundWinner_ = 0;
        RoundCompleted?.Invoke(winner);
    }

}
