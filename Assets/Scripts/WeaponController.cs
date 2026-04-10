using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Gun Stats")]
    [SerializeField] private GunStats revolverStats;
    [SerializeField] private GunStats shotgunStats;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("UI (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI shotgunAmmoText;

    private float revolverCooldown;
    private float shotgunCooldown;
    private int shotgunAmmo;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        shotgunAmmo = shotgunStats != null ? shotgunStats.startingAmmo : 0;
    }

    private void Update()
    {
        revolverCooldown -= Time.deltaTime;
        shotgunCooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            TryFireRevolver();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryFireShotgun();
        }

        UpdateAmmoUI();
    }

    private void TryFireRevolver()
    {
        if (revolverCooldown > 0 || revolverStats == null) return;

        revolverCooldown = 1f / revolverStats.fireRate;
        Vector2 direction = GetMouseDirection();
        SpawnBullet(revolverStats, direction, isRevolver: true);
    }

    private void TryFireShotgun()
    {
        if (shotgunCooldown > 0 || shotgunStats == null) return;
        if (shotgunAmmo <= 0) return;

        shotgunCooldown = 1f / shotgunStats.fireRate;
        shotgunAmmo--;

        Vector2 direction = GetMouseDirection();

        for (int i = 0; i < shotgunStats.pelletsPerShot; i++)
        {
            float angle = Random.Range(-shotgunStats.spreadAngle * 0.5f, shotgunStats.spreadAngle * 0.5f);
            Vector2 spread = Quaternion.Euler(0, 0, angle) * direction;
            SpawnBullet(shotgunStats, spread, isRevolver: false);
        }

        // Propel player in the opposite direction of the shot
        if (playerRb != null && shotgunStats.playerRecoilForce > 0)
        {
            playerRb.AddForce(-direction.normalized * shotgunStats.playerRecoilForce, ForceMode2D.Impulse);
        }
    }

    private void SpawnBullet(GunStats stats, Vector2 direction, bool isRevolver)
    {
        if (stats.bulletPrefab == null) return;

        GameObject bulletGo = Instantiate(stats.bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGo.GetComponent<Bullet>();

        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletGo.transform.rotation = Quaternion.Euler(0, 0, rotation);

        bullet.Init(direction, stats.bulletSpeed, stats.bulletRange, stats.damage, isRevolver, this);
    }

    private Vector2 GetMouseDirection()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return (mouseWorld - (Vector2)firePoint.position).normalized;
    }

    public void OnRevolverKill()
    {
        shotgunAmmo++;
    }

    private void UpdateAmmoUI()
    {
        if (shotgunAmmoText != null)
        {
            shotgunAmmoText.text = $"Shells: {shotgunAmmo}";
        }
    }

    public int ShotgunAmmo => shotgunAmmo;
}
