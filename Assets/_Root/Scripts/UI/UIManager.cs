using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Player 1 UI (Top Left)")]
    public TextMeshProUGUI player1RuleText;
    public TextMeshProUGUI player1GoalText;

    [Header("Player 2 UI (Top Right)")]
    public TextMeshProUGUI player2RuleText;
    public TextMeshProUGUI player2GoalText;

    public void UpdatePlayer1UI(string rule, string goal)
    {
        if (player1RuleText != null) player1RuleText.text = $"Rule: {rule}";
        if (player1GoalText != null) player1GoalText.text = $"Goal: {goal}";
    }

    public void UpdatePlayer2UI(string rule, string goal)
    {
        if (player2RuleText != null) player2RuleText.text = $"Rule: {rule}";
        if (player2GoalText != null) player2GoalText.text = $"Goal: {goal}";
    }
}