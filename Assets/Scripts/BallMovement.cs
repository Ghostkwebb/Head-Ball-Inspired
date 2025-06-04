using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;

    [Header("Kick Physics")]
    public float straightShootSpeed = 15f;
    public float arcForce = 12f;
    public float arcAngle = 45f; // This can still be used by both Player and AI initiated arc kicks

    [Header("Sound Effects")]
    public AudioClip normalKickSound;
    public AudioClip arcKickSound;
    public AudioClip headKickSound;
    private AudioSource audioSource;

    [Header("Visual Effects (Prefabs)")]
    public GameObject normalKickVFX;
    public GameObject arcKickVFX;
    public GameObject headImpactVFX;

    // Enum to define kick types for clarity
    public enum KickType { Normal, Arc, Head }

    void Awake() // Changed from Start to ensure AudioSource is ready before others might call
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        HandleGameStates();
    }

    void HandleGameStates()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.isGamePaused))
        {
            return;
        }
    }

    // --- Public Methods to be Called by Player/AI ---

    public void ExecuteStraightShot(GameObject kicker, float direction)
    {
        // Attempt to get Player or EnemyAI component from the kicker
        Player kickerPlayerScript = kicker.GetComponent<Player>();
        EnemyAI kickerAIScript = kicker.GetComponent<EnemyAI>(); // Assuming EnemyAI script also has an isGrounded property

        bool kickerIsGrounded = true; // Default to true
        float kickerYPos = kicker.transform.position.y;
        float kickerHeightOffset = 0.5f; // Approximate height where ball needs to be above for a headshot

        if (kickerPlayerScript != null)
        {
            kickerIsGrounded = kickerPlayerScript.isGrounded;
        }
        else if (kickerAIScript != null)
        {
            kickerIsGrounded = kickerAIScript.isGrounded; // Use AI's grounded status
        }

        // Ball's Y must be above kicker's effective head height and kicker must be in air
        bool isHeadContext = !kickerIsGrounded && (transform.position.y > kickerYPos + kickerHeightOffset);

        rb.isKinematic = false;
        Vector2 shootDirectionVec = new Vector2(direction, 0).normalized;
        rb.linearVelocity = Vector2.zero; // Clear previous velocity before applying new one
        rb.AddForce(shootDirectionVec * straightShootSpeed, ForceMode2D.Impulse); // Using AddForce for consistency

        PlayEffects(isHeadContext ? KickType.Head : KickType.Normal, transform.position, shootDirectionVec);
    }

    public void ExecuteArcShot(GameObject kicker, float direction)
    {
        Player kickerPlayerScript = kicker.GetComponent<Player>();
        EnemyAI kickerAIScript = kicker.GetComponent<EnemyAI>();

        bool kickerIsGrounded = true;
        float kickerYPos = kicker.transform.position.y;
        float kickerHeightOffset = 0.5f;

        if (kickerPlayerScript != null)
        {
            kickerIsGrounded = kickerPlayerScript.isGrounded;
        }
        else if (kickerAIScript != null)
        {
            kickerIsGrounded = kickerAIScript.isGrounded;
        }

        bool isHeadContext = !kickerIsGrounded && (transform.position.y > kickerYPos + kickerHeightOffset);

        rb.isKinematic = false;
        rb.linearVelocity = Vector2.zero;

        float angleRad = arcAngle * Mathf.Deg2Rad;
        Vector2 force = new Vector2(Mathf.Cos(angleRad) * direction, Mathf.Sin(angleRad)) * arcForce;
        rb.AddForce(force, ForceMode2D.Impulse);

        PlayEffects(isHeadContext ? KickType.Head : KickType.Arc, transform.position, force.normalized);
    }

    private void PlayEffects(KickType type, Vector3 effectPosition, Vector2 effectDirection)
    {
        AudioClip soundToPlay = null;
        GameObject vfxToSpawn = null;
        Vector3 vfxOffset = Vector3.zero; // Default no offset

        switch (type)
        {
            case KickType.Normal:
                soundToPlay = normalKickSound;
                vfxToSpawn = normalKickVFX;
                // Optionally, position VFX slightly in front of the ball
                vfxOffset = effectDirection.normalized * 0.2f;
                break;
            case KickType.Arc:
                soundToPlay = arcKickSound;
                vfxToSpawn = arcKickVFX;
                vfxOffset = effectDirection.normalized * 0.2f;
                break;
            case KickType.Head:
                soundToPlay = headKickSound;
                vfxToSpawn = headImpactVFX;
                break;
        }

        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }

        if (vfxToSpawn != null)
        {
            Quaternion vfxRotation = Quaternion.identity; // Default no rotation
            if (effectDirection != Vector2.zero)
            {
                // Simple angle for 2D sprites if needed, or LookRotation if VFX is 3D oriented
                 float angle = Mathf.Atan2(effectDirection.y, effectDirection.x) * Mathf.Rad2Deg;
                 vfxRotation = Quaternion.AngleAxis(angle, Vector3.forward); // Rotate around Z for 2D XY plane
            }

            GameObject vfxInstance = Instantiate(vfxToSpawn, effectPosition + vfxOffset, vfxRotation);
            // The VFXAutoDestroy script (from previous response) should be on the VFX prefab.
        }
    }
}