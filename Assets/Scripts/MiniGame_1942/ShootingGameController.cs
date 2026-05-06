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
        private WaveManager waveManager;

        void Start()
        {
            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(false);

            waveManager = GetComponent<WaveManager>();
            if (waveManager == null)
                waveManager = FindAnyObjectByType<WaveManager>();
        }

        // ── IMiniGame ─────────────────────────────

        public void OnGameClear()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = true;

            EndGame();

            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(true);

            AudioManager.Instance?.PlayBGM(AudioManager.Instance.bgmClear);
            ShootingUIManager.Instance?.ShowResult(true);
            Invoke(nameof(ExitToHub), resultHoldTime);
        }

        public void OnGameFail()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = false;

            EndGame();

            AudioManager.Instance?.PlayBGM(AudioManager.Instance.bgmGameOver);
            ShootingUIManager.Instance?.ShowResult(false);
            Invoke(nameof(ExitToHub), resultHoldTime);
        }

        void EndGame()
        {
            waveManager?.StopGame();
            AudioManager.Instance?.StopBGM();
            OptionsUIManager.Instance?.ForceClose();
            Time.timeScale = 1f;

            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) player.enabled = false;
        }

        // ── 디버그 패널 (Development Build 전용) ──

#pragma warning disable CS0162
        private PlayerHit      debugPlayerHit;
        private PlayerController debugPlayerCtrl;

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
