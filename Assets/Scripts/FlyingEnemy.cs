using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool moveHorizontal = true;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpawnOffset = 0.6f;

    [Header("Detection")]
    [SerializeField] private int maxHealth = 3;

    private Vector3 startPosition;
    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 moveDirection = Vector3.right;
    private System.Collections.IEnumerator shootRoutine;
    private Transform playerTarget;
    private bool isDead;

    private void Awake()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;
        moveDirection = moveHorizontal ? Vector3.right : Vector3.up;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerTarget = playerObject.transform;
    }

    private void Start()
    {
        if (projectilePrefab != null && shootInterval > 0f)
            StartCoroutine(ShootLoop());
    }

    private void Update()
    {
        Move();
    }

    private System.Collections.IEnumerator ShootLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(shootInterval);

            if (!isDead)
                ShootAtPlayer();
        }
    }

    private void ShootAtPlayer()
    {
        if (projectilePrefab == null)
            return;

        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerTarget = playerObject.transform;
        }

        if (playerTarget == null)
            return;

        Vector2 direction = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
        Vector3 spawnPosition = transform.position + (Vector3)(direction * projectileSpawnOffset);

        GameObject projectileGo = Object.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
    }

    private void Move()
    {
        if (isDead)
            return;

        Vector3 targetPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        if (moveHorizontal)
        {
            float xDiff = targetPos.x - startPosition.x;
            targetPos.x = startPosition.x + Mathf.Clamp(xDiff, -maxDistance, maxDistance);

            if (Mathf.Abs(moveDirection.x) > 0.01f)
            {
                if (spriteRenderer != null)
                    spriteRenderer.flipX = moveDirection.x < 0;
            }

            if (Mathf.Abs(xDiff) >= maxDistance)
                moveDirection.x *= -1f;
        }
        else
        {
            float yDiff = targetPos.y - startPosition.y;
            targetPos.y = startPosition.y + Mathf.Clamp(yDiff, -maxDistance, maxDistance);

            if (Mathf.Abs(yDiff) >= maxDistance)
                moveDirection.y *= -1f;
        }

        transform.position = targetPos;
    }

    public bool TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        isDead = true;

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            Destroy(gameObject);
        }
    }

    public void OnDeathAnimationComplete()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        if (moveHorizontal)
        {
            Gizmos.DrawLine(transform.position - Vector3.right * maxDistance, transform.position + Vector3.right * maxDistance);
        }
        else
        {
            Gizmos.DrawLine(transform.position - Vector3.up * maxDistance, transform.position + Vector3.up * maxDistance);
        }
    }
}
