using ArenaShooter.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaShooter.Feedback
{
    public class HitMarkerUI : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private Image hitMarkerImage;
        [SerializeField] private bool autoFindWeaponController = true;
        [SerializeField, Range(0f, 1f)] private float visibleAlpha = 0.9f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.12f;

        private float currentAlpha;

        private void Awake()
        {
            ResolveWeaponController();
            SetAlpha(0f);
        }

        private void OnEnable()
        {
            ResolveWeaponController();

            if (weaponController != null)
            {
                weaponController.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void OnDisable()
        {
            if (weaponController != null)
            {
                weaponController.HitConfirmed -= HandleHitConfirmed;
            }
        }

        private void Update()
        {
            if (hitMarkerImage == null || currentAlpha <= 0f)
            {
                return;
            }

            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.unscaledDeltaTime / fadeDuration);
            SetAlpha(currentAlpha);
        }

        private void HandleHitConfirmed()
        {
            currentAlpha = visibleAlpha;
            SetAlpha(currentAlpha);
        }

        private void ResolveWeaponController()
        {
            if (weaponController != null || !autoFindWeaponController)
            {
                return;
            }

            weaponController = FindFirstObjectByType<PlayerWeaponController>();
        }

        private void SetAlpha(float alpha)
        {
            if (hitMarkerImage == null)
            {
                return;
            }

            Color color = hitMarkerImage.color;
            color.a = alpha;
            hitMarkerImage.color = color;
        }
    }
}
