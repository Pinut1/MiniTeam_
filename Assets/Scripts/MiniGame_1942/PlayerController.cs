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

        private float minX, maxX, minY, maxY;
        private float nextFireTime = 0f;
        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation
                           | RigidbodyConstraints.FreezePositionZ;
            CalculateBounds();
        }

        /// <summary>
        /// Processes player input each frame: moves the player, constrains position within bounds, and fires bullets when the fire key is held and the firing cooldown has elapsed.
        /// </summary>
        /// <remarks>
        /// Uses a reduced fire interval of 0.02 seconds when <c>DebugRapidFire</c> is true; otherwise uses <c>fireRate</c>. When a shot is fired, <c>nextFireTime</c> is advanced by the effective fire interval.
        /// </remarks>
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
            Vector3 dir = new Vector3(h, v, 0f).normalized;
            rb.linearVelocity = dir * moveSpeed;
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
