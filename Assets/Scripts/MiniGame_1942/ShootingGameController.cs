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

        void Start()
        {
            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(false);

            waveManager = GetComponent<WaveManager>();
            if (waveManager == null)
                waveManager = FindAnyObjectByType<WaveManager>();
        }

        void Update()
        {
            if (isGameOver) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        // ── 일시정지 ──────────────────────────────

        void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            ShootingUIManager.Instance?.ShowPause(isPaused);
        }

        // ── IMiniGame ─────────────────────────────

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
