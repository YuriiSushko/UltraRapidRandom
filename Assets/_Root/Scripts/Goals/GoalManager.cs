using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("Configuration Limits")]
    [SerializeField] private int minTargetCount = 3;
    [SerializeField] private int maxTargetCount = 8;

    private HashSet<int> paintedTilesP1_ = new HashSet<int>();
    private HashSet<int> paintedTilesP2_ = new HashSet<int>();
    private Dictionary<int, int> paintOwnerByTileID_ = new Dictionary<int, int>();

    private ActiveGoal p1Goal_;
    private ActiveGoal p2Goal_;

    public void RollNewGoals(out string p1Desc, out string p2Desc)
    {
        paintedTilesP1_.Clear();
        paintedTilesP2_.Clear();
        paintOwnerByTileID_.Clear();

        // If either previous goal was CatchOpponent, disallow CatchOpponent for both new goals.
        bool previousHadCatch = (p1Goal_ != null && p1Goal_.Type == GoalType.CatchOpponent)
                              || (p2Goal_ != null && p2Goal_.Type == GoalType.CatchOpponent);

        p1Goal_ = GenerateRandomGoal(allowCatch: !previousHadCatch);
        p2Goal_ = GenerateRandomGoal(allowCatch: !previousHadCatch);

        p1Desc = p1Goal_.Description;
        p2Desc = p2Goal_.Description;
    }

    public ActiveGoal GetGoal(int playerIndex)
    {
        if (playerIndex == 0)
        {
            return p1Goal_;
        }

        if (playerIndex == 1)
        {
            return p2Goal_;
        }

        return null;
    }

    public bool TryGetGoalProgress(int playerIndex, out int currentCount, out int targetCount)
    {
        ActiveGoal activeGoal = GetGoal(playerIndex);

        if (activeGoal == null)
        {
            currentCount = 0;
            targetCount = 0;
            return false;
        }

        currentCount = activeGoal.CurrentCount;
        targetCount = activeGoal.TargetCount;
        return activeGoal.TargetCount > 0;
    }

    private ActiveGoal GenerateRandomGoal(bool allowCatch = true)
    {
        GoalType selected;

        if (!allowCatch)
        {
            // Don't allow CatchOpponent — pick a non-catch type.
            selected = GoalType.PaintTiles;
        }
        else
        {
            selected = (GoalType)Random.Range(0, 2);
        }

        int targetValue = Random.Range(minTargetCount, maxTargetCount + 1);

        switch (selected)
        {
            case GoalType.CatchOpponent:
                return new ActiveGoal(selected, "Catch your opponent! Move onto their square.");
            case GoalType.PaintTiles:
                return new ActiveGoal(selected, $"Paint {targetValue} tiles!", targetValue);
            default:
                return new ActiveGoal(GoalType.CatchOpponent, "Catch your opponent!");
        }
    }

    public int ProcessStepEvaluations(int actingPlayerIndex, Vector2Int currentTile, Vector2Int opponentTile)
    {
        ActiveGoal activeGoal = (actingPlayerIndex == 0) ? p1Goal_ : p2Goal_;

        if (activeGoal == null)
        {
            return 0;
        }

        switch (activeGoal.Type)
        {
            case GoalType.CatchOpponent:
                if (currentTile == opponentTile) return actingPlayerIndex + 1;
                break;
        }

        return 0;
    }

    public int ProcessActionEvaluations(int actingPlayerIndex, PlayerActionResult actionResult)
    {
        if (actionResult == null || actionResult.BoardMutations == null || !actionResult.BoardMutations.HasMutations)
        {
            return 0;
        }

        for (int i = 0; i < actionResult.BoardMutations.Mutations.Count; i++)
        {
            BoardMutation mutation = actionResult.BoardMutations.Mutations[i];

            if (mutation.TileID < 0 || mutation.TileMaterial == null)
            {
                continue;
            }

            int winner = ReportTilePainted(actingPlayerIndex, mutation.TileID);

            if (winner != 0)
            {
                return winner;
            }
        }

        return 0;
    }

    public int ReportTilePainted(int actingPlayerIndex, int tileID)
    {
        if (tileID < 0)
        {
            return 0;
        }

        if (paintOwnerByTileID_.TryGetValue(tileID, out int previousOwner))
        {
            if (previousOwner == actingPlayerIndex)
            {
                return TryCompletePaintGoal(actingPlayerIndex);
            }

            GetPaintedTiles(previousOwner).Remove(tileID);
            UpdatePaintGoalCount(previousOwner);
        }

        paintOwnerByTileID_[tileID] = actingPlayerIndex;
        GetPaintedTiles(actingPlayerIndex).Add(tileID);
        UpdatePaintGoalCount(actingPlayerIndex);

        return TryCompletePaintGoal(actingPlayerIndex);
    }

    private HashSet<int> GetPaintedTiles(int playerIndex)
    {
        return playerIndex == 0
            ? paintedTilesP1_
            : paintedTilesP2_;
    }

    private void UpdatePaintGoalCount(int playerIndex)
    {
        ActiveGoal activeGoal = GetGoal(playerIndex);

        if (activeGoal != null && activeGoal.Type == GoalType.PaintTiles)
        {
            activeGoal.CurrentCount = GetPaintedTiles(playerIndex).Count;
        }
    }

    private int TryCompletePaintGoal(int playerIndex)
    {
        ActiveGoal activeGoal = GetGoal(playerIndex);

        if (activeGoal != null
            && activeGoal.Type == GoalType.PaintTiles
            && activeGoal.TargetCount > 0
            && activeGoal.CurrentCount >= activeGoal.TargetCount)
        {
            return playerIndex + 1;
        }

        return 0;
    }
}
