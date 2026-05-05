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

        // Vertical Abyss Safety (Clamp to bottom instead of teleporting)
        if (pos.y < verticalLimit)
        {
            pos.y = verticalLimit;
            if (rb != null)
            {
                // Stop falling velocity but keep horizontal momentum
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(0, rb.linearVelocity.y));
            }
        }

        transform.position = pos;
    }
}
