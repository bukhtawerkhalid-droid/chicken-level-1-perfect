using UnityEngine;

public class ChickCollectible : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private bool collected;
    [SerializeField] private float magnetRadius = 0.5f;
    [SerializeField] private float magnetSpeed = 6.0f;
    
    private Vector3 initialPosition;
    private Transform initialParent;
    private Transform playerTransform;

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

    private void Update()
    {
        if (collected) return;

        // Smart Magnetism
        HandleMagnetism();
    }

    private void HandleMagnetism()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance < magnetRadius)
        {
            // Move toward player with increasing speed
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        ChickChainController chain = other.GetComponentInParent<ChickChainController>();
        if (chain == null) return;

        collected = true;
        chain.AttachChick(transform);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        LevelSession.Current?.RegisterChickCollected(this);
    }
}
