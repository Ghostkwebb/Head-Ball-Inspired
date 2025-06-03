using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float jumpForce = 15f;
    public float kickRange = 1.8f; 
    public float kickCooldown = 1.5f; 

    public Transform ballTransform;
    public Rigidbody2D ballRb;
    public Transform ownGoal; 
    public Transform opponentGoal;

    private Rigidbody2D rb;
    private bool isGrounded;
    public Transform groundCheck;
    private float timeSinceLastKick = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ballTransform == null)
        {
            GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
            if (ballObject != null) {
                ballTransform = ballObject.transform;
                ballRb = ballObject.GetComponent<Rigidbody2D>();
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (rb != null && rb.bodyType != RigidbodyType2D.Static) 
            {
                rb.linearVelocity = Vector2.zero;
            }
            return; 
        }

        if (ballTransform == null) return;

        timeSinceLastKick += Time.deltaTime;

        // Basic Movement: Move towards the ball's X position
        float directionToBall = ballTransform.position.x - transform.position.x;
        float moveDirection = 0f;

        // Defensive positioning: if ball is moving towards own goal, try to intercept
        if (ballRb.linearVelocity.x < -0.1f && transform.position.x > ballTransform.position.x) // Ball moving towards AI's left (assuming AI on right)
        {
             moveDirection = -1f;
        }
        else if (ballRb.linearVelocity.x > 0.1f && transform.position.x < ballTransform.position.x) // Ball moving towards AI's right (assuming AI on left)
        {
            moveDirection = 1f;
        }
        else // Otherwise, generally track the ball
        {
            if (directionToBall > 0.2f) moveDirection = 1f;
            else if (directionToBall < -0.2f) moveDirection = -1f;
        }


        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

        // Jumping: Jump if ball is above and AI is grounded
        if (isGrounded && ballTransform.position.y > transform.position.y + 1.0f && Mathf.Abs(directionToBall) < 2.0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Kicking: If close enough to the ball and cooldown is over
        float distanceToBall = Vector2.Distance(transform.position, ballTransform.position);
        if (distanceToBall <= kickRange && timeSinceLastKick >= kickCooldown)
        {
            KickBall();
            timeSinceLastKick = 0f;
        }
    }

    void KickBall()
    {
        if (ballRb == null) return;

        // Determine kick direction (towards opponent's goal)
        float kickDirection = (opponentGoal.position.x > transform.position.x) ? 1f : -1f;

        // Simple straight kick, similar to player's
        Vector2 shootDirection = new Vector2(kickDirection, Random.Range(0.1f, 0.5f)).normalized; // Add slight upward angle
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector2.zero; // Reset ball velocity before applying new force
        ballRb.AddForce(shootDirection * 12f, ForceMode2D.Impulse); // Adjust kick power as needed
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