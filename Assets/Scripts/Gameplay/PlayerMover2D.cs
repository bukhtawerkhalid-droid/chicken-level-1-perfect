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

    [Header("Landing & Friction")]
    [SerializeField] private float landingDecelBoost = 2.0f;
    [SerializeField] private float landingGripDuration = 0.3f;

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

    // Landing Logic
    private float landingGripTimer;

    public bool IsInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        HandleInput();
        HandleVisuals();
        
        // Update landing timer
        if (landingGripTimer > 0) landingGripTimer -= Time.deltaTime;
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
        
        // 1. Keyboard Support
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) rawInputX -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) rawInputX += 1f;
        }

        // 2. Touch/Mouse Support
        if (rawInputX == 0)
        {
            float screenWidth = Screen.width;
            float inputPosX = -1f;

            // Prefer explicit Touchscreen support
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                inputPosX = Touchscreen.current.primaryTouch.position.ReadValue().x;
            }
            // Fallback to Mouse (works for touch too in most cases)
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                inputPosX = Mouse.current.position.ReadValue().x;
            }

            if (inputPosX >= 0)
            {
                rawInputX = (inputPosX < screenWidth * 0.5f) ? -1f : 1f;
            }
        }

        if (rawInputX != 0)
        {
            bufferedInputX = rawInputX;
            lastInputTime = Time.time;
        }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we hit something below us, trigger landing grip
        if (collision.relativeVelocity.y > 0.5f)
        {
            landingGripTimer = landingGripDuration;
        }
    }

    private void ApplyMovement()
    {
        float targetXVelocity = moveInputX * moveSpeed;
        float currentXVelocity = rb.linearVelocity.x;
        
        // Use boosted deceleration if we just landed
        float currentDecel = deceleration;
        if (landingGripTimer > 0 && Mathf.Abs(moveInputX) < 0.01f)
        {
            currentDecel *= landingDecelBoost;
        }

        float activeRate = (Mathf.Abs(moveInputX) > 0.01f) ? acceleration : currentDecel;
        float newX = Mathf.MoveTowards(currentXVelocity, targetXVelocity, activeRate * Time.fixedDeltaTime);

        float currentYVelocity = rb.linearVelocity.y;
        float newY = Mathf.Max(currentYVelocity, -maxFallSpeed);

        rb.linearVelocity = new Vector2(newX, newY);
    }

    private void ApplySmartAssistance()
    {
        if (rb.linearVelocity.y > -0.5f) return;

        Vector2 originLeft = (Vector2)transform.position + new Vector2(-bodyWidth, 0);
        Vector2 originRight = (Vector2)transform.position + new Vector2(bodyWidth, 0);

        bool hitLeft = Physics2D.Raycast(originLeft, Vector2.down, raycastLength, groundLayer);
        bool hitRight = Physics2D.Raycast(originRight, Vector2.down, raycastLength, groundLayer);

        if (hitLeft && !hitRight)
        {
            rb.AddForce(Vector2.right * magnetStrength, ForceMode2D.Force);
        }
        else if (!hitLeft && hitRight)
        {
            rb.AddForce(Vector2.left * magnetStrength, ForceMode2D.Force);
        }
    }
}
