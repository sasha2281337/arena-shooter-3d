using UnityEngine;

namespace ArenaShooter.Weapons
{
    [CreateAssetMenu(fileName = "WD_NewWeapon", menuName = "Arena Shooter/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string weaponName = "Rifle";

        [Header("Damage")]
        [SerializeField, Min(1f)] private float damage = 20f;
        [SerializeField, Min(1)] private int projectilesPerShot = 1;
        [SerializeField, Range(0f, 25f)] private float spreadAngle = 0f;
        [SerializeField, Min(1f)] private float range = 40f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Fire")]
        [SerializeField, Min(0.1f)] private float fireRate = 5f;
        [SerializeField] private bool automatic = true;

        [Header("Ammo")]
        [SerializeField, Min(1)] private int magazineSize = 30;
        [SerializeField, Min(0.1f)] private float reloadDuration = 1.2f;
        [SerializeField] private bool infiniteReserveAmmo = true;
        [SerializeField, Min(0)] private int startingReserveAmmo = 90;

        public string WeaponName => weaponName;
        public float Damage => damage;
        public int ProjectilesPerShot => projectilesPerShot;
        public float SpreadAngle => spreadAngle;
        public float Range => range;
        public LayerMask HitMask => hitMask;
        public float FireRate => fireRate;
        public bool Automatic => automatic;
        public int MagazineSize => magazineSize;
        public float ReloadDuration => reloadDuration;
        public bool InfiniteReserveAmmo => infiniteReserveAmmo;
        public int StartingReserveAmmo => startingReserveAmmo;
        public float FireInterval => 1f / fireRate;

        private void OnValidate()
        {
            damage = Mathf.Max(1f, damage);
            projectilesPerShot = Mathf.Max(1, projectilesPerShot);
            fireRate = Mathf.Max(0.1f, fireRate);
            range = Mathf.Max(1f, range);
            magazineSize = Mathf.Max(1, magazineSize);
            reloadDuration = Mathf.Max(0.1f, reloadDuration);
            startingReserveAmmo = Mathf.Max(0, startingReserveAmmo);
        }
    }
}
