using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 이동 패턴 / 총알 발사 / 피격 처리 / 화면 밖 삭제
    public class Enemy : MonoBehaviour
    {
        public enum MovementType { Straight, Sine, SineReverse, Horizontal }
        public enum ShootType   { None, Single, Burst, Spread }

        [Header("이동")]
        public MovementType movementType = MovementType.Straight;
        public float moveSpeed  = 2f;
        public float sineAmount = 1.5f;
        public float sineSpeed  = 2f;

        [Header("총알")]
        public ShootType  shootType       = ShootType.None;
        public GameObject enemyBulletPrefab;
        public float      shootInterval   = 2f;
        public float      spreadAngle     = 20f; // Spread 3갈래 각도
        public float      burstDelay      = 0.15f; // Burst 연발 간격

        [Header("HP")]
        public int hp = 1;

        [Header("이펙트")]
        public GameObject explosionPrefab;

        private float destroyY;
        private float startX;
        private float elapsed = 0f;
        private bool  isDead  = false;

        private WaveManager waveManager;

        void Start()
        {
            waveManager = FindAnyObjectByType<WaveManager>();

            if (waveManager != null)
            {
                destroyY = waveManager.DestroyBoundsY;
            }
            else
            {
                Camera cam = Camera.main;
                float depth = Mathf.Abs(cam.transform.position.z);
                destroyY = cam.ViewportToWorldPoint(new Vector3(0, 0, depth)).y - 1f;
            }

            startX = transform.position.x;

            if (shootType != ShootType.None && enemyBulletPrefab != null)
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
                case MovementType.SineReverse:
                    float sign    = movementType == MovementType.SineReverse ? -1f : 1f;
                    float offsetX = Mathf.Sin(elapsed * sineSpeed) * sineAmount * sign;
                    Vector3 pos   = transform.position;
                    pos.y -= moveSpeed * Time.deltaTime;
                    pos.x  = startX + offsetX;
                    if (waveManager != null)
                        pos.x = Mathf.Clamp(pos.x, waveManager.SpawnMinX, waveManager.SpawnMaxX);
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
                switch (shootType)
                {
                    case ShootType.Single: FireSingle(Vector3.down); break;
                    case ShootType.Burst:  yield return StartCoroutine(FireBurst()); break;
                    case ShootType.Spread: FireSpread(); break;
                }
            }
        }

        void FireSingle(Vector3 dir)
        {
            var b = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            b.GetComponent<EnemyBullet>()?.SetDirection(dir);
        }

        IEnumerator FireBurst()
        {
            FireSingle(Vector3.down);
            yield return new WaitForSeconds(burstDelay);
            FireSingle(Vector3.down);
        }

        void FireSpread()
        {
            for (int i = -1; i <= 1; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, 0f, spreadAngle * i) * Vector3.down;
                FireSingle(dir);
            }
        }

        public void TakeHit()
        {
            if (isDead) return;
            hp--;

            if (hp <= 0)
            {
                isDead = true;
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxEnemyDie);
                ShootingUIManager.Instance?.AddScore(10);
                if (!ShootingUIManager.IsBombActive)
                    ShootingUIManager.Instance?.AddSpecialGauge(10f);

                if (explosionPrefab != null)
                    Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxEnemyHit);
                StartCoroutine(HitFlash());
            }
        }

        IEnumerator HitFlash()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            sr.color = original;
        }
    }
}
