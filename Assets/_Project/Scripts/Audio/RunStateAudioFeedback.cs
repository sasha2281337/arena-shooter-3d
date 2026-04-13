using ArenaShooter.Combat;
using ArenaShooter.Waves;
using UnityEngine;

namespace ArenaShooter.Audio
{
    public class RunStateAudioFeedback : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private AudioClip[] gameOverClips;
        [SerializeField] private AudioClip[] victoryClips;
        [SerializeField, Range(0f, 1f)] private float gameOverVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float victoryVolume = 0.9f;

        private void Awake()
        {
            if (playerHealth == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                playerHealth = playerObject != null ? playerObject.GetComponent<Health>() : null;
            }

            if (waveDirector == null)
            {
                waveDirector = FindFirstObjectByType<WaveDirector>();
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDied;
            }

            if (waveDirector != null)
            {
                waveDirector.RunCompleted += HandleRunCompleted;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDied;
            }

            if (waveDirector != null)
            {
                waveDirector.RunCompleted -= HandleRunCompleted;
            }
        }

        private void HandlePlayerDied()
        {
            AudioFeedbackUtility.PlayDetached(
                AudioFeedbackUtility.PickRandomClip(gameOverClips),
                Vector3.zero,
                gameOverVolume,
                true);
        }

        private void HandleRunCompleted()
        {
            AudioFeedbackUtility.PlayDetached(
                AudioFeedbackUtility.PickRandomClip(victoryClips),
                Vector3.zero,
                victoryVolume,
                true);
        }
    }
}
