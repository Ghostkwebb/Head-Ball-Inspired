using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    private Rigidbody2D rb;
    public bool isGrounded;
    public Transform groundCheck; 

    [Header("Kick Settings")]
    public float kickRange = 1.5f;
    public BallMovement ballMovementScript; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ballMovementScript == null)
        {
            GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
            if (ballObject != null)
            {
                ballMovementScript = ballObject.GetComponent<BallMovement>();
            }
            else
            {
                Debug.LogError("Player: BallMovement script not found on a 'Ball' tagged object.");
            }
        }
    }

    void Update()
    {
        if (IsGameInactive()) return;

        HandleMovement();
        HandleKickInput();
    }

    bool IsGameInactive()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return true;
        }
        if (GameManager.isGamePaused)
        {
            return true;
        }
        return false;
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void HandleKickInput()
    {
        if (ballMovementScript == null) return;

        float distanceToBall = Vector2.Distance(transform.position, ballMovementScript.transform.position);
        if (distanceToBall <= kickRange)
        {
            float playerFacingDirection = (transform.localScale.x < 0) ? -1f : 1f;

            if (Input.GetKeyDown(KeyCode.E)) // Straight kick
            {
                ballMovementScript.ExecuteStraightShot(gameObject, playerFacingDirection);
            }

            if (Input.GetKeyDown(KeyCode.Q)) // Arc kick
            {
                ballMovementScript.ExecuteArcShot(gameObject, playerFacingDirection);
            }
        }
    }

    // Ground Check - Can be done with Physics2D.OverlapCircle in HandleMovement or Update as well
    void FixedUpdate() // More reliable for physics checks like ground detection
    {
        CheckGrounded();
    }

    void CheckGrounded()
    {
        if (groundCheck == null)
        {
            return;
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