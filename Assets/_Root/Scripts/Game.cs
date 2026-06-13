using UnityEngine;

public class Game : MonoBehaviour
{
    private BoardController boardController_;
    [SerializeField] private UIManager uiManager_;

    void Awake()
    {
        boardController_ = GetComponent<BoardController>();
    }

    void Start()
    {
        if (uiManager_ != null)
        {
            uiManager_.UpdatePlayer1UI("P1 rule", "P1 goal");
            uiManager_.UpdatePlayer2UI("P2 rule", "P2 goal");
            
            uiManager_.UpdateRoundUI(1);
            uiManager_.TriggerNewRoundPopup(1); //for testing
        }
        else
        {
            Debug.LogError($"UIManager reference is missing on {gameObject.name}! Drag your UI Canvas or Manager object into the Inspector slot.");
        }
    }
}