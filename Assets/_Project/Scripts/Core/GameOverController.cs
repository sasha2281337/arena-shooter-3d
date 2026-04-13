using ArenaShooter.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaShooter.Core
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private bool pauseTimeOnGameOver = true;
        [SerializeField] private bool allowRestartHotkey = true;
        [SerializeField] private Key restartKey = Key.R;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool isSubscribed;
        private bool isGameOver;

        private void Awake()
        {
            Time.timeScale = 1f;
            ResolvePlayerHealth();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!isGameOver || !allowRestartHotkey || Keyboard.current == null)
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

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }

        private void ResolvePlayerHealth()
        {
            if (playerHealth != null || !autoFindPlayer)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponent<Health>();
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            ResolvePlayerHealth();

            if (playerHealth == null)
            {
                return;
            }

            playerHealth.Died += HandlePlayerDied;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || playerHealth == null)
            {
                return;
            }

            playerHealth.Died -= HandlePlayerDied;
            isSubscribed = false;
        }

        private void HandlePlayerDied()
        {
            isGameOver = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (pauseTimeOnGameOver)
            {
                Time.timeScale = 0f;
            }
        }
    }
}
