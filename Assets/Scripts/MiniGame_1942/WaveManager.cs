using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 웨이브 진행 → 보스 등장 타이밍 관리
    public class WaveManager : MonoBehaviour
    {
        [Header("웨이브 설정")]
        public EnemySpawner enemySpawner;
        public GameObject bossPrefab;
        public Transform bossSpawnPoint;

        [Header("웨이브 시간 (초)")]
        public float wave1Duration = 60f;
        public float wave2Duration = 60f;

        public bool IsBossDefeated { get; private set; } = false;
        public bool IsBossSpawned  { get; private set; } = false;

        private ShootingGameController gameController;

        void Start()
        {
            gameController = GetComponent<ShootingGameController>();
            if (gameController == null)
                gameController = FindObjectOfType<ShootingGameController>();

            StartCoroutine(RunWaves());
        }

        IEnumerator RunWaves()
        {
            // Wave 1: 직선 적만, 느린 속도
            enemySpawner.SetWave(1);
            enemySpawner.StartSpawning();
            yield return new WaitForSeconds(wave1Duration);

            // Wave 2: 직선 + 사인 혼합
            enemySpawner.SetWave(2);
            yield return new WaitForSeconds(wave2Duration);

            // 보스 등장
            enemySpawner.StopSpawning();
            SpawnBoss();
        }

        void SpawnBoss()
        {
            IsBossSpawned = true;
            Vector3 spawnPos = bossSpawnPoint != null
                ? bossSpawnPoint.position
                : new Vector3(0f, 6f, 0f);

            GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            BossController boss = bossObj.GetComponent<BossController>();
            if (boss != null)
                boss.OnBossDefeated += HandleBossDefeated;
        }

        void HandleBossDefeated()
        {
            IsBossDefeated = true;
            gameController?.OnGameClear();
        }
    }
}
