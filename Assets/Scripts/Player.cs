using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    private Rigidbody2D rb;
    public bool isGrounded;
    public Transform groundCheck;

    public bool ballTouching;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (rb.bodyType != RigidbodyType2D.Static) 
            {
                rb.linearVelocity = Vector2.zero; 
            }
            return; 
        }
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Base"))
        {
            isGrounded = true;
        }

        if (other.gameObject.CompareTag("Ball"))
        {
            ballTouching = true;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Base"))
        {
            isGrounded = false;
        }

        if (other.gameObject.CompareTag("Ball"))
        {
            ballTouching = false;
        }
    }
}