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

        public bool IsBossSpawned  { get; private set; } = false;
        public bool IsBossDefeated { get; private set; } = false;

        private float spawnTopY, spawnMinX, spawnMaxX;
        private float currentInterval;
        private int   currentWave = 0;
        private bool  spawning    = false;
        private bool  gameStopped = false;
        private Coroutine spawnCoroutine;
        private Coroutine waveCoroutine;

        private ShootingGameController gameController;

        /// <summary>
        /// Initializes runtime state for the WaveManager: locates the ShootingGameController, calculates enemy spawn bounds, and begins the wave sequence.
        /// </summary>
        /// <remarks>
        /// If a ShootingGameController is not attached to the same GameObject, one will be located in the scene. The running wave coroutine is stored so it can be stopped later.
        /// </remarks>
        void Start()
        {
            gameController = GetComponent<ShootingGameController>();
            if (gameController == null)
                gameController = FindAnyObjectByType<ShootingGameController>();

            CalculateSpawnBounds();
            waveCoroutine = StartCoroutine(RunWaves());
        }

        /// <summary>
        /// Controls the timed progression of Wave 1 and Wave 2, then stops enemy spawning and triggers the boss spawn when appropriate.
        /// </summary>
        /// <returns>An IEnumerator that performs the sequential wait-driven wave flow and finalizes by stopping spawning and (if the game is not stopped) spawning the boss.</returns>

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

        /// <summary>
        /// Activates the specified wave configuration, updates the spawn interval, displays the wave UI message, and begins enemy spawning.
        /// </summary>
        /// <param name="wave">Wave number to start; if `1` uses Wave 1 settings, otherwise uses Wave 2 settings.</param>
        void StartWave(int wave)
        {
            currentWave     = wave;
            currentInterval = wave == 1 ? wave1Interval : wave2Interval;

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

        /// <summary>
        /// Instantiates a random enemy at a random horizontal position along the spawn top and configures its behavior for the current wave.
        /// </summary>
        /// <remarks>
        /// If no enemy prefabs are configured the method does nothing. If the instantiated object lacks an Enemy component the object is left as-is and no configuration is applied.
        /// </remarks>
        /// <returns></returns>
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

        /// <summary>
        /// Immediately ends the current waves and spawns the boss for debugging purposes.
        /// </summary>

        public void DebugSkipToBoss()
        {
            StopGame();
            gameStopped = false;
            SpawnBoss();
        }

        /// <summary>
        /// Marks the boss as spawned and instantiates the boss GameObject at the configured spawn point or a default position.
        /// </summary>
        /// <remarks>
        /// Subscribes to the boss's OnBossDefeated event to handle defeat and requests the UI to display a "BOSS!" message.
        /// </remarks>
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

        /// <summary>
        /// Marks the boss as defeated and notifies the game controller to trigger game-clear handling.
        /// </summary>
        /// <remarks>
        /// If a game controller is available, its <c>OnGameClear</c> method is invoked.
        /// </remarks>
        void HandleBossDefeated()
        {
            IsBossDefeated = true;
            gameController?.OnGameClear();
        }

        /// <summary>
        /// Stops wave progression and enemy spawning, and marks the game as stopped.
        /// </summary>
        /// <remarks>
        /// Sets the internal stopped flag, halts the active spawn loop, and stops the running wave coroutine if present.
        /// </remarks>

        public void StopGame()
        {
            gameStopped = true;
            StopSpawning();
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        }

        /// <summary>
        /// Computes world-space spawn bounds from the main camera's viewport and stores them for spawning.
        /// </summary>
        /// <remarks>
        /// Sets <c>spawnTopY</c>, <c>spawnMinX</c>, and <c>spawnMaxX</c> using the camera's top/right and bottom/left viewport corners converted to world coordinates. The vertical bound includes <c>spawnYOffset</c>, and horizontal bounds include a 0.5 unit padding from the screen edges.
        /// </remarks>

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
