using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Player 1 UI (Top Left)")]
    public TextMeshProUGUI player1RuleText;
    public TextMeshProUGUI player1GoalText;

    [Header("Player 2 UI (Top Right)")]
    public TextMeshProUGUI player2RuleText;
    public TextMeshProUGUI player2GoalText;

    [Header("Round UI (Top Center)")]
    public TextMeshProUGUI roundText;

    [Header("New Round Popup (Center)")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    [SerializeField] private float popupDisplayDuration = 3f;

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

    public void UpdateRoundUI(int roundNumber)
    {
        if (roundText != null) roundText.text = $"ROUND {roundNumber}";
    }

    public void TriggerNewRoundPopup(int playerNumber)
    {
        if (popupPanel != null && popupText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHidePopup(playerNumber));
        }
    }

    private IEnumerator ShowAndHidePopup(int playerNumber)
    {
        popupText.text = $"Player {playerNumber} wins this one, next round!";
        popupPanel.SetActive(true);

        yield return new WaitForSeconds(popupDisplayDuration);

        popupPanel.SetActive(false);
    }
}