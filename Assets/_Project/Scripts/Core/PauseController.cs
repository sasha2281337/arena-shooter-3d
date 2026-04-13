using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaShooter.Core
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject[] blockingPanels;
        [SerializeField] private Key pauseKey = Key.Escape;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool isPaused;

        private void Awake()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (!Keyboard.current[pauseKey].wasPressedThisFrame)
            {
                return;
            }

            if (IsAnyBlockingPanelActive())
            {
                return;
            }

            TogglePause();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            Time.timeScale = isPaused ? 0f : 1f;
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            TogglePause();
        }

        public void LoadMainMenu()
        {
            isPaused = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private bool IsAnyBlockingPanelActive()
        {
            if (blockingPanels == null)
            {
                return false;
            }

            for (int i = 0; i < blockingPanels.Length; i++)
            {
                if (blockingPanels[i] != null && blockingPanels[i].activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
