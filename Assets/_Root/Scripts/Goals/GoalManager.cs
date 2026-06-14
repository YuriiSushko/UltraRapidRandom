using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("Configuration Limits")]
    [SerializeField] private int minTargetCount = 3;
    [SerializeField] private int maxTargetCount = 8;

    private HashSet<int> paintedTilesP1_ = new HashSet<int>();
    private HashSet<int> paintedTilesP2_ = new HashSet<int>();
    
    private HashSet<int> specialTileIDs_ = new HashSet<int>();
    private HashSet<int> spawnedItemTileIDs_ = new HashSet<int>();

    private ActiveGoal p1Goal_;
    private ActiveGoal p2Goal_;
    public void InitializeMapObjectives(BoardController board)
    {
        specialTileIDs_.Clear();
        spawnedItemTileIDs_.Clear();

        int totalTiles = board.Columns * board.Rows;
        if (totalTiles <= 0) return;

        for (int i = 0; i < totalTiles; i++)
        {
            if (i % 6 == 0) specialTileIDs_.Add(i);
            else if (i % 11 == 0) spawnedItemTileIDs_.Add(i);
        }
    }
    
    public void RollNewGoals(out string p1Desc, out string p2Desc)
    {
        paintedTilesP1_.Clear();
        paintedTilesP2_.Clear();

        p1Goal_ = GenerateRandomGoal();
        p2Goal_ = GenerateRandomGoal();

        p1Desc = p1Goal_.Description;
        p2Desc = p2Goal_.Description;
    }

    private ActiveGoal GenerateRandomGoal()
    {
        GoalType selected = (GoalType)Random.Range(0, 4);
        int targetValue = Random.Range(minTargetCount, maxTargetCount + 1);

        switch (selected)
        {
            case GoalType.CatchOpponent:
                return new ActiveGoal(selected, "Catch your opponent! Move onto their square.");
            case GoalType.PickUpObjects:
                return new ActiveGoal(selected, $"Collect {targetValue} glowing items from the map!", targetValue);
            case GoalType.PaintTiles:
                return new ActiveGoal(selected, $"Paint {targetValue} tiles! (Step on unpainted tiles)", targetValue);
            case GoalType.StepOnSpecialTiles:
                return new ActiveGoal(selected, $"Activate {targetValue} special marked tiles!", targetValue);
            default:
                return new ActiveGoal(GoalType.CatchOpponent, "Catch your opponent!");
        }
    }
    
    public int ProcessStepEvaluations(int actingPlayerIndex, Vector2Int currentTile, int currentTileID, Vector2Int opponentTile)
    {
        ActiveGoal activeGoal = (actingPlayerIndex == 0) ? p1Goal_ : p2Goal_;

        switch (activeGoal.Type)
        {
            case GoalType.CatchOpponent:
                if (currentTile == opponentTile) return actingPlayerIndex + 1;
                break;

            case GoalType.PaintTiles:
                HashSet<int> myPaintSet = (actingPlayerIndex == 0) ? paintedTilesP1_ : paintedTilesP2_;
                if (!myPaintSet.Contains(currentTileID))
                {
                    myPaintSet.Add(currentTileID);
                    activeGoal.CurrentCount = myPaintSet.Count;
                    if (activeGoal.CurrentCount >= activeGoal.TargetCount) return actingPlayerIndex + 1;
                }
                break;

            case GoalType.StepOnSpecialTiles:
                if (specialTileIDs_.Contains(currentTileID))
                {
                    activeGoal.CurrentCount++;
                    if (activeGoal.CurrentCount >= activeGoal.TargetCount) return actingPlayerIndex + 1;
                }
                break;

            case GoalType.PickUpObjects:
                if (spawnedItemTileIDs_.Contains(currentTileID))
                {
                    spawnedItemTileIDs_.Remove(currentTileID);
                    
                    activeGoal.CurrentCount++;
                    if (activeGoal.CurrentCount >= activeGoal.TargetCount) return actingPlayerIndex + 1;
                }
                break;
        }

        return 0;
    }

    public bool IsSpecialTile(int id) => specialTileIDs_.Contains(id);
    public bool IsItemTile(int id) => spawnedItemTileIDs_.Contains(id);
}