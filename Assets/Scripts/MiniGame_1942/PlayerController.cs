using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 이동 / 사격만 담당
    // 피격 처리 → PlayerHit.cs
    // 편대원 관리 → FormationManager.cs
    public class PlayerController : MonoBehaviour
    {
        [Header("이동")]
        public float moveSpeed = 5f;

        [Header("사격")]
        public GameObject bulletPrefab;
        public Transform firePoint;
        public float fireRate = 0.15f;

        public bool DebugRapidFire = false;

        [Header("필살기 파티클")]
        public GameObject bombParticlePrefab;

        [HideInInspector] public float currentSpeedMultiplier = 1f;

        private float minX, maxX, minY, maxY;
        private float nextFireTime = 0f;
        private Rigidbody2D rb;
        private FormationShooter[] formationShooters;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            CalculateBounds();
            formationShooters = GetComponentsInChildren<FormationShooter>(includeInactive: true);
        }

        void Update()
        {
            Move();
            ClampPosition();

            float currentFireRate = DebugRapidFire ? 0.02f : fireRate;
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + currentFireRate;
            }

            if (Input.GetKeyDown(KeyCode.Z) && ShootingUIManager.Instance != null && ShootingUIManager.Instance.IsSpecialReady)
            {
                if (ShootingUIManager.Instance.UseSpecial())
                    FireBomb();
            }
        }

        void FireBomb()
        {
            ShootingUIManager.SetBombActive(true);

            foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (enemy.explosionPrefab != null)
                    Instantiate(enemy.explosionPrefab, enemy.transform.position, Quaternion.identity);
                Destroy(enemy.gameObject);
            }

            foreach (var bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
                Destroy(bullet);

            ShootingUIManager.SetBombActive(false);

            if (bombParticlePrefab != null)
            {
                Camera cam   = Camera.main;
                float depth  = Mathf.Abs(cam.transform.position.z);
                Vector3 center;

                var wm = FindAnyObjectByType<WaveManager>();
                if (wm != null && wm.gameAreaRect != null)
                {
                    Vector3[] corners = new Vector3[4];
                    wm.gameAreaRect.GetWorldCorners(corners);
                    Vector3 screenCenter = new Vector3(
                        (corners[0].x + corners[2].x) * 0.5f,
                        (corners[0].y + corners[2].y) * 0.5f,
                        depth);
                    center = cam.ScreenToWorldPoint(screenCenter);
                }
                else
                {
                    center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
                }

                var particle = Instantiate(bombParticlePrefab, center, Quaternion.identity);
                Destroy(particle, 2f);
            }

            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxPlayerShoot);
        }

        void Move()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(h, v).normalized;
            rb.linearVelocity = dir * moveSpeed * currentSpeedMultiplier;
        }

        void ClampPosition()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            pos.z = 0f;
            transform.position = pos;
        }

        void Shoot()
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxPlayerShoot);

            foreach (var shooter in formationShooters)
                shooter.TriggerFire();
        }

        void CalculateBounds()
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));

            float margin = 0.3f;
            minX = bottomLeft.x + margin;
            maxX = topRight.x   - margin;
            minY = bottomLeft.y + margin;
            maxY = topRight.y   - margin;
        }
    }
}
