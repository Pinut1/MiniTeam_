using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    public class ShootingGameController : MonoBehaviour, IMiniGame
    {
        [Header("연출")]
        public GameObject spaceshipRewardObj;
        public GameObject coinSpinObj;         // 이어하기 코인 (씬에 배치된 오브젝트)
        public EntryCutsceneManager entryCutscene;
        public ClearCutsceneManager clearCutscene;

        [Header("결과 화면 표시 후 허브 복귀까지 대기 시간")]
        public float resultHoldTime = 3f;

        private bool isGameOver        = false;
        private bool isCleared         = false;
        private bool isWaitingContinue = false;
        private WaveManager waveManager;

        void Update()
        {
            if (isWaitingContinue && Input.GetKeyDown(KeyCode.Return))
                StartCoroutine(ContinueRoutine());
        }

        IEnumerator ContinueRoutine()
        {
            isWaitingContinue = false;

            var am = AudioManager.Instance;
            am?.StopBGM();
            am?.PlaySFX(am.sfxContinueCoin);

            if (coinSpinObj != null)
            {
                coinSpinObj.SetActive(true);
                var anim = coinSpinObj.GetComponent<Animator>();
                if (anim != null) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            yield return new WaitForSecondsRealtime(0.6f);

            isGameOver     = false;
            Time.timeScale = 1f;

            FindAnyObjectByType<FormationManager>()?.FullRestore();

            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) player.enabled = true;

            if (coinSpinObj != null) coinSpinObj.SetActive(false);
            waveManager?.ResumeBGM();
            ShootingUIManager.Instance?.HideResult();
        }

        void Start()
        {
            SceneManager.SetActiveScene(gameObject.scene);

            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(false);

            waveManager = GetComponent<WaveManager>();
            if (waveManager == null)
                waveManager = FindAnyObjectByType<WaveManager>();

            if (entryCutscene != null)
                entryCutscene.Play(() => waveManager?.BeginGame());
        }

        // ── IMiniGame ─────────────────────────────

        public void OnGameClear()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = true;

            EndGame();

            var am = AudioManager.Instance;
            if (am != null) am.PlayBGM(am.bgmClear);

            Debug.Log($"[1942] OnGameClear | clearCutscene:{clearCutscene != null}");
            if (clearCutscene != null)
            {
                clearCutscene.Play(() =>
                {
                    if (spaceshipRewardObj != null)
                        spaceshipRewardObj.SetActive(true);
                    Invoke(nameof(ExitToHub), resultHoldTime);
                });
            }
            else
            {
                if (spaceshipRewardObj != null)
                    spaceshipRewardObj.SetActive(true);
                Invoke(nameof(ExitToHub), resultHoldTime);
            }
        }

        public void OnGameFail()
        {
            if (isGameOver) return;
            isGameOver = true;
            isCleared  = false;

            OptionsUIManager.Instance?.ForceClose();
            AudioManager.Instance?.StopBGM();
            var am = AudioManager.Instance;
            if (am != null) am.PlayBGM(am.bgmGameOver);

            Time.timeScale        = 0f;
            isWaitingContinue     = true;
            ShootingUIManager.Instance?.ShowResult(false);
        }

        void EndGame()
        {
            waveManager?.StopGame();
            AudioManager.Instance?.StopBGM();
            OptionsUIManager.Instance?.ForceClose();
            Time.timeScale = 1f;

            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.enabled = false;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                player.transform.position = new Vector3(3.35f, -1.75f, 0f);
            }
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

            var ui = ShootingUIManager.Instance;
            if (ui != null)
            {
                string gaugeLabel = ui.IsSpecialReady ? "필살기 게이지 FULL" : "필살기 게이지 충전";
                GUI.enabled = !ui.IsSpecialReady;
                if (GUILayout.Button(gaugeLabel))
                    ui.AddSpecialGauge(100f);
                GUI.enabled = true;
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
