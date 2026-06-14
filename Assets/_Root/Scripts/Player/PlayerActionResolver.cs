using System.Collections.Generic;
using UnityEngine;

public enum PassivePlayerAbility
{
    None,
    PaintCurrentTile
}

public enum ActivePlayerAbility
{
    None = 0,
    HideFirstOpponent = 2,
    SwapWithFirstOpponent = 3
}

public class PlayerActionResolver : MonoBehaviour
{
    [Header("Passive Ability")]
    public PassivePlayerAbility passiveAbility = PassivePlayerAbility.None;
    public Material passivePaintMaterial;

    [Header("Active Ability")]
    public ActivePlayerAbility activeAbility = ActivePlayerAbility.None;
    public float hideOpponentSeconds = 2f;

    public bool CanPaintCurrentTile => passiveAbility == PassivePlayerAbility.PaintCurrentTile;

    private bool hasLoggedMissingPassivePaintMaterial_;

    public PlayerActionResult ResolveActions(
        PlayerActionInput input,
        PlayerActionContext context
    )
    {
        if (context == null)
        {
            return PlayerActionResult.Empty;
        }

        List<BoardMutation> boardMutations = new List<BoardMutation>();
        List<PlayerEffect> playerEffects = new List<PlayerEffect>();

        if (input.HasPassiveAction)
        {
            ResolvePassiveAbility(context, boardMutations);
        }

        if (input.HasActiveAction)
        {
            ResolveActiveAbility(context, playerEffects);
        }

        if (boardMutations.Count == 0 && playerEffects.Count == 0)
        {
            return PlayerActionResult.Empty;
        }

        return new PlayerActionResult(
            new BoardMutationData(boardMutations),
            playerEffects
        );
    }

    public string GetAbilitySummary()
    {
        List<string> summaries = new List<string>();

        if (CanPaintCurrentTile)
        {
            summaries.Add("Paint");
        }

        if (activeAbility == ActivePlayerAbility.HideFirstOpponent)
        {
            summaries.Add($"Hide {hideOpponentSeconds:0.#}s");
        }
        else if (activeAbility == ActivePlayerAbility.SwapWithFirstOpponent)
        {
            summaries.Add("Swap");
        }

        return summaries.Count > 0
            ? string.Join(", ", summaries)
            : string.Empty;
    }

    private void ResolvePassiveAbility(
        PlayerActionContext context,
        List<BoardMutation> boardMutations
    )
    {
        if (!CanPaintCurrentTile)
        {
            return;
        }

        ResolvePassivePaint(context, boardMutations);
    }

    private void ResolvePassivePaint(
        PlayerActionContext context,
        List<BoardMutation> boardMutations
    )
    {
        if (passivePaintMaterial == null)
        {
            if (!hasLoggedMissingPassivePaintMaterial_)
            {
                Debug.LogWarning($"{name}: Passive paint ability needs a Passive Paint Material.");
                hasLoggedMissingPassivePaintMaterial_ = true;
            }

            return;
        }

        if (context.Board != null && context.Board.GetTileMaterial(context.CurrentTileID) == passivePaintMaterial)
        {
            return;
        }

        AddTileMaterialMutation(
            boardMutations,
            context.CurrentTileID,
            passivePaintMaterial
        );
    }

    private void ResolveActiveAbility(
        PlayerActionContext context,
        List<PlayerEffect> playerEffects
    )
    {
        PlayerController opponent = context.GetFirstOpponent();

        if (opponent == null)
        {
            return;
        }

        if (activeAbility == ActivePlayerAbility.HideFirstOpponent)
        {
            playerEffects.Add(PlayerEffect.Hide(context.Actor, opponent, hideOpponentSeconds));
            activeAbility = ActivePlayerAbility.None;
        }
        else if (activeAbility == ActivePlayerAbility.SwapWithFirstOpponent)
        {
            playerEffects.Add(PlayerEffect.Swap(context.Actor, opponent));
            activeAbility = ActivePlayerAbility.None;
        }
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
