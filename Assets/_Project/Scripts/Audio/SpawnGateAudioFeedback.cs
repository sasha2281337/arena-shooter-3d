using ArenaShooter.Waves;
using UnityEngine;

namespace ArenaShooter.Audio
{
    public class SpawnGateAudioFeedback : MonoBehaviour
    {
        [SerializeField] private SpawnGateVisual spawnGateVisual;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] openClips;
        [SerializeField] private AudioClip[] closeClips;
        [SerializeField, Range(0f, 1f)] private float openVolume = 0.75f;
        [SerializeField, Range(0f, 1f)] private float closeVolume = 0.65f;

        private void Awake()
        {
            if (spawnGateVisual == null)
            {
                spawnGateVisual = GetComponent<SpawnGateVisual>();
            }

            EnsureAudioSource();
        }

        private void OnEnable()
        {
            if (spawnGateVisual == null)
            {
                return;
            }

            spawnGateVisual.OpenedForSpawn += HandleOpenedForSpawn;
            spawnGateVisual.ClosedAfterSpawn += HandleClosedAfterSpawn;
        }

        private void OnDisable()
        {
            if (spawnGateVisual == null)
            {
                return;
            }

            spawnGateVisual.OpenedForSpawn -= HandleOpenedForSpawn;
            spawnGateVisual.ClosedAfterSpawn -= HandleClosedAfterSpawn;
        }

        private void HandleOpenedForSpawn()
        {
            PlayClip(AudioFeedbackUtility.PickRandomClip(openClips), openVolume);
        }

        private void HandleClosedAfterSpawn()
        {
            PlayClip(AudioFeedbackUtility.PickRandomClip(closeClips), closeVolume);
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
            audioSource.spatialBlend = 1f;
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }
    }
}
