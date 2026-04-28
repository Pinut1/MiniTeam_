using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("적 프리팹")]
        public GameObject[] enemyPrefabs;

        [Header("Wave 1 설정 (직선만, 느림)")]
        public float wave1Interval = 2.5f;
        public float wave1Speed    = 1.2f;

        [Header("Wave 2 설정 (직선+사인 혼합)")]
        public float wave2Interval = 1.8f;
        public float wave2Speed    = 1.8f;

        [Header("소환 위치")]
        public float spawnYOffset = 1f;

        private float spawnTopY, spawnMinX, spawnMaxX;
        private float currentInterval;
        private int   currentWave = 0;
        private bool  spawning    = false;
        private Coroutine spawnCoroutine;

        void Start()
        {
            CalculateSpawnBounds();
        }

        public void SetWave(int wave)
        {
            currentWave = wave;
            currentInterval = wave == 1 ? wave1Interval : wave2Interval;

            float speed = wave == 1 ? wave1Speed : wave2Speed;
            SetEnemySpeed(speed);

            ShootingUIManager.Instance?.ShowWaveMessage(
                wave == 1 ? "WAVE 1" : "WAVE 2", 2f);
        }

        public void StartSpawning()
        {
            spawning = true;
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
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

            float randomX = Random.Range(spawnMinX, spawnMaxX);
            Vector3 spawnPos = new Vector3(randomX, spawnTopY, 0f);

            int idx = Random.Range(0, enemyPrefabs.Length);
            GameObject obj = Instantiate(enemyPrefabs[idx], spawnPos, Quaternion.identity);

            if (currentWave == 2)
            {
                Enemy enemy = obj.GetComponent<Enemy>();
                if (enemy != null && Random.value > 0.5f)
                    enemy.movementType = Enemy.MovementType.Sine;
            }
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

        void CalculateSpawnBounds()
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));

            spawnTopY = topRight.y + spawnYOffset;
            spawnMinX = bottomLeft.x + 0.5f;
            spawnMaxX = topRight.x   - 0.5f;
        }
    }
}
