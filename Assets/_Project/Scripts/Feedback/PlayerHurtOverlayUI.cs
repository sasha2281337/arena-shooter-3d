using ArenaShooter.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaShooter.Feedback
{
    [RequireComponent(typeof(Image))]
    public class PlayerHurtOverlayUI : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private Image overlayImage;
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.35f;
        [SerializeField, Min(0.01f)] private float fadeSpeed = 2.5f;

        private float lastHealth;
        private float currentAlpha;

        private void Reset()
        {
            overlayImage = GetComponent<Image>();
        }

        private void Awake()
        {
            ResolveOverlayImage();
            ResolvePlayerHealth();
            HideOverlayImmediately();
            lastHealth = playerHealth != null ? playerHealth.CurrentHealth : 0f;
        }

        private void OnEnable()
        {
            ResolveOverlayImage();
            ResolvePlayerHealth();
            HideOverlayImmediately();

            if (playerHealth != null)
            {
                playerHealth.HealthChanged += HandleHealthChanged;
                lastHealth = playerHealth.CurrentHealth;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= HandleHealthChanged;
            }
        }

        private void Update()
        {
            if (overlayImage == null || currentAlpha <= 0f)
            {
                return;
            }

            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, fadeSpeed * Time.unscaledDeltaTime);
            SetOverlayAlpha(currentAlpha);
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (currentHealth < lastHealth)
            {
                float damageFraction = maxHealth > 0f ? Mathf.Clamp01((lastHealth - currentHealth) / maxHealth) : 0.1f;
                currentAlpha = Mathf.Clamp(maxAlpha * (0.6f + damageFraction * 1.8f), 0f, maxAlpha);
                SetOverlayAlpha(currentAlpha);
            }

            lastHealth = currentHealth;
        }

        private void ResolveOverlayImage()
        {
            if (overlayImage == null)
            {
                overlayImage = GetComponent<Image>();
            }
        }

        private void ResolvePlayerHealth()
        {
            if (playerHealth != null || !autoFindPlayer)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponent<Health>();
            }
        }

        private void HideOverlayImmediately()
        {
            currentAlpha = 0f;
            SetOverlayAlpha(0f);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (overlayImage == null)
            {
                return;
            }

            Color color = overlayImage.color;
            color.a = alpha;
            overlayImage.color = color;
        }
    }
}
