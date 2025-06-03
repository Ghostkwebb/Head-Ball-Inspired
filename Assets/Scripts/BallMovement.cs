using UnityEngine;
public class BallMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Player playerScript; 
    public Transform playerTransform; 

    [Header("Kick Interaction")]
    public float kickRange = 1.5f; 

    [Header("Straight Shot")]
    public float straightShootSpeed = 15f;

    [Header("Arc Shot (Physics-Based)")]
    public float arcForce = 12f;       
    public float arcAngle = 45f;      

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerScript == null || playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                playerScript = playerObject.GetComponent<Player>();
                playerTransform = playerObject.transform;
            }
        }

    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        if (playerScript == null || playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool canKick = distanceToPlayer <= kickRange;

        if (!canKick)
        {
            return;
        }

        float playerFacingDirection = (playerTransform.localScale.x < 0) ? -1f : 1f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ShootStraight(playerFacingDirection);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShootArc_Physics(playerFacingDirection);
        }
    }

    void ShootStraight(float direction)
    {
        rb.isKinematic = false;
        Vector2 shootDirection = new Vector2(direction, 0).normalized;
        rb.linearVelocity = shootDirection * straightShootSpeed;
    }

    void ShootArc_Physics(float direction)
    {
        Debug.Log("Arc Shot (Physics)! Distance: " + Vector2.Distance(transform.position, playerTransform.position));
        rb.isKinematic = false; // IMPORTANT: Ensure the Rigidbody is NOT kinematic for physics forces to work
        rb.linearVelocity = Vector2.zero; // Reset any existing velocity for a clean shot

        // Convert angle to radians for trigonometric functions
        float angleRad = arcAngle * Mathf.Deg2Rad;

        // Calculate force components
        // X component uses the direction the player is facing
        // Y component is always upwards (positive sin)
        Vector2 force = new Vector2(Mathf.Cos(angleRad) * direction, Mathf.Sin(angleRad)) * arcForce;

        rb.AddForce(force, ForceMode2D.Impulse); // ForceMode2D.Impulse applies an instant force
    }

}