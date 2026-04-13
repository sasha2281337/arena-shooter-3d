using System;
using ArenaShooter.Combat;
using ArenaShooter.Weapons;
using ArenaShooter.Upgrades;
using UnityEngine;

namespace ArenaShooter.Player
{
    public class PlayerWeaponController : MonoBehaviour
    {
        private sealed class WeaponRuntimeState
        {
            public WeaponRuntimeState(WeaponData weapon)
            {
                Weapon = weapon;
                CurrentAmmo = weapon.MagazineSize;
                ReserveAmmo = weapon.StartingReserveAmmo;
            }

            public WeaponData Weapon { get; }
            public int CurrentAmmo { get; set; }
            public int ReserveAmmo { get; set; }
        }

        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerAim playerAim;
        [SerializeField] private PlayerUpgradeController upgradeController;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("Weapons")]
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private WeaponData[] equippedWeapons;

        [Header("Debug")]
        [SerializeField] private bool drawDebugShot = true;

        public event Action<WeaponData> WeaponChanged;
        public event Action<int, int, bool> AmmoChanged;
        public event Action<bool> ReloadStateChanged;
        public event Action HitConfirmed;
        public event Action<WeaponData> ShotFired;

        public WeaponData CurrentWeapon => currentWeaponState?.Weapon;
        public int CurrentAmmo => currentWeaponState?.CurrentAmmo ?? 0;
        public int ReserveAmmo => currentWeaponState?.ReserveAmmo ?? 0;
        public bool IsReloading { get; private set; }

        private WeaponRuntimeState[] weaponStates;
        private WeaponRuntimeState currentWeaponState;
        private int currentWeaponIndex;
        private float nextShotTime;
        private float reloadEndTime;
        private bool queuedSingleShot;
        private Health ownHealth;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (playerAim == null)
            {
                playerAim = GetComponent<PlayerAim>();
            }

            if (upgradeController == null)
            {
                upgradeController = GetComponent<PlayerUpgradeController>();
            }

            ownHealth = GetComponent<Health>();
            BuildRuntimeWeaponStates();
            EquipInitialWeapon();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                return;
            }

            inputReader.FirePressed += QueueSingleShot;
            inputReader.ReloadPressed += StartReload;
            inputReader.NextWeaponPressed += EquipNextWeapon;
            inputReader.PreviousWeaponPressed += EquipPreviousWeapon;
            inputReader.WeaponSlotPressed += TryEquipWeaponSlot;
        }

        private void OnDisable()
        {
            if (inputReader == null)
            {
                return;
            }

            inputReader.FirePressed -= QueueSingleShot;
            inputReader.ReloadPressed -= StartReload;
            inputReader.NextWeaponPressed -= EquipNextWeapon;
            inputReader.PreviousWeaponPressed -= EquipPreviousWeapon;
            inputReader.WeaponSlotPressed -= TryEquipWeaponSlot;
        }

        private void Update()
        {
            if (CurrentWeapon == null || inputReader == null || playerAim == null)
            {
                return;
            }

            UpdateReload();

            if (IsReloading)
            {
                queuedSingleShot = false;
                return;
            }

            if (CurrentWeapon.Automatic)
            {
                if (inputReader.IsFireHeld)
                {
                    TryFire();
                }

                return;
            }

            if (queuedSingleShot)
            {
                queuedSingleShot = false;
                TryFire();
            }
        }

        public void StartReload()
        {
            if (CurrentWeapon == null || IsReloading || CurrentAmmo >= CurrentWeapon.MagazineSize)
            {
                return;
            }

            if (!CurrentWeapon.InfiniteReserveAmmo && ReserveAmmo <= 0)
            {
                return;
            }

            IsReloading = true;
            reloadEndTime = Time.time + CurrentWeapon.ReloadDuration;
            ReloadStateChanged?.Invoke(IsReloading);
            RaiseAmmoChanged();
        }

        public void EquipNextWeapon()
        {
            if (weaponStates == null || weaponStates.Length <= 1)
            {
                return;
            }

            TryEquipWeaponSlot((currentWeaponIndex + 1) % weaponStates.Length);
        }

        public void EquipPreviousWeapon()
        {
            if (weaponStates == null || weaponStates.Length <= 1)
            {
                return;
            }

            int nextIndex = currentWeaponIndex - 1;

            if (nextIndex < 0)
            {
                nextIndex = weaponStates.Length - 1;
            }

            TryEquipWeaponSlot(nextIndex);
        }

        public void TryEquipWeaponSlot(int slotIndex)
        {
            if (weaponStates == null || slotIndex < 0 || slotIndex >= weaponStates.Length)
            {
                return;
            }

            WeaponRuntimeState nextState = weaponStates[slotIndex];

            if (nextState == null || nextState == currentWeaponState)
            {
                return;
            }

            currentWeaponIndex = slotIndex;
            EquipRuntimeState(nextState);
        }

        private void BuildRuntimeWeaponStates()
        {
            if (equippedWeapons == null || equippedWeapons.Length == 0)
            {
                weaponStates = startingWeapon == null ? Array.Empty<WeaponRuntimeState>() : new[] { new WeaponRuntimeState(startingWeapon) };
                return;
            }

            weaponStates = new WeaponRuntimeState[equippedWeapons.Length];

            for (int i = 0; i < equippedWeapons.Length; i++)
            {
                if (equippedWeapons[i] != null)
                {
                    weaponStates[i] = new WeaponRuntimeState(equippedWeapons[i]);
                }
            }
        }

        private void EquipInitialWeapon()
        {
            if (weaponStates == null || weaponStates.Length == 0)
            {
                return;
            }

            for (int i = 0; i < weaponStates.Length; i++)
            {
                if (weaponStates[i] == null)
                {
                    continue;
                }

                currentWeaponIndex = i;
                EquipRuntimeState(weaponStates[i]);
                return;
            }
        }

        private void EquipRuntimeState(WeaponRuntimeState weaponState)
        {
            currentWeaponState = weaponState;
            IsReloading = false;
            queuedSingleShot = false;
            nextShotTime = 0f;

            WeaponChanged?.Invoke(CurrentWeapon);
            ReloadStateChanged?.Invoke(IsReloading);
            RaiseAmmoChanged();
        }

        private void UpdateReload()
        {
            if (!IsReloading || Time.time < reloadEndTime)
            {
                return;
            }

            CompleteReload();
        }

        private void CompleteReload()
        {
            int missingAmmo = CurrentWeapon.MagazineSize - CurrentAmmo;

            if (CurrentWeapon.InfiniteReserveAmmo)
            {
                currentWeaponState.CurrentAmmo = CurrentWeapon.MagazineSize;
            }
            else
            {
                int ammoToLoad = Mathf.Min(missingAmmo, ReserveAmmo);
                currentWeaponState.CurrentAmmo += ammoToLoad;
                currentWeaponState.ReserveAmmo -= ammoToLoad;
            }

            IsReloading = false;
            ReloadStateChanged?.Invoke(IsReloading);
            RaiseAmmoChanged();
        }

        private void TryFire()
        {
            if (Time.time < nextShotTime)
            {
                return;
            }

            if (CurrentAmmo <= 0)
            {
                StartReload();
                return;
            }

            nextShotTime = Time.time + GetModifiedFireInterval();
            currentWeaponState.CurrentAmmo--;
            RaiseAmmoChanged();
            Fire();

            if (CurrentAmmo <= 0)
            {
                StartReload();
            }
        }

        private void Fire()
        {
            Vector3 shotOrigin = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up;
            Vector3 baseDirection = playerAim.CurrentAimPoint - shotOrigin;

            if (baseDirection.sqrMagnitude < 0.001f)
            {
                baseDirection = transform.forward;
            }

            baseDirection.Normalize();

            for (int i = 0; i < CurrentWeapon.ProjectilesPerShot; i++)
            {
                Vector3 shotDirection = ApplySpread(baseDirection);
                FireProjectileRay(shotOrigin, shotDirection);
            }

            ShotFired?.Invoke(CurrentWeapon);

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
        }

        private Vector3 ApplySpread(Vector3 baseDirection)
        {
            if (CurrentWeapon.SpreadAngle <= 0f)
            {
                return baseDirection;
            }

            float yaw = UnityEngine.Random.Range(-CurrentWeapon.SpreadAngle, CurrentWeapon.SpreadAngle);
            Quaternion spreadRotation = Quaternion.AngleAxis(yaw, Vector3.up);
            return spreadRotation * baseDirection;
        }

        private void FireProjectileRay(Vector3 shotOrigin, Vector3 shotDirection)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                shotOrigin,
                shotDirection,
                CurrentWeapon.Range,
                CurrentWeapon.HitMask,
                QueryTriggerInteraction.Ignore);

            if (hits.Length > 0)
            {
                Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];

                    if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    IDamageable damageable =
                        hit.collider.GetComponent<IDamageable>() ??
                        hit.collider.GetComponentInParent<IDamageable>();

                    if (damageable == null)
                    {
                        break;
                    }

                    Health targetHealth =
                        hit.collider.GetComponent<Health>() ??
                        hit.collider.GetComponentInParent<Health>();

                    if (targetHealth == ownHealth)
                    {
                        continue;
                    }

                    if (targetHealth != null)
                    {
                        targetHealth.TakeDamage(GetModifiedDamage(), this, $"{name} {CurrentWeapon.WeaponName}");
                        HitConfirmed?.Invoke();
                    }

                    break;
                }
            }

            if (drawDebugShot)
            {
                Debug.DrawRay(shotOrigin, shotDirection * CurrentWeapon.Range, Color.cyan, 0.2f);
            }
        }

        private float GetModifiedFireInterval()
        {
            float fireRateMultiplier = upgradeController != null ? upgradeController.FireRateMultiplier : 1f;
            return CurrentWeapon.FireInterval / Mathf.Max(0.1f, fireRateMultiplier);
        }

        private float GetModifiedDamage()
        {
            float damageMultiplier = upgradeController != null ? upgradeController.WeaponDamageMultiplier : 1f;
            return CurrentWeapon.Damage * damageMultiplier;
        }

        private void RaiseAmmoChanged()
        {
            if (CurrentWeapon == null)
            {
                return;
            }

            AmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo, CurrentWeapon.InfiniteReserveAmmo);
        }

        private void QueueSingleShot()
        {
            queuedSingleShot = true;
        }
    }
}
