using System.Collections;
using ArenaShooter.Waves;
using TMPro;
using UnityEngine;

namespace ArenaShooter.UI
{
    public class WaveHudUI : MonoBehaviour
    {
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text enemiesText;
        [SerializeField] private TMP_Text waveBannerText;
        [SerializeField, Min(0.1f)] private float bannerVisibleDuration = 1.8f;
        [SerializeField, Min(0.1f)] private float bannerFadeDuration = 0.45f;

        private Coroutine bannerRoutine;
        private Color bannerBaseColor = Color.white;

        private void Awake()
        {
            if (waveBannerText != null)
            {
                bannerBaseColor = waveBannerText.color;
                SetBannerAlpha(0f);
                waveBannerText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (waveDirector == null)
            {
                waveDirector = FindFirstObjectByType<WaveDirector>();
            }

            if (waveDirector == null)
            {
                return;
            }

            waveDirector.WaveStarted += HandleWaveStarted;
            waveDirector.BossWaveStarted += HandleBossWaveStarted;
            waveDirector.AliveEnemyCountChanged += HandleAliveEnemyCountChanged;

            HandleWaveStarted(waveDirector.CurrentWave);
            HandleAliveEnemyCountChanged(waveDirector.AliveEnemyCount);
        }

        private void OnDisable()
        {
            if (waveDirector != null)
            {
                waveDirector.WaveStarted -= HandleWaveStarted;
                waveDirector.BossWaveStarted -= HandleBossWaveStarted;
                waveDirector.AliveEnemyCountChanged -= HandleAliveEnemyCountChanged;
            }

            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
                bannerRoutine = null;
            }

            if (waveBannerText != null)
            {
                waveBannerText.gameObject.SetActive(false);
            }
        }

        private void HandleWaveStarted(int waveNumber)
        {
            if (waveText != null)
            {
                waveText.text = waveDirector != null ? waveDirector.GetWaveHudLabel(waveNumber) : (waveNumber <= 0 ? "Wave: -" : $"Wave: {waveNumber}");
            }

            ShowBanner(waveDirector != null ? waveDirector.GetWaveBannerLabel(waveNumber) : string.Empty);
        }

        private void HandleBossWaveStarted()
        {
            if (waveText != null)
            {
                waveText.text = "Wave: BOSS";
            }
        }

        private void HandleAliveEnemyCountChanged(int aliveEnemyCount)
        {
            if (enemiesText != null)
            {
                enemiesText.text = $"Enemies: {aliveEnemyCount}";
            }
        }

        private void ShowBanner(string bannerLabel)
        {
            if (waveBannerText == null || string.IsNullOrWhiteSpace(bannerLabel))
            {
                return;
            }

            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
            }

            bannerRoutine = StartCoroutine(ShowBannerRoutine(bannerLabel));
        }

        private IEnumerator ShowBannerRoutine(string bannerLabel)
        {
            waveBannerText.gameObject.SetActive(true);
            waveBannerText.text = bannerLabel;
            SetBannerAlpha(1f);

            yield return new WaitForSecondsRealtime(bannerVisibleDuration);

            float elapsed = 0f;

            while (elapsed < bannerFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / bannerFadeDuration));
                SetBannerAlpha(alpha);
                yield return null;
            }

            SetBannerAlpha(0f);
            waveBannerText.gameObject.SetActive(false);
            bannerRoutine = null;
        }

        private void SetBannerAlpha(float alpha)
        {
            if (waveBannerText == null)
            {
                return;
            }

            Color color = bannerBaseColor;
            color.a = alpha;
            waveBannerText.color = color;
        }
    }
}
