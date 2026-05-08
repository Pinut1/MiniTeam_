using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        private string currentScene;
        private GameObject[] hubRootObjects;

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
            if (IsInMiniGame) return;

            // Hub 씬 오브젝트 숨기기 (DontDestroyOnLoad 오브젝트는 이미 별도 씬으로 이동했으므로 포함 안 됨)
            hubRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var go in hubRootObjects)
                go.SetActive(false);

            currentScene = sceneName;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void ExitMiniGame()
        {
            if (string.IsNullOrEmpty(currentScene)) return;

            var op = SceneManager.UnloadSceneAsync(currentScene);
            currentScene = null;

            // 씬 언로드 완료 후 Hub 오브젝트 복원 (즉시 복원 시 깜빡임 방지)
            if (op != null)
                op.completed += _ => RestoreHub();
            else
                RestoreHub();
        }

        private void RestoreHub()
        {
            if (hubRootObjects == null) return;
            foreach (var go in hubRootObjects)
                if (go != null) go.SetActive(true);
            hubRootObjects = null;
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
