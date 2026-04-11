using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float animationResetDelay = 0.12f;

    private Animator animator;
    private Coroutine resetRoutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (animator != null)
            {
                animator.ResetTrigger("Spring");
                animator.SetTrigger("Spring");

                if (resetRoutine != null)
                    StopCoroutine(resetRoutine);

                resetRoutine = StartCoroutine(ResetAnimationAfterDelay());
            }

            Rigidbody2D rb = collision.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                rb.AddForce(new Vector2(0, bounceForce), ForceMode2D.Impulse);
            }
        }
    }

    private System.Collections.IEnumerator ResetAnimationAfterDelay()
    {
        yield return new WaitForSeconds(animationResetDelay);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        resetRoutine = null;
    }
}
