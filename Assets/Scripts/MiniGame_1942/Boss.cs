using System.Collections;
using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    // 모조조조 보스
    // Phase 1 (HP > 50%): 좌우 이동 + 3방향 사격
    // Phase 2 (HP <= 50%): 빠른 이동 + 5방향 사격 + 직하강 돌진
    public class Boss : MonoBehaviour
    {
        [Header("HP")]
        public int maxHp = 20;

        [Header("이동")]
        public float phase1Speed = 2.5f;
        public float phase2Speed = 4.5f;
        public float moveRange   = 3.5f;   // 좌우 이동 범위

        [Header("사격")]
        public GameObject bulletPrefab;
        public float phase1FireRate = 2.0f;
        public float phase2FireRate = 1.2f;
        public float bulletSpeed    = 5f;

        [Header("돌진 (Phase 2)")]
        public float chargeSpeed    = 8f;
        public float chargeInterval = 6f;

        private int currentHp;
        private bool isPhase2 = false;
        private bool isCharging = false;
        private float moveDir = 1f;
        private float leftLimit;
        private float rightLimit;
        private ShootingGameController gameController;

        void Start()
        {
            currentHp = maxHp;
            gameController = FindFirstObjectByType<ShootingGameController>();

            // 이동 한계 계산
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, depth));
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, depth));
            leftLimit  = bottomLeft.x + 1f;
            rightLimit = topRight.x   - 1f;

            StartCoroutine(FireRoutine());
            StartCoroutine(EnterRoutine());
        }

        // 등장 연출: 화면 위에서 중앙으로 천천히 내려옴
        IEnumerator EnterRoutine()
        {
            Vector3 target = new Vector3(0f, 3.5f, 0f);
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, 3f * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            StartCoroutine(MoveRoutine());
            if (!isPhase2)
                StartCoroutine(Phase2ChargeRoutine());
        }

        void Update()
        {
            CheckPhase();
        }

        void CheckPhase()
        {
            if (!isPhase2 && currentHp <= maxHp / 2)
            {
                isPhase2 = true;
                Debug.Log("[Boss] Phase 2 돌입");
            }
        }

        IEnumerator MoveRoutine()
        {
            while (true)
            {
                if (isCharging) { yield return null; continue; }

                float speed = isPhase2 ? phase2Speed : phase1Speed;
                transform.position += Vector3.right * moveDir * speed * Time.deltaTime;

                if (transform.position.x >= rightLimit) moveDir = -1f;
                if (transform.position.x <= leftLimit)  moveDir =  1f;

                yield return null;
            }
        }

        IEnumerator FireRoutine()
        {
            yield return new WaitForSeconds(1.5f); // 등장 직후 약간 딜레이

            while (true)
            {
                float rate = isPhase2 ? phase2FireRate : phase1FireRate;
                yield return new WaitForSeconds(rate);

                int count = isPhase2 ? 5 : 3;
                FireSpread(count);
            }
        }

        // Phase 2: 일정 간격으로 플레이어 방향 돌진
        IEnumerator Phase2ChargeRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => isPhase2);
                yield return new WaitForSeconds(chargeInterval);

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) continue;

                isCharging = true;
                Vector3 dir = (player.transform.position - transform.position).normalized;
                float elapsed = 0f;

                while (elapsed < 0.6f)
                {
                    transform.position += dir * chargeSpeed * Time.deltaTime;
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // 돌진 후 원래 위치로 복귀
                Vector3 returnPos = new Vector3(transform.position.x, 3.5f, 0f);
                while (Vector3.Distance(transform.position, returnPos) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, returnPos, phase1Speed * 2f * Time.deltaTime);
                    yield return null;
                }

                isCharging = false;
            }
        }

        void FireSpread(int count)
        {
            if (bulletPrefab == null) return;

            float angleStep = 20f;
            float startAngle = -(count - 1) * angleStep * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.down;

                GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                var bb = b.GetComponent<BossBullet>();
                if (bb != null)
                {
                    bb.speed = bulletSpeed;
                    bb.SetDirection(dir);
                }
            }
        }

        public void TakeHit()
        {
            currentHp--;
            Debug.Log($"[Boss] 피격 - 남은 HP: {currentHp}/{maxHp}");

            if (currentHp <= 0)
            {
                Debug.Log("[Boss] 격파!");
                gameController?.OnBossDefeated();
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                Destroy(other.gameObject);
                TakeHit();
            }
        }
    }
}
