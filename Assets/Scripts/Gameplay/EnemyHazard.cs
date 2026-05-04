using UnityEngine;

public class EnemyHazard : MonoBehaviour
{
    [SerializeField] private float hitCooldown = 0.5f;
    private float nextHitTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null)
        {
            TryApplyHit(collision.collider);
        }
    }

    private void TryApplyHit(Collider2D other)
    {
        if (Time.time < nextHitTime)
        {
            return;
        }

        ChickChainController chain = other.GetComponentInParent<ChickChainController>();
        if (chain == null)
        {
            return;
        }

        nextHitTime = Time.time + hitCooldown;
        LevelSession.Current?.HandleEnemyTouch(chain);
    }
}
