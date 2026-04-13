using ArenaShooter.Waves;
using UnityEngine;

namespace ArenaShooter.Upgrades
{
    public class UpgradeSelectionController : MonoBehaviour
    {
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private PlayerUpgradeController playerUpgradeController;
        [SerializeField] private UpgradeSelectionUI selectionUI;
        [SerializeField] private UpgradeData[] upgradePool;
        [SerializeField, Range(1, 3)] private int choicesCount = 3;
        [SerializeField] private bool pauseTimeDuringChoice = true;

        private void Awake()
        {
            if (waveDirector == null)
            {
                waveDirector = FindFirstObjectByType<WaveDirector>();
            }

            if (playerUpgradeController == null)
            {
                playerUpgradeController = FindFirstObjectByType<PlayerUpgradeController>();
            }

            if (selectionUI != null)
            {
                selectionUI.Initialize(this);
                selectionUI.Hide();
            }

            if (waveDirector != null)
            {
                waveDirector.AutoStartNextWave = false;
            }
        }

        private void OnEnable()
        {
            if (waveDirector != null)
            {
                waveDirector.WaveCompleted += HandleWaveCompleted;
            }
        }

        private void OnDisable()
        {
            if (waveDirector != null)
            {
                waveDirector.WaveCompleted -= HandleWaveCompleted;
            }
        }

        public void SelectUpgrade(UpgradeData upgrade)
        {
            if (upgrade != null && playerUpgradeController != null)
            {
                playerUpgradeController.ApplyUpgrade(upgrade);
            }

            if (selectionUI != null)
            {
                selectionUI.Hide();
            }

            if (pauseTimeDuringChoice)
            {
                Time.timeScale = 1f;
            }

            if (waveDirector != null)
            {
                waveDirector.StartNextWave();
            }
        }

        private void HandleWaveCompleted(int completedWave)
        {
            UpgradeData[] choices = RollChoices();

            if (choices.Length == 0 || selectionUI == null)
            {
                waveDirector.StartNextWave();
                return;
            }

            if (pauseTimeDuringChoice)
            {
                Time.timeScale = 0f;
            }

            selectionUI.ShowChoices(completedWave, choices);
        }

        private UpgradeData[] RollChoices()
        {
            if (upgradePool == null || upgradePool.Length == 0)
            {
                return new UpgradeData[0];
            }

            int count = Mathf.Min(choicesCount, upgradePool.Length);
            UpgradeData[] result = new UpgradeData[count];
            int added = 0;
            int safety = 0;

            while (added < count && safety < 100)
            {
                safety++;
                UpgradeData candidate = upgradePool[Random.Range(0, upgradePool.Length)];

                if (candidate == null || Contains(result, candidate, added))
                {
                    continue;
                }

                result[added] = candidate;
                added++;
            }

            if (added == count)
            {
                return result;
            }

            UpgradeData[] trimmed = new UpgradeData[added];

            for (int i = 0; i < added; i++)
            {
                trimmed[i] = result[i];
            }

            return trimmed;
        }

        private static bool Contains(UpgradeData[] choices, UpgradeData candidate, int filledCount)
        {
            for (int i = 0; i < filledCount; i++)
            {
                if (choices[i] == candidate)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
