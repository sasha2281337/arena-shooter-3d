using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Audio
{
    public class HealthAudioFeedback : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] damageClips;
        [SerializeField] private AudioClip[] deathClips;
        [SerializeField] private bool playAs2D = false;
        [SerializeField, Range(0f, 1f)] private float damageVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float deathVolume = 0.8f;

        private float lastHealth;
        private bool hasHealthSnapshot;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            EnsureAudioSource();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                return;
            }

            health.HealthChanged += HandleHealthChanged;
            health.Died += HandleDied;
            lastHealth = health.CurrentHealth;
            hasHealthSnapshot = true;
        }

        private void OnDisable()
        {
            if (health == null)
            {
                return;
            }

            health.HealthChanged -= HandleHealthChanged;
            health.Died -= HandleDied;
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (!hasHealthSnapshot)
            {
                lastHealth = currentHealth;
                hasHealthSnapshot = true;
                return;
            }

            if (currentHealth < lastHealth && currentHealth > 0f)
            {
                PlayLocal(AudioFeedbackUtility.PickRandomClip(damageClips), damageVolume);
            }

            lastHealth = currentHealth;
        }

        private void HandleDied()
        {
            AudioClip clip = AudioFeedbackUtility.PickRandomClip(deathClips);

            if (clip == null)
            {
                return;
            }

            AudioFeedbackUtility.PlayDetached(clip, transform.position, deathVolume, playAs2D);
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = playAs2D ? 0f : 1f;
        }

        private void PlayLocal(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }
    }
}
