using System;
using UnityEngine;

namespace ArenaShooter.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField, Min(0f)] private float destroyDelay = 0f;
        [SerializeField, Min(0f)] private float damageCooldownDuration = 0f;
        [SerializeField] private bool logIncomingDamage = false;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0f;

        private float nextDamageTime;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            nextDamageTime = 0f;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void IncreaseMaxHealth(float amount, bool healByAddedAmount)
        {
            if (amount <= 0f)
            {
                return;
            }

            maxHealth += amount;

            if (healByAddedAmount)
            {
                CurrentHealth += amount;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, null, null);
        }

        public void TakeDamage(float damage, UnityEngine.Object source, string sourceLabel)
        {
            if (!IsAlive || damage <= 0f)
            {
                return;
            }

            if (damageCooldownDuration > 0f && Time.time < nextDamageTime)
            {
                return;
            }

            nextDamageTime = Time.time + damageCooldownDuration;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (ShouldLogIncomingDamage())
            {
                string resolvedSource = ResolveSourceLabel(source, sourceLabel);
                Debug.Log($"{name} took {damage:0.#} damage from {resolvedSource} (HP {CurrentHealth:0.#}/{MaxHealth:0.#})", source != null ? source : this);
            }

            if (CurrentHealth > 0f)
            {
                return;
            }

            Died?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        private bool ShouldLogIncomingDamage()
        {
            return logIncomingDamage || CompareTag("Player");
        }

        private static string ResolveSourceLabel(UnityEngine.Object source, string sourceLabel)
        {
            if (!string.IsNullOrWhiteSpace(sourceLabel))
            {
                return sourceLabel;
            }

            return source != null ? source.name : "Unknown source";
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            destroyDelay = Mathf.Max(0f, destroyDelay);
            damageCooldownDuration = Mathf.Max(0f, damageCooldownDuration);
        }
    }
}
