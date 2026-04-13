using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Feedback
{
    [RequireComponent(typeof(Health))]
    public class DamageFlashFeedback : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField, Min(0.01f)] private float flashDuration = 0.12f;
        [SerializeField] private bool autoFindChildRenderers = true;

        private float lastHealth;
        private float flashTimer;
        private Color[] originalColors;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if ((targetRenderers == null || targetRenderers.Length == 0) && autoFindChildRenderers)
            {
                targetRenderers = ResolveTargetRenderers();
            }

            CacheOriginalColors();
            lastHealth = health != null ? health.CurrentHealth : 0f;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= HandleHealthChanged;
            }

            RestoreOriginalColors();
        }

        private void Update()
        {
            if (flashTimer <= 0f || targetRenderers == null)
            {
                return;
            }

            flashTimer -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - (flashTimer / flashDuration));

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i] == null)
                {
                    continue;
                }

                targetRenderers[i].material.color = Color.Lerp(flashColor, originalColors[i], t);
            }

            if (flashTimer <= 0f)
            {
                RestoreOriginalColors();
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (currentHealth < lastHealth)
            {
                flashTimer = flashDuration;

                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    if (targetRenderers[i] != null)
                    {
                        targetRenderers[i].material.color = flashColor;
                    }
                }
            }

            lastHealth = currentHealth;
        }

        private void CacheOriginalColors()
        {
            if (targetRenderers == null)
            {
                originalColors = new Color[0];
                return;
            }

            originalColors = new Color[targetRenderers.Length];

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                originalColors[i] = targetRenderers[i] != null ? targetRenderers[i].material.color : Color.white;
            }
        }

        private void RestoreOriginalColors()
        {
            if (targetRenderers == null || originalColors == null)
            {
                return;
            }

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i] != null)
                {
                    targetRenderers[i].material.color = originalColors[i];
                }
            }
        }

        private Renderer[] ResolveTargetRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            System.Collections.Generic.List<Renderer> resolved = new System.Collections.Generic.List<Renderer>(renderers.Length);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                resolved.Add(renderer);
            }

            return resolved.ToArray();
        }
    }
}
