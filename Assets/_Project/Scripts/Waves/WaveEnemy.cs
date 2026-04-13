using System;
using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Waves
{
    [RequireComponent(typeof(Health))]
    public class WaveEnemy : MonoBehaviour
    {
        public event Action<WaveEnemy> Died;

        private Health health;
        private bool deathReported;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += HandleDied;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void HandleDied()
        {
            if (deathReported)
            {
                return;
            }

            deathReported = true;
            Died?.Invoke(this);
        }
    }
}
