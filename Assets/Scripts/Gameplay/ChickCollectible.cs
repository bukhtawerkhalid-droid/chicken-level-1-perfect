using UnityEngine;

public class ChickCollectible : MonoBehaviour
{
    [SerializeField] private bool collected;
    private Vector3 initialPosition;
    private Transform initialParent;

    private void Awake()
    {
        initialPosition = transform.position;
        initialParent = transform.parent;
    }

    public void ResetCollectible()
    {
        collected = false;
        transform.SetParent(initialParent);
        transform.position = initialPosition;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        ChickChainController chain = other.GetComponentInParent<ChickChainController>();
        if (chain == null)
        {
            return;
        }

        collected = true;
        chain.AttachChick(transform);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        LevelSession.Current?.RegisterChickCollected(this);
    }
}
