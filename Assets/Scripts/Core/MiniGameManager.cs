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

        /// <summary>
        /// Ensures a single MiniGameManager instance exists and makes the surviving instance persist across scene loads.
        /// </summary>
        /// <remarks>
        /// If another MiniGameManager already exists, this GameObject is destroyed; otherwise this instance is assigned to <c>Instance</c> and marked with <c>DontDestroyOnLoad</c>.
        /// </remarks>
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

        /// <summary>
        /// Begins a mini-game by recording and hiding the current hub scene's root objects and loading the specified scene additively.
        /// </summary>
        /// <param name="sceneName">The name of the mini-game scene to load.</param>
        /// <remarks>
        /// If a mini-game is already active, the method returns without action. The hub root objects are stored so they can be restored after the mini-game ends.
        /// </remarks>
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

        /// <summary>
        /// Exits the currently active mini-game by unloading its scene and restoring the hub scene's root objects.
        /// </summary>
        /// <remarks>
        /// If no mini-game is active this method does nothing. It begins unloading the active mini-game scene, clears the internal scene tracker, and restores the previously hidden hub root objects after the unload completes. If the async unload operation is unavailable, hub objects are restored immediately.
        /// </remarks>
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

        /// <summary>
        /// Restores previously hidden hub root GameObjects by re-enabling them and clears the cached references.
        /// </summary>
        /// <remarks>
        /// Does nothing if no hub root objects are saved.
        /// </remarks>
        private void RestoreHub()
        {
            if (hubRootObjects == null) return;
            foreach (var go in hubRootObjects)
                if (go != null) go.SetActive(true);
            hubRootObjects = null;
        }

        /// <summary>
        /// Handles successful completion of the current mini-game and initiates returning to the hub.
        /// </summary>
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
