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

        [HideInInspector] public float currentSpeedMultiplier = 1f;

        private float minX, maxX, minY, maxY;
        private float nextFireTime = 0f;
        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            CalculateBounds();
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
