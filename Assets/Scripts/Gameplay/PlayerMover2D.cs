using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float maxFallSpeed = 12f;

    [Header("Smart Assistance")]
    [SerializeField] private bool enableGapMagnet = true;
    [SerializeField] private float magnetStrength = 5f;
    [SerializeField] private float raycastLength = 0.6f;
    [SerializeField] private float bodyWidth = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual Settings")]
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float moveInputX;
    
    // Input Buffering
    private float lastInputTime;
    private float inputBufferWindow = 0.15f;
    private float bufferedInputX;

    public bool IsInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Auto-assign ground layer if not set
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        HandleInput();
        HandleVisuals();
    }

    private void HandleInput()
    {
        if (!IsInputEnabled)
        {
            moveInputX = 0f;
            bufferedInputX = 0f;
            return;
        }

        float rawInputX = 0f;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) rawInputX -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) rawInputX += 1f;
        }

        if (rawInputX == 0 && (Mouse.current?.leftButton.isPressed ?? false))
        {
            float screenWidth = Screen.width;
            float mouseX = Mouse.current.position.ReadValue().x;
            rawInputX = (mouseX < screenWidth * 0.5f) ? -1f : 1f;
        }

        // Input Buffering: Remember the last intentional move
        if (rawInputX != 0)
        {
            bufferedInputX = rawInputX;
            lastInputTime = Time.time;
        }

        // Use buffered input if within window, else use current raw input
        if (Time.time - lastInputTime <= inputBufferWindow)
        {
            moveInputX = bufferedInputX;
        }
        else
        {
            moveInputX = rawInputX;
        }
    }

    private void HandleVisuals()
    {
        if (moveInputX != 0)
        {
            spriteRenderer.flipX = moveInputX > 0;
        }

        float targetTilt = -moveInputX * tiltAmount;
        float currentTilt = transform.localEulerAngles.z;
        if (currentTilt > 180) currentTilt -= 360;
        
        float newTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, newTilt);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        if (enableGapMagnet) ApplySmartAssistance();
    }

    private void ApplyMovement()
    {
        float targetXVelocity = moveInputX * moveSpeed;
        float currentXVelocity = rb.linearVelocity.x;
        float activeRate = (Mathf.Abs(moveInputX) > 0.01f) ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(currentXVelocity, targetXVelocity, activeRate * Time.fixedDeltaTime);

        float currentYVelocity = rb.linearVelocity.y;
        float newY = Mathf.Max(currentYVelocity, -maxFallSpeed);

        rb.linearVelocity = new Vector2(newX, newY);
    }

    private void ApplySmartAssistance()
    {
        // Only assist when falling
        if (rb.linearVelocity.y > -0.5f) return;

        // Cast rays down from left and right edges
        Vector2 originLeft = (Vector2)transform.position + new Vector2(-bodyWidth, 0);
        Vector2 originRight = (Vector2)transform.position + new Vector2(bodyWidth, 0);

        bool hitLeft = Physics2D.Raycast(originLeft, Vector2.down, raycastLength, groundLayer);
        bool hitRight = Physics2D.Raycast(originRight, Vector2.down, raycastLength, groundLayer);

        // Debug visualization (visible in Scene view)
        Debug.DrawRay(originLeft, Vector2.down * raycastLength, hitLeft ? Color.red : Color.green);
        Debug.DrawRay(originRight, Vector2.down * raycastLength, hitRight ? Color.red : Color.green);

        // If one side hits and the other doesn't, we are grazing an edge!
        if (hitLeft && !hitRight)
        {
            // Nudge right into the gap
            rb.AddForce(Vector2.right * magnetStrength, ForceMode2D.Force);
        }
        else if (!hitLeft && hitRight)
        {
            // Nudge left into the gap
            rb.AddForce(Vector2.left * magnetStrength, ForceMode2D.Force);
        }
    }
}
