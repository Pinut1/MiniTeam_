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
        public float wave1Duration   = 60f;
        public float wave1Interval   = 2.5f;
        public float wave1Speed      = 1.2f;
        [Range(0f, 1f)]
        public float wave1ShootRatio = 0.3f;

        [Header("Wave 2")]
        public float wave2Duration   = 60f;
        public float wave2Interval   = 1.8f;
        public float wave2Speed      = 1.8f;
        [Range(0f, 1f)]
        public float wave2ShootRatio = 0.5f;

        [Header("보스")]
        public GameObject bossPrefab;
        public Transform  bossSpawnPoint;

        [Header("소환 위치")]
        public float spawnYOffset = 1f;

        public bool IsBossSpawned  { get; private set; } = false;
        public bool IsBossDefeated { get; private set; } = false;

        private float spawnTopY, spawnMinX, spawnMaxX;
        private float currentInterval;
        private int   currentWave = 0;
        private bool  spawning    = false;
        private Coroutine spawnCoroutine;

        private ShootingGameController gameController;

        void Start()
        {
            gameController = GetComponent<ShootingGameController>();
            if (gameController == null)
                gameController = FindAnyObjectByType<ShootingGameController>();

            CalculateSpawnBounds();
            StartCoroutine(RunWaves());
        }

        // ── 웨이브 흐름 ──────────────────────────

        IEnumerator RunWaves()
        {
            StartWave(1);
            yield return new WaitForSeconds(wave1Duration);

            StartWave(2);
            yield return new WaitForSeconds(wave2Duration);

            StopSpawning();
            SpawnBoss();
        }

        void StartWave(int wave)
        {
            currentWave     = wave;
            currentInterval = wave == 1 ? wave1Interval : wave2Interval;

            SetEnemySpeed(wave == 1 ? wave1Speed : wave2Speed);
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

            float shootRatio = currentWave == 1 ? wave1ShootRatio : wave2ShootRatio;
            enemy.canShoot = Random.value < shootRatio;

            if (currentWave == 2 && Random.value > 0.5f)
                enemy.movementType = Enemy.MovementType.Sine;
        }

        void SetEnemySpeed(float speed)
        {
            foreach (var prefab in enemyPrefabs)
            {
                if (prefab == null) continue;
                Enemy e = prefab.GetComponent<Enemy>();
                if (e != null) e.moveSpeed = speed;
            }
        }

        // ── 보스 ─────────────────────────────────

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

            ShootingUIManager.Instance?.ShowWaveMessage("BOSS!", 2f);
        }

        void HandleBossDefeated()
        {
            IsBossDefeated = true;
            gameController?.OnGameClear();
        }

        // ── 유틸 ─────────────────────────────────

        void CalculateSpawnBounds()
        {
            Camera cam   = Camera.main;
            float depth  = Mathf.Abs(cam.transform.position.z);
            Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));

            spawnTopY = topRight.y + spawnYOffset;
            spawnMinX = bottomLeft.x + 0.5f;
            spawnMaxX = topRight.x   - 0.5f;
        }
    }
}
