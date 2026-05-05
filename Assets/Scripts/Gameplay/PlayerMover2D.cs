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

    [Header("Visual Settings")]
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float moveInputX;
    
    public bool IsInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ensure Rigidbody settings are optimized for smooth movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
            return;
        }

        moveInputX = 0f;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInputX -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInputX += 1f;
        }

        // Also support Mouse/Touch if needed for mobile feel
        if (moveInputX == 0 && (Mouse.current?.leftButton.isPressed ?? false))
        {
            float screenWidth = Screen.width;
            float mouseX = Mouse.current.position.ReadValue().x;
            moveInputX = (mouseX < screenWidth * 0.5f) ? -1f : 1f;
        }
    }

    private void HandleVisuals()
    {
        if (moveInputX != 0)
        {
            spriteRenderer.flipX = moveInputX > 0;
        }

        // Procedural tilt for extra "Juice"
        float targetTilt = -moveInputX * tiltAmount;
        float currentTilt = transform.localEulerAngles.z;
        if (currentTilt > 180) currentTilt -= 360;
        
        float newTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, newTilt);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 1. Horizontal Smoothing (Accel/Decel)
        float targetXVelocity = moveInputX * moveSpeed;
        float currentXVelocity = rb.linearVelocity.x;
        
        // Use acceleration if we are pushing the stick, deceleration if we let go
        float activeRate = (Mathf.Abs(moveInputX) > 0.01f) ? acceleration : deceleration;
        
        float newX = Mathf.MoveTowards(currentXVelocity, targetXVelocity, activeRate * Time.fixedDeltaTime);

        // 2. Terminal Velocity (Stabilization)
        float currentYVelocity = rb.linearVelocity.y;
        float newY = Mathf.Max(currentYVelocity, -maxFallSpeed);

        rb.linearVelocity = new Vector2(newX, newY);
    }
}
