using UnityEngine;

public class KnockbackHazard : MonoBehaviour
{
    public float knockbackPower = 15f;
    public float stunDuration = 0.6f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Push away from hazard and slightly upward
                Vector3 direction = (other.transform.position - transform.position).normalized;
                direction.y = 0.4f;

                player.ApplyKnockback(direction.normalized * knockbackPower, stunDuration);
            }
        }
    }
}