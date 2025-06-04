using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Still needed if you ever interact with Button components directly
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

    public GameObject pauseMenuUI; // Assign your Pause Menu Panel here
    public GameObject endGameMainMenuButton; // Assign your standalone end-game Main Menu button
    public static bool isGamePaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        isGamePaused = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (enemyTransform == null) enemyTransform = GameObject.FindGameObjectWithTag("Enemy")?.transform;
        if (ballTransform == null) ballTransform = GameObject.FindGameObjectWithTag("Ball")?.transform;
        if (ballRb == null && ballTransform != null) ballRb = ballTransform.GetComponent<Rigidbody2D>();

        if (playerTransform) playerStartPos = playerTransform.position;
        if (enemyTransform) enemyStartPos = enemyTransform.position;
        if (ballTransform) ballStartPos = ballTransform.position;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (endGameMainMenuButton != null) endGameMainMenuButton.SetActive(false); // Hide at start
        InitializeGame();
    }

    void InitializeGame()
    {
        isGameOver = false;
        player1Score = 0;
        player2Score = 0;
        currentTime = gameDuration;
        isTimerRunning = true;
        isGamePaused = false;
        Time.timeScale = 1f;

        UpdateScoreUI();
        UpdateTimerUI();
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (winnerText != null) winnerText.gameObject.SetActive(false);

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (endGameMainMenuButton != null) endGameMainMenuButton.SetActive(false); // Hide on re-init

        ResetPositionsAfterGoal(true);
    }

    void Update()
    {
        HandlePauseInput();
        HandleGameTimer();
    }

    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameOver) // If game is over, Escape key does nothing related to pause menu
            {
                return;
            }

            // Toggle pause menu if game is not over
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void HandleGameTimer()
    {
        if (!isGameOver && !isGamePaused && isTimerRunning)
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

    public void ResumeGame()
    {
        if (isGameOver)
        {
            Debug.Log("Cannot resume: Game is Over.");
            return;
        }

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
        Debug.Log("Game Resumed");
    }

    void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;
        Debug.Log("Game Paused");
    }

    public void LoadMainMenuFromPause() // This will be used by BOTH pause menu and end game button
    {
        Time.timeScale = 1f;
        isGamePaused = false; // Reset state
        isGameOver = false;  // Reset state
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGameFromPause()
    {
        Time.timeScale = 1f;
        Debug.Log("Quitting game from pause menu...");
        Application.Quit();
    }

    public void ScoreGoal(bool player1Scored)
    {
        if (isGameOver) return;

        if (player1Scored)
        {
            player1Score++;
        }
        else
        {
            player2Score++;
        }

        UpdateScoreUI();
        ResetPositionsAfterGoal(true);
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
        Time.timeScale = 1f;
        isGamePaused = false;

        Debug.Log("Game Over!");

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        string winnerMessage;
        if (player1Score > player2Score)
        {
            winnerMessage = "Player 1 Wins!";
        }
        else if (player2Score > player1Score)
        {
            winnerMessage = "CPU Wins!";
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

        // Ensure full pause menu is hidden if it was somehow active
        if (pauseMenuUI != null)
        {
             pauseMenuUI.SetActive(false);
        }

        // Show only the dedicated end-game Main Menu button
        if (endGameMainMenuButton != null)
        {
            endGameMainMenuButton.SetActive(true);
        }
    }

    public void RestartGameLogic()
    {
        InitializeGame(); // This now hides the endGameMainMenuButton correctly
    }

    public void RestartGameScene()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        isGameOver = false; // Ensure game over is reset before scene reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}