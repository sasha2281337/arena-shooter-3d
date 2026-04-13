using ArenaShooter.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaShooter.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health targetHealth;
        [SerializeField] private Image fillImage;
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.9f, 0.35f);
        [SerializeField] private Color criticalColor = new Color(0.95f, 0.2f, 0.2f);

        private bool isSubscribed;

        private void Awake()
        {
            TryResolveHealth();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetTarget(Health newTarget)
        {
            if (targetHealth == newTarget)
            {
                return;
            }

            Unsubscribe();
            targetHealth = newTarget;
            Subscribe();
            Refresh();
        }

        private void TryResolveHealth()
        {
            if (targetHealth != null || !autoFindPlayer)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                targetHealth = playerObject.GetComponent<Health>();
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            TryResolveHealth();

            if (targetHealth == null)
            {
                return;
            }

            targetHealth.HealthChanged += HandleHealthChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || targetHealth == null)
            {
                return;
            }

            targetHealth.HealthChanged -= HandleHealthChanged;
            isSubscribed = false;
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateVisuals(currentHealth, maxHealth);
        }

        private void Refresh()
        {
            if (fillImage == null)
            {
                return;
            }

            TryResolveHealth();

            if (targetHealth == null)
            {
                fillImage.fillAmount = 1f;
                fillImage.color = healthyColor;
                return;
            }

            UpdateVisuals(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        private void UpdateVisuals(float currentHealth, float maxHealth)
        {
            if (fillImage == null)
            {
                return;
            }

            float normalized = maxHealth <= 0f ? 0f : currentHealth / maxHealth;
            normalized = Mathf.Clamp01(normalized);

            fillImage.fillAmount = normalized;
            fillImage.color = Color.Lerp(criticalColor, healthyColor, normalized);
        }
    }
}
