using System;
using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Upgrades
{
    [RequireComponent(typeof(Health))]
    public class PlayerUpgradeController : MonoBehaviour
    {
        [SerializeField] private Health health;

        public event Action UpgradeStatsChanged;
        public event Action<UpgradeData> UpgradeApplied;

        public float MoveSpeedMultiplier { get; private set; } = 1f;
        public float WeaponDamageMultiplier { get; private set; } = 1f;
        public float FireRateMultiplier { get; private set; } = 1f;

        private void Reset()
        {
            health = GetComponent<Health>();
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                return;
            }

            switch (upgrade.Kind)
            {
                case UpgradeKind.Heal:
                    health.Heal(upgrade.Value);
                    break;
                case UpgradeKind.MaxHealth:
                    health.IncreaseMaxHealth(upgrade.Value, healByAddedAmount: true);
                    break;
                case UpgradeKind.MoveSpeedPercent:
                    MoveSpeedMultiplier += PercentToMultiplierBonus(upgrade.Value);
                    break;
                case UpgradeKind.WeaponDamagePercent:
                    WeaponDamageMultiplier += PercentToMultiplierBonus(upgrade.Value);
                    break;
                case UpgradeKind.FireRatePercent:
                    FireRateMultiplier += PercentToMultiplierBonus(upgrade.Value);
                    break;
            }

            Debug.Log($"Upgrade applied: {upgrade.Title}", this);
            UpgradeApplied?.Invoke(upgrade);
            UpgradeStatsChanged?.Invoke();
        }

        private static float PercentToMultiplierBonus(float percent)
        {
            return percent / 100f;
        }
    }
}
