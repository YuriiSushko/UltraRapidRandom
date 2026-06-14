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
            ResolveActiveAbility(context, boardMutations, playerEffects);
        }

        return new PlayerActionResult(
            new BoardMutationData(boardMutations),
            playerEffects
        );
    }

    private void ResolvePassiveAbility(
        PlayerActionContext context,
        List<BoardMutation> boardMutations
    )
    {
        if (passiveAbility == PassivePlayerAbility.PaintCurrentTile)
        {
            AddTileMaterialMutation(
                boardMutations,
                context.CurrentTileID,
                passivePaintMaterial
            );
        }
    }

    private void ResolveActiveAbility(
        PlayerActionContext context,
        List<BoardMutation> boardMutations,
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
        }
        else if (activeAbility == ActivePlayerAbility.SwapWithFirstOpponent)
        {
            playerEffects.Add(PlayerEffect.Swap(context.Actor, opponent));
        }
    }

    public string GetAbilitySummary()
    {
        string passiveText = GetPassiveAbilityText();
        string activeText = GetActiveAbilityText();

        if (passiveText.Length > 0 && activeText.Length > 0)
        {
            return $"{passiveText}, {activeText}";
        }

        if (passiveText.Length > 0)
        {
            return passiveText;
        }

        return activeText;
    }

    private string GetPassiveAbilityText()
    {
        if (passiveAbility == PassivePlayerAbility.PaintCurrentTile)
        {
            return $"Paint{GetMaterialSuffix(passivePaintMaterial)}";
        }

        return string.Empty;
    }

    private string GetActiveAbilityText()
    {
        if (activeAbility == ActivePlayerAbility.HideFirstOpponent)
        {
            return $"Hide {hideOpponentSeconds:0.#}s";
        }

        if (activeAbility == ActivePlayerAbility.SwapWithFirstOpponent)
        {
            return "Swap";
        }

        return string.Empty;
    }

    private string GetMaterialSuffix(Material material)
    {
        return material != null
            ? $" ({material.name})"
            : string.Empty;
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
