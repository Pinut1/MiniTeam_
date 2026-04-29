using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class WaveConfig
        {
            public string waveName;
            public float duration = 45f;
            public float spawnInterval = 1.2f;

            [Header("적 프리팹 (Enemy 컴포넌트 필수)")]
            public GameObject[] straightEnemies;    // 직진
            public GameObject[] sineEnemies;        // 사인파
            public GameObject[] horizontalEnemies;  // 수평
            public GameObject[] shootingEnemies;    // 사격형

            [Range(0f, 1f)]
            public float shootingRatio = 0.3f;      // 사격 적 비율
        }

        [Header("웨이브 설정")]
        public WaveConfig[] waves;

        [Header("보스")]
        public GameObject bossPrefab;
        public Transform bossSpawnPoint;

        private ShootingGameController gameController;
        private int currentWave = 0;
        private bool bossSpawned = false;

        public int CurrentWave => currentWave;
        public bool IsBossPhase => bossSpawned;

        void Awake()
        {
            gameController = GetComponent<ShootingGameController>();
        }

        public void StartWaves()
        {
            StartCoroutine(WaveRoutine());
        }

        IEnumerator WaveRoutine()
        {
            for (int i = 0; i < waves.Length; i++)
            {
                currentWave = i + 1;
                var wave = waves[i];
                Debug.Log($"[Wave] {wave.waveName} 시작");

                yield return StartCoroutine(RunWave(wave));

                Debug.Log($"[Wave] {wave.waveName} 종료");
                yield return new WaitForSeconds(1.5f); // 웨이브 사이 짧은 휴식
            }

            SpawnBoss();
        }

        IEnumerator RunWave(WaveConfig wave)
        {
            float elapsed = 0f;

            while (elapsed < wave.duration)
            {
                SpawnEnemy(wave);
                yield return new WaitForSeconds(wave.spawnInterval);
                elapsed += wave.spawnInterval;
            }
        }

        void SpawnEnemy(WaveConfig wave)
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));

            float spawnX = Random.Range(bottomLeft.x + 0.5f, topRight.x - 0.5f);
            float spawnY = topRight.y + 1f;
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject prefab = PickEnemyPrefab(wave);
            if (prefab != null)
                Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        GameObject PickEnemyPrefab(WaveConfig wave)
        {
            // 사격 비율에 따라 사격 적 / 일반 적 결정
            bool isShooting = Random.value < wave.shootingRatio
                              && wave.shootingEnemies != null
                              && wave.shootingEnemies.Length > 0;

            if (isShooting)
                return wave.shootingEnemies[Random.Range(0, wave.shootingEnemies.Length)];

            // 일반 적: 직진/사인/수평 중 랜덤
            int type = Random.Range(0, 3);
            return type switch
            {
                0 when wave.straightEnemies?.Length   > 0 => wave.straightEnemies[Random.Range(0, wave.straightEnemies.Length)],
                1 when wave.sineEnemies?.Length       > 0 => wave.sineEnemies[Random.Range(0, wave.sineEnemies.Length)],
                2 when wave.horizontalEnemies?.Length > 0 => wave.horizontalEnemies[Random.Range(0, wave.horizontalEnemies.Length)],
                _ => wave.straightEnemies?.Length > 0 ? wave.straightEnemies[0] : null,
            };
        }

        void SpawnBoss()
        {
            if (bossPrefab == null) return;

            bossSpawned = true;
            Vector3 pos = bossSpawnPoint != null
                ? bossSpawnPoint.position
                : new Vector3(0f, 6f, 0f);

            Debug.Log("[Wave] 보스 등장!");
            Instantiate(bossPrefab, pos, Quaternion.identity);
        }
    }
}
