using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float kickRange = 1.5f;
    public float kickCooldown = 1.5f;

    public Transform ballTransform;
    public Rigidbody2D ballRb;
    public Transform ownGoal;
    public Transform opponentGoal;
    public Transform groundCheck; // Ensure this is assigned in Inspector

    public BallMovement ballMovementScript; // Assign Ball's BallMovement script

    private Rigidbody2D rb;
    public bool isGrounded;
    private float timeSinceLastKick = 0f;

    private float baseMoveSpeed;
    private float baseJumpForce;
    private float baseKickRange;
    private float baseKickCooldown;

    void Awake() // Use Awake to capture base values and apply difficulty before Start
    {
        rb = GetComponent<Rigidbody2D>();

        // Capture base values from what's set in the Inspector
        baseMoveSpeed = moveSpeed;
        baseJumpForce = jumpForce;
        baseKickRange = kickRange;
        baseKickCooldown = kickCooldown;

        LoadAndApplyDifficulty();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeBallReferences();
        if (ballMovementScript == null && ballTransform != null)
        {
            ballMovementScript = ballTransform.GetComponent<BallMovement>();
        }
    }

    void LoadAndApplyDifficulty()
    {
        // Use the same key and default value as in DifficultySettings.cs
        float difficultyLevel = PlayerPrefs.GetFloat(DifficultySettings.DifficultyKey, DifficultySettings.DefaultDifficulty);
        Debug.Log("EnemyAI Loaded Difficulty: " + difficultyLevel);

        // --- Apply difficulty modifications ---
        // Scale factors:
        // For speed: Level 1 -> 0.7x base, Level 5 (default) -> 1.0x base, Level 10 -> 1.3x base
        // For cooldown: Level 1 -> 1.3x base, Level 5 (default) -> 1.0x base, Level 10 -> 0.7x base
        // Lerp (value - min) / (range) to get a 0-1 value for interpolation. Slider is 1-10.
        float normalizedDifficulty = (difficultyLevel - 1f) / (10f - 1f); // (current - min) / (max - min)

        moveSpeed = baseMoveSpeed * Mathf.Lerp(0.7f, 1.3f, normalizedDifficulty);
        kickCooldown = baseKickCooldown * Mathf.Lerp(1.3f, 0.7f, normalizedDifficulty);

        // Example: Optionally adjust jump force too
        // jumpForce = baseJumpForce * Mathf.Lerp(0.8f, 1.2f, normalizedDifficulty);

        // Kick range might be better to keep consistent or have less variation
        // kickRange = baseKickRange * Mathf.Lerp(0.9f, 1.1f, normalizedDifficulty);

        Debug.Log($"EnemyAI Applied Params: Speed={moveSpeed:F2}, JumpF={jumpForce:F2}, KickR={kickRange:F2}, KickCD={kickCooldown:F2}");
    }

    void InitializeBallReferences()
    {
        if (ballTransform == null)
        {
            GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
            if (ballObject != null)
            {
                ballTransform = ballObject.transform;
                ballRb = ballObject.GetComponent<Rigidbody2D>();
            }
            else
            {
                Debug.LogError("EnemyAI: Ball not found. Ensure ball has 'Ball' tag.");
            }
        }
    }

    void Update()
    {
        if (CheckGameOver()) return;
        if (IsBallMissing()) return;

        UpdateTimeSinceLastKick();
        ProcessMovement();
        ProcessJumping();
        ProcessKicking();
    }

    bool CheckGameOver()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = Vector2.zero; // Stop movement
            }
            return true; // Game is over, do nothing further
        }
        return false; // Game is not over
    }

    bool IsBallMissing()
    {
        if (ballTransform == null)
        {
            // Attempt to re-initialize if ball was lost (e.g. due to scene reset without script re-init)
            InitializeBallReferences();
            if (ballTransform == null) return true; // Still missing after re-check
        }
        return false;
    }

    void UpdateTimeSinceLastKick()
    {
        timeSinceLastKick += Time.deltaTime;
    }

    void ProcessMovement()
    {
        float directionToBall = ballTransform.position.x - transform.position.x;
        float moveDirection = CalculateMoveDirection(directionToBall);
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    float CalculateMoveDirection(float directionToBall)
    {
        float moveDir = 0f;

        // Defensive positioning
        if (ballRb != null) // Check if ballRb is assigned
        {
            bool ballMovingTowardsOwnGoalLeft = ballRb.linearVelocity.x < -0.1f && transform.position.x > ballTransform.position.x;
            bool ballMovingTowardsOwnGoalRight = ballRb.linearVelocity.x > 0.1f && transform.position.x < ballTransform.position.x;

            if (ballMovingTowardsOwnGoalLeft)
            {
                moveDir = -1f;
            }
            else if (ballMovingTowardsOwnGoalRight)
            {
                moveDir = 1f;
            }
            else // General tracking
            {
                if (directionToBall > 0.2f) moveDir = 1f;
                else if (directionToBall < -0.2f) moveDir = -1f;
            }
        }
        else // Fallback if ballRb is not available (e.g., ball destroyed or not yet found)
        {
             if (directionToBall > 0.2f) moveDir = 1f;
             else if (directionToBall < -0.2f) moveDir = -1f;
        }
        return moveDir;
    }

    void ProcessJumping()
    {
        float directionToBall = ballTransform.position.x - transform.position.x; // Recalculate or pass as parameter if needed
        if (isGrounded && ballTransform.position.y > transform.position.y + 1.0f && Mathf.Abs(directionToBall) < 2.0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void ProcessKicking()
    {
        float distanceToBall = Vector2.Distance(transform.position, ballTransform.position);
        if (distanceToBall <= kickRange && timeSinceLastKick >= kickCooldown)
        {
            KickBall();
            timeSinceLastKick = 0f;
        }
    }

    void KickBall() // This method is called from ProcessKicking()
    {
        if (ballMovementScript == null) return;

        float kickDirection = (opponentGoal.position.x > transform.position.x) ? 1f : -1f;

        // AI decision for kick type (can be randomized or strategic)
        bool preferArcKick = Random.Range(0, 100) < 30; // 30% chance for an arc kick

        if (preferArcKick)
        {
            ballMovementScript.ExecuteArcShot(gameObject, kickDirection);
        }
        else
        {
            ballMovementScript.ExecuteStraightShot(gameObject, kickDirection);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Base"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Base"))
        {
            isGrounded = false;
        }
    }
}