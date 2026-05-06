using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    public class ShootingGameController : MonoBehaviour, IMiniGame
    {
        [Header("연출")]
        public GameObject spaceshipRewardObj;

        [Header("결과 화면 표시 후 허브 복귀까지 대기 시간")]
        public float resultHoldTime = 3f;

        private bool isGameOver = false;
        private bool isCleared  = false;
        private bool isPaused   = false;
        private WaveManager waveManager;

        /// <summary>
        /// Initializes controller state by hiding the configured spaceship reward object and resolving the WaveManager reference.
        /// </summary>
        /// <remarks>
        /// If <c>spaceshipRewardObj</c> is assigned, it will be deactivated. Attempts to obtain <c>waveManager</c> from the same GameObject first; if not found, searches the scene for any <c>WaveManager</c>.
        /// </remarks>
        void Start()
        {
            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(false);

            waveManager = GetComponent<WaveManager>();
            if (waveManager == null)
                waveManager = FindAnyObjectByType<WaveManager>();
        }

        /// <summary>
        /// Handles per-frame input to toggle the game's pause state and ignores input once the game is over.
        /// </summary>
        /// <remarks>
        /// If the game is marked as over, the method returns immediately. When the Escape key is pressed and the game is not over, it toggles pause/resume.
        /// </remarks>
        void Update()
        {
            if (isGameOver) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        /// <summary>
        /// Toggles the game's paused state.
        /// </summary>
        /// <remarks>
        /// Updates the global time scale and notifies the UI of the new pause state.
        /// </remarks>

        void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            ShootingUIManager.Instance?.ShowPause(isPaused);
        }

        /// <summary>
        /// Finalize the game as a win and start the post-win sequence.
        /// </summary>
        /// <remarks>
        /// Marks the game as ended and cleared, restores time scale if paused, stops the wave manager, activates the spaceship reward object if assigned, shows the victory result UI, and schedules ExitToHub to run after <c>resultHoldTime</c>.
        /// </remarks>

        public void OnGameClear()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = true;

            if (isPaused) Time.timeScale = 1f;

            Debug.Log("[1942] Game Clear!");
            waveManager?.StopGame();

            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(true);

            ShootingUIManager.Instance?.ShowResult(true);
            Invoke(nameof(ExitToHub), resultHoldTime);
        }

        /// <summary>
        /// Handle a game failure by finalizing state and starting the failure result sequence.
        /// </summary>
        /// <remarks>
        /// Marks the game as over and not cleared; if the game was paused, restores normal time scale.
        /// Stops the wave manager, displays the failure result UI, and schedules ExitToHub to run after <c>resultHoldTime</c> seconds.
        /// </remarks>
        public void OnGameFail()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = false;

            if (isPaused) Time.timeScale = 1f;

            Debug.Log("[1942] Game Fail!");
            waveManager?.StopGame();

            ShootingUIManager.Instance?.ShowResult(false);
            Invoke(nameof(ExitToHub), resultHoldTime);
        }

        // ── 디버그 패널 (Development Build 전용) ──

#pragma warning disable CS0162
        private PlayerHit      debugPlayerHit;
        private PlayerController debugPlayerCtrl;

        /// <summary>
        /// Renders a small on-screen debug panel in development builds for testing gameplay features.
        /// </summary>
        /// <remarks>
        /// The panel appears only when Debug.isDebugBuild is true. It provides controls to:
        /// - immediately summon the boss,
        /// - force the boss into phase 2,
        /// - toggle the player's god mode,
        /// - toggle the player's rapid-fire debug mode.
        /// The method also lazily resolves player-related debug objects when needed.
        /// </remarks>
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            if (debugPlayerHit  == null) debugPlayerHit  = FindAnyObjectByType<PlayerHit>();
            if (debugPlayerCtrl == null) debugPlayerCtrl = FindAnyObjectByType<PlayerController>();

            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            GUILayout.Label("[ DEBUG ]");

            if (GUILayout.Button("보스 바로 소환"))
                waveManager?.DebugSkipToBoss();

            BossController boss = FindAnyObjectByType<BossController>();
            if (boss != null)
            {
                string phaseLabel = boss.IsPhase2 ? "2페이즈 중" : "2페이즈 강제 진입";
                GUI.enabled = !boss.IsPhase2;
                if (GUILayout.Button(phaseLabel)) boss.ForcePhase2();
                GUI.enabled = true;
            }

            if (debugPlayerHit != null)
            {
                string godLabel = debugPlayerHit.IsGodMode ? "무적 ON" : "무적 OFF";
                if (GUILayout.Button(godLabel))
                    debugPlayerHit.IsGodMode = !debugPlayerHit.IsGodMode;
            }

            if (debugPlayerCtrl != null)
            {
                string rapidLabel = debugPlayerCtrl.DebugRapidFire ? "공격력 증가 ON" : "공격력 증가 OFF";
                if (GUILayout.Button(rapidLabel))
                    debugPlayerCtrl.DebugRapidFire = !debugPlayerCtrl.DebugRapidFire;
            }

            GUILayout.EndArea();
        }
#pragma warning restore CS0162

        /// <summary>
        /// Restores game time scale and notifies the MiniGameManager of the mini-game result, returning to the hub.
        /// </summary>
        /// <remarks>
        /// If MiniGameManager.Instance is null, logs a debug message and exits without notifying.
        /// </remarks>
        void ExitToHub()
        {
            Time.timeScale = 1f;

            if (MiniGameManager.Instance == null)
            {
                Debug.Log("[1942] MiniGameManager 없음 - 씬 단독 테스트 중");
                return;
            }

            if (isCleared)
                MiniGameManager.Instance.OnMiniGameClear();
            else
                MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
