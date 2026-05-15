using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 웨이브 진행 / 적 소환 / 보스 등장 통합 관리
    public class WaveManager : MonoBehaviour
    {
        [Header("적 프리팹")]
        public GameObject[] enemyPrefabs;

        [Header("Wave 1")]
        public float wave1Duration   = 30f;
        public float wave1Interval   = 2.5f;
        public float wave1Speed      = 1.2f;
        [Range(0f, 1f)]
        public float wave1ShootRatio = 0.3f;

        [Header("Wave 2")]
        public float wave2Duration   = 30f;
        public float wave2Interval   = 1.8f;
        public float wave2Speed      = 1.8f;
        [Range(0f, 1f)]
        public float wave2ShootRatio = 0.5f;

        [Header("보스")]
        public GameObject bossPrefab;
        public Transform  bossSpawnPoint;

        [Header("소환 위치")]
        public float spawnYOffset = 1f;

        [Header("게임 영역 (GameView RectTransform 연결)")]
        public RectTransform gameAreaRect;

        public bool IsBossSpawned  { get; private set; } = false;
        public bool IsBossDefeated { get; private set; } = false;

        public float SpawnMinX     { get; private set; }
        public float SpawnMaxX     { get; private set; }
        public float DestroyBoundsY { get; private set; }

        private float spawnTopY, spawnMinX, spawnMaxX;
        private float currentInterval;
        private int   currentWave = 0;
        private bool  spawning    = false;
        private bool  gameStopped = false;
        private Coroutine spawnCoroutine;
        private Coroutine waveCoroutine;

        private ShootingGameController gameController;

        void Start()
        {
            gameController = GetComponent<ShootingGameController>();
            if (gameController == null)
                gameController = FindAnyObjectByType<ShootingGameController>();

            CalculateSpawnBounds();
            waveCoroutine = StartCoroutine(RunWaves());
        }

        // ── 웨이브 흐름 ──────────────────────────

        IEnumerator RunWaves()
        {
            StartWave(1);
            yield return new WaitForSeconds(wave1Duration);

            StartWave(2);
            yield return new WaitForSeconds(wave2Duration);

            StopSpawning();
            if (!gameStopped)
                SpawnBoss();
        }

        void StartWave(int wave)
        {
            currentWave     = wave;
            currentInterval = wave == 1 ? wave1Interval : wave2Interval;

            AudioManager.Instance?.PlayBGM(wave == 1
                ? AudioManager.Instance.bgmWave1
                : AudioManager.Instance.bgmWave2);

            ShootingUIManager.Instance?.ShowWaveMessage($"WAVE {wave}", 2f);

            StartSpawning();
        }

        // ── 적 소환 ──────────────────────────────

        void StartSpawning()
        {
            spawning = true;
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        void StopSpawning()
        {
            spawning = false;
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        }

        IEnumerator SpawnLoop()
        {
            while (spawning)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(currentInterval);
            }
        }

        void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            float randomX    = Random.Range(spawnMinX, spawnMaxX);
            Vector3 spawnPos = new Vector3(randomX, spawnTopY, 0f);

            int idx        = Random.Range(0, enemyPrefabs.Length);
            GameObject obj = Instantiate(enemyPrefabs[idx], spawnPos, Quaternion.identity);

            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy == null) return;

            enemy.moveSpeed = currentWave == 1 ? wave1Speed : wave2Speed;

            float shootRatio = currentWave == 1 ? wave1ShootRatio : wave2ShootRatio;
            enemy.canShoot = Random.value < shootRatio;

            if (currentWave == 2 && Random.value > 0.5f)
                enemy.movementType = Enemy.MovementType.Sine;
        }

        // ── 보스 ─────────────────────────────────

        public void DebugSkipToBoss()
        {
            StopGame();
            gameStopped = false;
            SpawnBoss();
        }

        void SpawnBoss()
        {
            IsBossSpawned = true;
            Vector3 spawnPos = bossSpawnPoint != null
                ? bossSpawnPoint.position
                : new Vector3(0f, 6f, 0f);

            GameObject bossObj     = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            BossController boss    = bossObj.GetComponent<BossController>();
            if (boss != null)
                boss.OnBossDefeated += HandleBossDefeated;

            AudioManager.Instance?.PlayBGM(AudioManager.Instance.bgmBoss);
            ShootingUIManager.Instance?.ShowWaveMessage("BOSS!", 2f);
        }

        void HandleBossDefeated()
        {
            IsBossDefeated = true;
            gameController?.OnGameClear();
        }

        // ── 외부 호출 ─────────────────────────────

        public void StopGame()
        {
            gameStopped = true;
            StopSpawning();
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        }

        // ── 유틸 ─────────────────────────────────

        void CalculateSpawnBounds()
        {
            Camera cam  = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);

            Vector3 worldBL, worldTR;

            if (gameAreaRect != null)
            {
                Vector3[] corners = new Vector3[4];
                gameAreaRect.GetWorldCorners(corners);
                // Screen Space Overlay Canvas에서 GetWorldCorners는 실제 픽셀 좌표를 반환
                worldBL = cam.ScreenToWorldPoint(new Vector3(corners[0].x, corners[0].y, depth));
                worldTR = cam.ScreenToWorldPoint(new Vector3(corners[2].x, corners[2].y, depth));
            }
            else
            {
                worldBL = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));
                worldTR = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
            }

            spawnTopY      = worldTR.y + spawnYOffset;
            spawnMinX      = worldBL.x + 0.5f;
            spawnMaxX      = worldTR.x - 0.5f;
            DestroyBoundsY = worldBL.y - 1f;

            SpawnMinX = spawnMinX;
            SpawnMaxX = spawnMaxX;
        }
    }
}
