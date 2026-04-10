using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStats", menuName = "Weapons/Gun Stats")]
public class GunStats : ScriptableObject
{
    [Header("Identity")]
    public string gunName;

    [Header("Firing")]
    public float fireRate = 1f;
    public int pelletsPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float bulletRange = 30f;
    public int damage = 1;

    [Header("Recoil")]
    public float playerRecoilForce = 0f;

    [Header("Ammo")]
    public bool infiniteAmmo = true;
    public int startingAmmo = 0;
}
