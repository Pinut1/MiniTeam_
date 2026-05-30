using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class MiniGameManager : MonoBehaviour
    {
        public enum State { Init, Hub_Idle, Hub_Cutscene, MiniGame_Transition, MiniGame_Playing }

        public static MiniGameManager Instance { get; private set; }

        public State CurrentState { get; private set; }

        private string currentScene;
        private GameObject[] hubRootObjects;

        private HubPlayerMove playerMove;

        [Header("Game Progress Data")]
        public GameProgressData progressData = new GameProgressData();

        #region Unity Life Cycle

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
            // 저장된 스테이지 정보 로드
            LoadGame();

            playerMove = FindAnyObjectByType<HubPlayerMove>();

            ChangeState(State.Init);
            if (SpiralDiveCutscene.Instance != null)
                SpiralDiveCutscene.Instance.PlayIfFirstTime(() => HubCutsceneDirector.Instance?.PlayWakeUpSequence());
            else
                HubCutsceneDirector.Instance?.PlayWakeUpSequence();
        }

        #endregion

        #region State Management
        
        public void ChangeState(State newState)
        {
            CurrentState = newState;
            Debug.Log($"[MiniGameManager] State changed to: {newState}");

            switch (newState)
            {
                case State.Init:
                case State.Hub_Cutscene:
                case State.MiniGame_Transition:
                case State.MiniGame_Playing:
                    if (playerMove != null)
                    {
                        playerMove.UnlockCursor();
                        playerMove.enabled = false;
                    }
                    break;

                case State.Hub_Idle:
                    if (playerMove != null)
                    {
                        playerMove.LockCursor();
                        playerMove.enabled = true;
                    }
                    break;
            }
        }

        #endregion

        #region Public Methods

        public bool IsInMiniGame => !string.IsNullOrEmpty(currentScene);

        public void EnterMiniGame(string sceneName)
        {
            if (IsInMiniGame) return;

            ChangeState(State.MiniGame_Transition);

            // 미니게임 진입 시 기존 BGM 및 효과음 강제 종료 (안전장치)
            SoundManager.Instance?.StopBGM();
            SoundManager.Instance?.StopAllSFX();

            // Hub 씬 오브젝트 숨기기 (DontDestroyOnLoad 오브젝트는 이미 별도 씬으로 이동했으므로 포함 안 됨)
            hubRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var go in hubRootObjects)
                go.SetActive(false);

            currentScene = sceneName;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            ChangeState(State.MiniGame_Playing);
        }

        public void ExitMiniGame()
        {
            if (string.IsNullOrEmpty(currentScene)) return;

            // 미니게임 탈출 시 모든 오디오 강제 종료
            SoundManager.Instance?.StopBGM();
            SoundManager.Instance?.StopAllSFX();

            var op = SceneManager.UnloadSceneAsync(currentScene);
            currentScene = null;

            // 씬 언로드 완료 후 Hub 오브젝트 복원 (즉시 복원 시 깜빡임 방지)
            if (op != null)
                op.completed += _ => RestoreHub();
            else
                RestoreHub();
        }

        public void OnMiniGameClear()
        {
            progressData.isLastGameCleared = true;
            progressData.currentStage++;
            progressData.isCutscenePlayed = false; // 새로운 스테이지 진입으로 컷신 미재생 초기화
            SaveGame(); // 스테이지 증가 및 컷신 미재생 상태 저장
            ExitMiniGame();
        }

        public void OnMiniGameFail()
        {
            progressData.isLastGameCleared = false;
            ExitMiniGame();
        }

        public void DisablePlayerInput()
        {
            ChangeState(State.Hub_Cutscene);
        }

        public void EnablePlayerInput()
        {
            ChangeState(State.Hub_Idle);
        }

        public void SaveGame()
        {
            SaveSystem.Save(progressData);
        }

        public void LoadGame()
        {
            progressData = SaveSystem.Load();
        }

        public void ResetSaveData()
        {
            SaveSystem.ResetSaveData();
            progressData.Reset();
        }

        public void SetCutscenePlayed(bool played)
        {
            progressData.isCutscenePlayed = played;
            SaveGame(); // 컷신 상태 즉시 저장
            BottomUIController.Instance?.UpdateExclamationMark(); // 느낌표 UI 실시간 업데이트
        }

        #endregion

        #region Private Methods

        internal void LoadEndingScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndingCut_Test");
        }

        private void RestoreHub()
        {
            ChangeState(State.MiniGame_Transition);

            if (hubRootObjects == null) return;

            // 1. 플레이어 위치를 먼저 안전한 원점으로 이동 (CharacterController 일시 정지)
            if (playerMove != null)
            {
                CharacterController cc = playerMove.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false; // 물리 씹힘 방지

                playerMove.gameObject.transform.position = new Vector3(0, 0f, 0);

                if (cc != null) cc.enabled = true;
            }

            // 2. 그 후 허브 오브젝트 복원
            foreach (var go in hubRootObjects)
                if (go != null) go.SetActive(true);
            hubRootObjects = null;

            if (SoundManager.Instance != null && AudioManager.Instance != null)
            {
                SoundManager.Instance.SetBGMPitch(0.7f);
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmHub);
            }

            if (progressData.isLastGameCleared)
            {
                HubCutsceneDirector.Instance?.PlaySequenceForGameClear();
            }
            else
            {
                HubCutsceneDirector.Instance?.PlayWakeUpSequence();
            }
        }

        #endregion
    }
}
