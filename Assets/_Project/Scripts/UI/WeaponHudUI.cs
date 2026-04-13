using ArenaShooter.Player;
using ArenaShooter.Weapons;
using TMPro;
using UnityEngine;

namespace ArenaShooter.UI
{
    public class WeaponHudUI : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private TMP_Text weaponText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text reloadText;

        private string currentWeaponName = "Weapon";
        private int currentAmmo;
        private int reserveAmmo;
        private bool hasInfiniteReserveAmmo;

        private void Awake()
        {
            ClearReloadText();
        }

        private void OnEnable()
        {
            if (weaponController == null)
            {
                weaponController = FindFirstObjectByType<PlayerWeaponController>();
            }

            if (weaponController == null)
            {
                return;
            }

            weaponController.WeaponChanged += HandleWeaponChanged;
            weaponController.AmmoChanged += HandleAmmoChanged;
            weaponController.ReloadStateChanged += HandleReloadStateChanged;

            HandleWeaponChanged(weaponController.CurrentWeapon);
            HandleAmmoChanged(weaponController.CurrentAmmo, weaponController.ReserveAmmo, weaponController.CurrentWeapon != null && weaponController.CurrentWeapon.InfiniteReserveAmmo);
            HandleReloadStateChanged(weaponController.IsReloading);
        }

        private void OnDisable()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.WeaponChanged -= HandleWeaponChanged;
            weaponController.AmmoChanged -= HandleAmmoChanged;
            weaponController.ReloadStateChanged -= HandleReloadStateChanged;
        }

        private void HandleWeaponChanged(WeaponData weaponData)
        {
            currentWeaponName = weaponData != null ? weaponData.WeaponName : "No Weapon";

            if (weaponText != null)
            {
                weaponText.text = currentWeaponName;
            }
        }

        private void HandleAmmoChanged(int newCurrentAmmo, int newReserveAmmo, bool newHasInfiniteReserveAmmo)
        {
            currentAmmo = newCurrentAmmo;
            reserveAmmo = newReserveAmmo;
            hasInfiniteReserveAmmo = newHasInfiniteReserveAmmo;
            RefreshAmmoText();
        }

        private void HandleReloadStateChanged(bool isReloading)
        {
            if (reloadText != null)
            {
                reloadText.text = isReloading ? "Reloading..." : string.Empty;
            }
        }

        private void RefreshAmmoText()
        {
            if (ammoText == null)
            {
                return;
            }

            string reserveText = hasInfiniteReserveAmmo ? "INF" : reserveAmmo.ToString();
            ammoText.text = $"Ammo: {currentAmmo} / {reserveText}";
        }

        private void ClearReloadText()
        {
            if (reloadText != null)
            {
                reloadText.text = string.Empty;
            }
        }
    }
}
