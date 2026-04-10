using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private float range;
    private bool isRevolverBullet;
    private Vector2 origin;

    private WeaponController owner;

    public void Init(Vector2 direction, float speed, float range, int damage, bool isRevolverBullet, WeaponController owner)
    {
        this.damage = damage;
        this.range = range;
        this.isRevolverBullet = isRevolverBullet;
        this.owner = owner;
        this.origin = transform.position;

        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (Vector2.Distance(origin, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            bool killed = enemy.TakeDamage(damage);

            if (isRevolverBullet && killed)
            {
                owner?.OnRevolverKill();
            }

            Destroy(gameObject);
            return;
        }

        // Destroy on anything tagged as ground/wall, ignore player
        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
