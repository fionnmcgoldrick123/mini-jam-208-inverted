using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float groundAcceleration = 80f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float airAcceleration = 60f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float baseGravityScale = 3f;
    [SerializeField] private float fallGravityScale = 6f;
    [SerializeField] private float jumpCutGravityScale = 12f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Health")]
    [SerializeField] private bool isDead = false;

    public UnityEvent OnPlayerDeath = new UnityEvent();

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpCut;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = baseGravityScale;
    }

    private void Update()
    {
        if (isDead) return;

        wasGrounded = isGrounded;
        UpdateGroundedState();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        ApplyMovement();
        ApplyGravityScale();
        TryJump();

        float clampedY = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (wasGrounded == false)
            {
                isJumping = false;
                jumpCut = false;
            }
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && isJumping && rb.linearVelocity.y > 0)
        {
            jumpCut = true;
        }
    }

    private void ApplyMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float targetSpeed = moveX * maxSpeed;

        float accel;
        if (isGrounded)
        {
            accel = Mathf.Abs(moveX) > 0.01f ? groundAcceleration : groundDeceleration;
        }
        else
        {
            accel = Mathf.Abs(moveX) > 0.01f ? airAcceleration : airDeceleration;
        }

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void ApplyGravityScale()
    {
        if (jumpCut && rb.linearVelocity.y > 0)
        {
            rb.gravityScale = jumpCutGravityScale;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravityScale;
        }
        else
        {
            rb.gravityScale = baseGravityScale;
        }
    }

    private void TryJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            isJumping = true;
            jumpCut = false;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        OnPlayerDeath?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
