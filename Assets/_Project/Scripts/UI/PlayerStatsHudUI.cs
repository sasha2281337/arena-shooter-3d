using ArenaShooter.Upgrades;
using TMPro;
using UnityEngine;

namespace ArenaShooter.UI
{
    public class PlayerStatsHudUI : MonoBehaviour
    {
        [SerializeField] private PlayerUpgradeController upgradeController;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text lastUpgradeText;

        private void OnEnable()
        {
            if (upgradeController == null)
            {
                upgradeController = FindFirstObjectByType<PlayerUpgradeController>();
            }

            if (upgradeController == null)
            {
                return;
            }

            upgradeController.UpgradeStatsChanged += RefreshStats;
            upgradeController.UpgradeApplied += HandleUpgradeApplied;
            RefreshStats();
        }

        private void OnDisable()
        {
            if (upgradeController == null)
            {
                return;
            }

            upgradeController.UpgradeStatsChanged -= RefreshStats;
            upgradeController.UpgradeApplied -= HandleUpgradeApplied;
        }

        private void RefreshStats()
        {
            if (statsText == null || upgradeController == null)
            {
                return;
            }

            statsText.text =
                $"SPD x{upgradeController.MoveSpeedMultiplier:0.00}\n" +
                $"DMG x{upgradeController.WeaponDamageMultiplier:0.00}\n" +
                $"ROF x{upgradeController.FireRateMultiplier:0.00}";
        }

        private void HandleUpgradeApplied(UpgradeData upgrade)
        {
            if (lastUpgradeText != null && upgrade != null)
            {
                lastUpgradeText.text = $"Upgrade: {upgrade.Title}";
            }
        }
    }
}
