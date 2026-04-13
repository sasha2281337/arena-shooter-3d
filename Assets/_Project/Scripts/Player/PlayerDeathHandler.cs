using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] componentsToDisable;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void HandleDeath()
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                if (componentsToDisable[i] != null)
                {
                    componentsToDisable[i].enabled = false;
                }
            }

            Debug.Log("Player died.");
        }
    }
}
