using UnityEngine;
using UnityEngine.UI;

namespace ArenaShooter.Audio
{
    public class UIButtonAudioFeedback : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] clickClips;
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.75f;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            EnsureAudioSource();
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            AudioClip clip = AudioFeedbackUtility.PickRandomClip(clickClips);

            if (clip == null)
            {
                return;
            }

            if (audioSource != null && audioSource.isActiveAndEnabled && gameObject.activeInHierarchy)
            {
                audioSource.PlayOneShot(clip, clickVolume);
                return;
            }

            AudioFeedbackUtility.PlayDetached(clip, Vector3.zero, clickVolume, true);
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
            audioSource.spatialBlend = 0f;
        }
    }
}
