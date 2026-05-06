using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        private string currentScene;

        public bool IsInMiniGame => !string.IsNullOrEmpty(currentScene);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterMiniGame(string sceneName)
        {
            currentScene = sceneName;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void ExitMiniGame()
        {
            if (string.IsNullOrEmpty(currentScene)) return;
            SceneManager.UnloadSceneAsync(currentScene);
            currentScene = null;
        }

        public void OnMiniGameClear()
        {
            ExitMiniGame();
        }

        public void OnMiniGameFail()
        {
            ExitMiniGame();
        }
    }
}
