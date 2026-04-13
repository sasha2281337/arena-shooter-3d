using ArenaShooter.Player;
using ArenaShooter.Weapons;
using UnityEngine;

namespace ArenaShooter.Audio
{
    public class WeaponAudioFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] rifleShotClips;
        [SerializeField] private AudioClip[] shotgunShotClips;
        [SerializeField] private AudioClip[] reloadStartClips;
        [SerializeField] private AudioClip[] reloadCompleteClips;
        [SerializeField] private AudioClip[] hitConfirmClips;
        [SerializeField, Range(0f, 1f)] private float shotVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float reloadVolume = 0.65f;
        [SerializeField, Range(0f, 1f)] private float hitVolume = 0.4f;

        private bool wasReloading;

        private void Awake()
        {
            if (weaponController == null)
            {
                weaponController = GetComponent<PlayerWeaponController>();
            }

            EnsureAudioSource();
        }

        private void OnEnable()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.ShotFired += HandleShotFired;
            weaponController.ReloadStateChanged += HandleReloadStateChanged;
            weaponController.HitConfirmed += HandleHitConfirmed;
            wasReloading = weaponController.IsReloading;
        }

        private void OnDisable()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.ShotFired -= HandleShotFired;
            weaponController.ReloadStateChanged -= HandleReloadStateChanged;
            weaponController.HitConfirmed -= HandleHitConfirmed;
        }

        private void HandleShotFired(WeaponData weaponData)
        {
            AudioClip clip = IsShotgun(weaponData)
                ? AudioFeedbackUtility.PickRandomClip(shotgunShotClips)
                : AudioFeedbackUtility.PickRandomClip(rifleShotClips);

            PlayClip(clip, shotVolume);
        }

        private void HandleReloadStateChanged(bool isReloading)
        {
            if (!wasReloading && isReloading)
            {
                PlayClip(AudioFeedbackUtility.PickRandomClip(reloadStartClips), reloadVolume);
            }
            else if (wasReloading && !isReloading)
            {
                PlayClip(AudioFeedbackUtility.PickRandomClip(reloadCompleteClips), reloadVolume);
            }

            wasReloading = isReloading;
        }

        private void HandleHitConfirmed()
        {
            PlayClip(AudioFeedbackUtility.PickRandomClip(hitConfirmClips), hitVolume);
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

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }

        private static bool IsShotgun(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                return false;
            }

            return weaponData.ProjectilesPerShot > 1 ||
                   (!string.IsNullOrWhiteSpace(weaponData.WeaponName) &&
                    weaponData.WeaponName.ToLowerInvariant().Contains("shotgun"));
        }
    }
}
