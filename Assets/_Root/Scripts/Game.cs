using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Game : MonoBehaviour
{


    private BoardController boardController_;
    void Awake()
    {
        boardController_ = GetComponent<BoardController>();   
    }

    // Update is called once per frame
    void Update()
    {
    }
}
