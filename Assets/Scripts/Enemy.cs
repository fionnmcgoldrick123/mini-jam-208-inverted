using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
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
        Destroy(gameObject);
    }
}

