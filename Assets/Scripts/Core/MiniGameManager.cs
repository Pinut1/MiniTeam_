using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        private string currentScene;
        private GameObject[] hubRootObjects;

        private HubPlayerMove playerMove;

        [Header("Game Progress")]
        public int currentStage = 1;

        [Header("Door Management")]
        [Tooltip("스테이지 순서대로 문(Stage Door)을 할당. (Stage 1 = Index 0)")]
        public StageDoor[] stageDoors;

        public bool IsDoorActive(StageDoor door)
        {
            // currentStage는 1부터 시작하고 배열 인덱스는 0부터 시작하므로 수 맞춤
            int stageIndex = currentStage ;

            if (stageDoors == null || stageDoors.Length == 0) return false;
            if (stageIndex < 0 || stageIndex >= stageDoors.Length) return false;

            return stageDoors[stageIndex] == door;
        }
  

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
        private void Start()
        {
            playerMove = FindAnyObjectByType<HubPlayerMove>();


            if (playerMove != null)
            {
                // 눈 깜빡임 연출이 진행되는 동안 플레이어가 움직이지 못하게 스크립트 OFF
                DisablePlayerInput();
                // 연출(WakeUp)을 실행, 다 끝나면 다음 메서드를 콜백.
                HubUIManager.Instance.WakeUp(() =>EnablePlayerInput());
            }
            else
            {
                HubUIManager.Instance.WakeUp();
            }
           
        }
        public void EnterMiniGame(string sceneName)
        {
            if (IsInMiniGame) return;

            // Hub 씬 오브젝트 숨기기 (DontDestroyOnLoad 오브젝트는 이미 별도 씬으로 이동했으므로 포함 안 됨)
            playerMove.UnlockCursor();
            hubRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var go in hubRootObjects)
                go.SetActive(false);

            currentScene = sceneName;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void ExitMiniGame()
        {
            if (string.IsNullOrEmpty(currentScene)) return;

            SoundManager.Instance?.StopBGM();

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

            playerMove.gameObject.transform.position = new Vector3(0, 0f, 0) ;


            // 허브 씬이 다시 켜진 직후, 현재 스테이지에 맞는 연출을 지시.
            JudangChiController.Instance?.PlaySequenceForGameClear();
        }

        public void OnMiniGameClear()
        {
            currentStage++;
            ExitMiniGame();
        }

        public void OnMiniGameFail()
        {
            ExitMiniGame();
        }

        public void DisablePlayerInput()
        {

            if (playerMove != null)
            {
                playerMove.UnlockCursor();
                playerMove.enabled = false;
            }
        }

        public void EnablePlayerInput()
        {
            if (playerMove != null)
            {
                playerMove.LockCursor();
                playerMove.enabled = true;
            }
        }

        internal void LoadEndingScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndingCut_Test");
        }


    }
}
