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
        public int currentStage = 0;
        public bool isCutscenePlayed = false;
        private bool isLastGameCleared = false;

        [Header("Door Management")]
        [Tooltip("스테이지 순서대로 문(Stage Door)을 할당. (Stage 1 = Index 0)")]
        public StageDoor[] stageDoors;

        private const string SAVE_STAGE_KEY = "SavedCurrentStage";

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
            // 저장된 스테이지 정보 로드
            LoadGame();

            playerMove = FindAnyObjectByType<HubPlayerMove>();


            void PlayWakeUp()
            {
                HubUIManager.Instance.WakeUp(() =>
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.bgmHub != null)
                    {
                        SoundManager.Instance?.SetBGMPitch(0.7f);
                        AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmHub);
                    }
                    if (playerMove != null) EnablePlayerInput();
                });
            }

            if (playerMove != null)
            {
                DisablePlayerInput();
                // 스파이럴 → 눈깜빡 → BGM + 플레이어 입력 활성화
                if (SpiralDiveCutscene.Instance != null)
                    SpiralDiveCutscene.Instance.PlayIfFirstTime(() => PlayWakeUp());
                else
                    PlayWakeUp();
            }
            else
            {
                PlayWakeUp();
            }
           
        }
        public void EnterMiniGame(string sceneName)
        {
            if (IsInMiniGame) return;

            // 미니게임 진입 시 기존 BGM 및 효과음 강제 종료 (안전장치)
            SoundManager.Instance?.StopBGM();
            SoundManager.Instance?.StopAllSFX();

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

        private void RestoreHub()
        {
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

            if (isLastGameCleared)
            {
                JudangChiController.Instance?.PlaySequenceForGameClear();
            }
            else
            {
                EnablePlayerInput();
                HubUIManager.Instance?.InitializeBottomUI(currentStage);
            }

            // 미니게임에서 허브로 복귀 시 허브 BGM 다시 재생 (피치 0.7)
            if (AudioManager.Instance != null && AudioManager.Instance.bgmHub != null)
            {
                SoundManager.Instance?.SetBGMPitch(0.7f);
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmHub);
            }
        }

        public void OnMiniGameClear()
        {
            isLastGameCleared = true;
            currentStage++;
            isCutscenePlayed = false; // 새로운 스테이지 진입으로 컷신 미재생 초기화
            SaveGame(); // 스테이지 증가 및 컷신 미재생 상태 저장
            ExitMiniGame();
        }

        public void OnMiniGameFail()
        {
            isLastGameCleared = false;
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

        // ── 세이브/로드 시스템 ──────────────────────────────────

        public void SaveGame()
        {
            PlayerPrefs.SetInt(SAVE_STAGE_KEY, currentStage);
            PlayerPrefs.SetInt("SavedCutscenePlayed", isCutscenePlayed ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Game Saved. Current Stage: {currentStage}, Cutscene Played: {isCutscenePlayed}");
        }

        public void LoadGame()
        {
            // 기본 스테이지는 0으로 설정
            currentStage = PlayerPrefs.GetInt(SAVE_STAGE_KEY, 0);
            isCutscenePlayed = PlayerPrefs.GetInt("SavedCutscenePlayed", 0) == 1;
            Debug.Log($"[SaveSystem] Game Loaded. Current Stage: {currentStage}, Cutscene Played: {isCutscenePlayed}");
        }

        public void ResetSaveData()
        {
            PlayerPrefs.DeleteKey(SAVE_STAGE_KEY);
            PlayerPrefs.DeleteKey("SavedCutscenePlayed");
            PlayerPrefs.Save();
            currentStage = 0;
            isCutscenePlayed = false;
            Debug.Log("[SaveSystem] Save Data Reset.");
        }

        public void SetCutscenePlayed(bool played)
        {
            isCutscenePlayed = played;
            SaveGame(); // 컷신 상태 즉시 저장
            HubUIManager.Instance?.UpdateExclamationMark(); // 느낌표 UI 실시간 업데이트
        }


    }
}
