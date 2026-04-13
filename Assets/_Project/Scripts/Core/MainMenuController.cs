using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaShooter.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "ArenaPrototype";

        private void Awake()
        {
            Time.timeScale = 1f;
        }

        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }
    }
}
