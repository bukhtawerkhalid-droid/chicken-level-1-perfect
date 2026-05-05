using UnityEngine;

public class ScreenWrap2D : MonoBehaviour
{
    [Header("Wrap Settings")]
    [SerializeField] private float leftLimit = -2.7f;
    [SerializeField] private float rightLimit = 2.7f;
    [SerializeField] private float verticalLimit = -10f; // Abyss threshold

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;

        // Horizontal Wrapping (World Space)
        if (pos.x < leftLimit)
        {
            pos.x = rightLimit - 0.1f; // Slight offset to prevent infinite loop
            // No need to reset velocity, let them keep their momentum!
        }
        else if (pos.x > rightLimit)
        {
            pos.x = leftLimit + 0.1f;
        }

        // Vertical Abyss Recovery
        if (pos.y < verticalLimit)
        {
            // Respawn at the top center if they fall off the map
            pos.y = 8f; 
            pos.x = 0;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        transform.position = pos;
    }
}
