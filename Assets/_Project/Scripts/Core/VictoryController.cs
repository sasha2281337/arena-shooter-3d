using ArenaShooter.Waves;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaShooter.Core
{
    public class VictoryController : MonoBehaviour
    {
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private bool pauseTimeOnVictory = true;
        [SerializeField] private bool allowRestartHotkey = true;
        [SerializeField] private Key restartKey = Key.R;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool isVictory;

        private void Awake()
        {
            if (waveDirector == null)
            {
                waveDirector = FindFirstObjectByType<WaveDirector>();
            }

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (waveDirector != null)
            {
                waveDirector.RunCompleted += HandleRunCompleted;
            }
        }

        private void OnDisable()
        {
            if (waveDirector != null)
            {
                waveDirector.RunCompleted -= HandleRunCompleted;
            }
        }

        private void Update()
        {
            if (!isVictory || !allowRestartHotkey || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current[restartKey].wasPressedThisFrame)
            {
                RestartScene();
            }
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void HandleRunCompleted()
        {
            isVictory = true;

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            if (pauseTimeOnVictory)
            {
                Time.timeScale = 0f;
            }
        }
    }
}
