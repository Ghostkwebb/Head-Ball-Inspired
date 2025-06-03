using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
public class GameManager : MonoBehaviour
{
    // --- Score ---
    public int player1Score = 0;
    public int player2Score = 0;

    // --- Game Object References ---
    public Transform playerTransform;
    public Transform enemyTransform;
    public Transform ballTransform;
    public Rigidbody2D ballRb;

    // --- Initial Positions ---
    private Vector3 playerStartPos;
    private Vector3 enemyStartPos;
    private Vector3 ballStartPos;

    // --- UI References ---
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI winnerText;

    // --- Timer ---
    public float gameDuration = 60f; // 1 minute
    private float currentTime;
    private bool isTimerRunning = false;

    // --- Game State ---
    public bool isGameOver = false;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find objects if not assigned
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (enemyTransform == null) enemyTransform = GameObject.FindGameObjectWithTag("Enemy")?.transform;
        if (ballTransform == null) ballTransform = GameObject.FindGameObjectWithTag("Ball")?.transform;
        if (ballRb == null && ballTransform != null) ballRb = ballTransform.GetComponent<Rigidbody2D>();

        // Store initial positions
        if (playerTransform) playerStartPos = playerTransform.position;
        if (enemyTransform) enemyStartPos = enemyTransform.position;
        if (ballTransform) ballStartPos = ballTransform.position;

        InitializeGame();
    }

    void InitializeGame()
    {
        isGameOver = false;// Reset scores if re-initializing
        player2Score = 0;
        currentTime = gameDuration;
        isTimerRunning = true;

        UpdateScoreUI();
        UpdateTimerUI();
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (winnerText != null) winnerText.gameObject.SetActive(false); 

        ResetPositionsAfterGoal(true); 
    }


    void Update()
    {
        if (!isGameOver && isTimerRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                EndGame();
            }
        }
    }

    public void ScoreGoal(bool player1Scored)
    {
        if (isGameOver) return; // Don't score if game is over

        if (player1Scored)
        {
            player1Score++;
        }
        else
        {
            player2Score++;
        }

        UpdateScoreUI();
        ResetPositionsAfterGoal(true); // Pass true to reset ball velocity for sure
    }

    void UpdateScoreUI()
    {
        if (player1ScoreText != null) player1ScoreText.text = "P1    " + player1Score;
        if (player2ScoreText != null) player2ScoreText.text = player2Score + "    CPU";
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void ResetPositionsAfterGoal(bool forceBallReset)
    {
        if (playerTransform) playerTransform.position = playerStartPos;
        if (enemyTransform)
        {
             enemyTransform.position = enemyStartPos;
             Rigidbody2D enemyRb = enemyTransform.GetComponent<Rigidbody2D>();
             if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;
        }


        if (ballTransform)
        {
            ballTransform.position = ballStartPos;
            if (ballRb)
            {
                ballRb.linearVelocity = Vector2.zero;
                ballRb.angularVelocity = 0f;
            }
        }
    }

    void EndGame()
    {
        isGameOver = true;
        isTimerRunning = false;
        Debug.Log("Game Over!");

        // Stop ball
        if (ballRb)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        // Determine winner
        string winnerMessage;
        if (player1Score > player2Score)
        {
            winnerMessage = "Player 1 Wins!";
        }
        else if (player2Score > player1Score)
        {
            winnerMessage = "Player 2 Wins!";
        }
        else
        {
            winnerMessage = "It's a Draw!";
        }

        if (winnerText != null)
        {
            winnerText.text = winnerMessage;
            winnerText.gameObject.SetActive(true);
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

    }


    // Call this if you prefer a full scene reload to restart
    public void RestartGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Call this to restart the game logic without reloading the scene
    public void RestartGameLogic()
    {
        InitializeGame();
    }
}