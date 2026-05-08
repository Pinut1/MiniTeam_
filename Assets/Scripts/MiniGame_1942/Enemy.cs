using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 이동 패턴 / 총알 발사 / 피격 처리 / 화면 밖 삭제
    public class Enemy : MonoBehaviour
    {
        public enum MovementType { Straight, Sine, Horizontal }

        [Header("이동")]
        public MovementType movementType = MovementType.Straight;
        public float moveSpeed  = 2f;
        public float sineAmount = 1.5f;
        public float sineSpeed  = 2f;

        [Header("총알")]
        public bool canShoot = false;
        public GameObject enemyBulletPrefab;
        public float shootInterval = 2f;

        [Header("HP")]
        public int hp = 1;

        private float destroyY;
        private float startX;
        private float elapsed = 0f;
        private bool  isDead  = false;

        void Start()
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            destroyY = cam.ViewportToWorldPoint(new Vector3(0, 0, depth)).y - 1f;

            startX = transform.position.x;

            if (canShoot && enemyBulletPrefab != null)
                StartCoroutine(ShootRoutine());
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            Move();

            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }

        void Move()
        {
            switch (movementType)
            {
                case MovementType.Straight:
                    transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
                    break;

                case MovementType.Sine:
                    float offsetX = Mathf.Sin(elapsed * sineSpeed) * sineAmount;
                    Vector3 pos = transform.position;
                    pos.y -= moveSpeed * Time.deltaTime;
                    pos.x  = startX + offsetX;
                    transform.position = pos;
                    break;

                case MovementType.Horizontal:
                    transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
                    break;
            }
        }

        IEnumerator ShootRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(shootInterval);
                if (enemyBulletPrefab != null)
                    Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Applies one point of damage to the enemy and handles its death when HP reaches zero.
        /// </summary>
        /// <remarks>
        /// Decrements the enemy's HP by 1. If HP is less than or equal to 0, plays the enemy-death sound effect, awards 10 points, marks the enemy as dead to ignore further hits, and destroys the enemy GameObject.
        /// </remarks>
        public void TakeHit()
        {
            if (isDead) return;
            hp--;
            if (hp <= 0)
            {
                isDead = true;
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxEnemyDie);
                ShootingUIManager.Instance?.AddScore(10);
                Destroy(gameObject);
            }
        }
    }
}
