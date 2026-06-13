using System.Collections.Generic;
using UnityEngine;

public class PlayerActionResolver : MonoBehaviour
{
    [Header("Mock Passive Action")]
    public bool mutateCurrentTileOnPassiveAction = false;
    public Material passiveTileMaterial;

    [Header("Mock Active Action")]
    public bool mutateCurrentTileOnActiveAction = false;
    public Material activeTileMaterial;

    public BoardMutationData ResolveActions(PlayerActionInput input, int currentTileID)
    {
        List<BoardMutation> mutations = new List<BoardMutation>();

        if (input.HasPassiveAction && mutateCurrentTileOnPassiveAction)
        {
            AddTileMaterialMutation(mutations, currentTileID, passiveTileMaterial);
        }

        if (input.HasActiveAction && mutateCurrentTileOnActiveAction)
        {
            AddTileMaterialMutation(mutations, currentTileID, activeTileMaterial);
        }

        if (mutations.Count == 0)
        {
            return BoardMutationData.Empty;
        }

        return new BoardMutationData(mutations);
    }

    private void AddTileMaterialMutation(
        List<BoardMutation> mutations,
        int tileID,
        Material material
    )
    {
        if (tileID < 0 || material == null)
        {
            return;
        }

        mutations.Add(new BoardMutation(tileID, material));
    }
}
