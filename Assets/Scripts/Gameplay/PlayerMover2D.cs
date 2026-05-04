using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.6f;
    [SerializeField] private float acceleration = 12f;

    private Rigidbody2D rb;
    private float moveInputX;

    public bool IsInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
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
    }

    private void FixedUpdate()
    {
        float targetX = moveInputX * moveSpeed;
        float nextX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(nextX, rb.linearVelocity.y);
    }
}
