using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("적 프리팹")]
        public GameObject[] enemyPrefabs;

        [Header("소환 설정")]
        public float spawnInterval = 1.5f;
        public float spawnYOffset  = 1f;

        [Header("난이도 (시간 경과 자동 증가)")]
        public float difficultyInterval = 15f;
        public float minInterval        = 0.5f;

        private float spawnTopY;
        private float spawnMinX;
        private float spawnMaxX;

        void Start()
        {
            CalculateSpawnBounds();
            StartCoroutine(SpawnLoop());
            StartCoroutine(DifficultyLoop());
        }

        IEnumerator SpawnLoop()
        {
            while (true)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        IEnumerator DifficultyLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(difficultyInterval);
                spawnInterval = Mathf.Max(minInterval, spawnInterval - 0.2f);
                Debug.Log($"[Spawner] 난이도 증가 - 소환 간격: {spawnInterval:F1}s");
            }
        }

        void SpawnEnemy()
        {
            float randomX = Random.Range(spawnMinX, spawnMaxX);
            Vector3 spawnPos = new Vector3(randomX, spawnTopY, 0f);

            int idx = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[idx], spawnPos, Quaternion.identity);
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
