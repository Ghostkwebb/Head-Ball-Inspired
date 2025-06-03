using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isPlayer1Goal;
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (gameManager != null)
            {
                Debug.Log("Ball entered goal: " + gameObject.name);
                if (isPlayer1Goal)
                {
                    gameManager.ScoreGoal(false); // Player 2 scored (or enemy)
                }
                else
                {
                    gameManager.ScoreGoal(true); // Player 1 scored
                }
            }
        }
    }
}