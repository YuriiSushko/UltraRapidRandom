public class PlayerActionContext
{
    public BoardController Board { get; }
    public PlayerController Actor { get; }
    public PlayerController[] Players { get; }
    public int CurrentTileID { get; }

    public PlayerActionContext(
        BoardController board,
        PlayerController actor,
        PlayerController[] players,
        int currentTileID
    )
    {
        Board = board;
        Actor = actor;
        Players = players != null
            ? players
            : new PlayerController[0];
        CurrentTileID = currentTileID;
    }

    public PlayerController GetFirstOpponent()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            PlayerController player = Players[i];

            if (player == null || player == Actor)
            {
                continue;
            }

            if (!player.HasInitialized)
            {
                continue;
            }

            return player;
        }

        return null;
    }
}
