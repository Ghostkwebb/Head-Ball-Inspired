using UnityEngine;

public class VFXAutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;

    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = GetComponentInChildren<ParticleSystem>(); // Check children too
        }

        if (ps == null)
        {
            // If no particle system, destroy after a fixed time as a fallback
            Destroy(gameObject, 3f);
            Debug.LogWarning("VFXAutoDestroy: No ParticleSystem found on " + gameObject.name + ". Will self-destruct in 3s.");
        }
    }

    public void Update()
    {
        if (ps != null)
        {
            if (!ps.IsAlive()) // IsAlive checks if the system and all its particles are dead
            {
                Destroy(gameObject);
            }
        }
        // If ps is null, Start already scheduled a delayed destroy
    }
}