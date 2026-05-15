using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class BossController : MonoBehaviour
    {
        [Header("HP")]
        public int maxHp = 30;

        [Header("이동")]
        public float moveSpeed = 1.5f;
        public float moveRange = 3f;

        [Header("탄 프리팹")]
        [UnityEngine.Serialization.FormerlySerializedAs("bulletPrefab")]
        public GameObject bossBulletPrefab;

        [Header("패턴 1 - 플레이어 조준 산탄")]
        public float pattern1Interval = 2f;
        public int   pattern1Count    = 3;
        public float spreadAngle      = 20f;

        [Header("패턴 2 - 전방위 원형탄")]
        public float pattern2Interval    = 5f;
        public int   pattern2CircleCount = 8;

        [Header("2페이즈")]
        public float phase2MoveSpeed       = 2.5f;
        public float phase2Pattern1Interval = 1f;
        public int   phase2Pattern1Count   = 5;
        public float phase2Pattern2Interval = 3f;
        public int   phase2CircleCount     = 12;
        public float phase2TransitionTime  = 0.8f;

        public event Action OnBossDefeated;

        public bool IsPhase2 => isPhase2;

        private int   currentHp;
        private float startX;
        private bool  isPhase2      = false;
        private bool  isInvincible  = false;
        private bool  isDefeated    = false;

        [Header("스프라이트")]
        public Sprite spriteIdle;
        public Sprite spriteLeft;
        public Sprite spriteRight;

        private SpriteRenderer sr;
        private float prevX;
        private Transform playerTransform;

        private Coroutine moveCoroutine;
        private readonly List<Coroutine> patternCoroutines = new();

        void Start()
        {
            currentHp       = maxHp;
            startX          = transform.position.x;
            prevX           = startX;
            sr              = GetComponentInChildren<SpriteRenderer>();
            playerTransform = GameObject.FindWithTag("Player")?.transform;

            moveCoroutine = StartCoroutine(MoveRoutine());
            StartAllPatterns();
        }

        // ── 패턴 코루틴 관리 ──────────────────────

        void StartAllPatterns()
        {
            patternCoroutines.Add(StartCoroutine(Pattern1Routine()));
            patternCoroutines.Add(StartCoroutine(Pattern2Routine()));
        }

        void StopAllPatterns()
        {
            foreach (var c in patternCoroutines)
                if (c != null) StopCoroutine(c);
            patternCoroutines.Clear();
        }

        // ── 이동 ─────────────────────────────────

        IEnumerator MoveRoutine()
        {
            float elapsed = 0f;
            while (true)
            {
                elapsed += Time.deltaTime;
                float x = startX + Mathf.Sin(elapsed * moveSpeed) * moveRange;
                transform.position = new Vector3(x, transform.position.y, 0f);

                float dx = x - prevX;
                if (sr != null)
                {
                    if (Mathf.Abs(dx) < 0.001f) sr.sprite = spriteIdle;
                    else if (dx < 0f)           sr.sprite = spriteLeft;
                    else                        sr.sprite = spriteRight;
                }
                prevX = x;

                yield return null;
            }
        }

        // ── 패턴 1: 플레이어 조준 산탄 ──────────────

        IEnumerator Pattern1Routine()
        {
            yield return new WaitForSeconds(1f);
            while (true)
            {
                int   count    = isPhase2 ? phase2Pattern1Count    : pattern1Count;
                float interval = isPhase2 ? phase2Pattern1Interval : pattern1Interval;
                FireAimed(count);
                yield return new WaitForSeconds(interval);
            }
        }

        // ── 패턴 2: 전방위 원형탄 ────────────────

        IEnumerator Pattern2Routine()
        {
            yield return new WaitForSeconds(2.5f);
            while (true)
            {
                int   count    = isPhase2 ? phase2CircleCount      : pattern2CircleCount;
                float interval = isPhase2 ? phase2Pattern2Interval : pattern2Interval;
                FireCircle(count);
                yield return new WaitForSeconds(interval);
            }
        }

        // ── 발사 ─────────────────────────────────

        // 플레이어 방향을 중심으로 count발 부채꼴 발사
        void FireAimed(int count)
        {
            if (bossBulletPrefab == null) return;

            Vector3 aimDir = playerTransform != null
                ? (playerTransform.position - transform.position).normalized
                : Vector3.down;

            float totalAngle = spreadAngle * (count - 1);
            float startAngle = -totalAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 dir = Quaternion.Euler(0f, 0f, angle) * aimDir;
                GameObject b = Instantiate(bossBulletPrefab, transform.position, Quaternion.identity);
                b.GetComponent<BossBullet>()?.SetDirection(dir);
            }
        }

        // 360도 균등 분할 원형 발사
        void FireCircle(int count)
        {
            if (bossBulletPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                float angle = 360f / count * i;
                Vector3 dir = Quaternion.Euler(0f, 0f, angle) * Vector3.down;
                GameObject b = Instantiate(bossBulletPrefab, transform.position, Quaternion.identity);
                b.GetComponent<BossBullet>()?.SetDirection(dir);
            }
        }

        // ── 피격 ─────────────────────────────────

        public void TakeHit()
        {
            if (isInvincible || isDefeated) return;

            currentHp = Mathf.Clamp(currentHp - 1, 0, maxHp);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxBossHit);
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);
            ShootingUIManager.Instance?.AddSpecialGauge(3f);
            StartCoroutine(BossHitFlash());

            if (currentHp <= 0)
            {
                isDefeated = true;
                ShootingUIManager.Instance?.AddScore(200);
                OnBossDefeated?.Invoke();
                Destroy(gameObject);
                return;
            }

            if (!isPhase2 && currentHp <= maxHp / 2)
                StartCoroutine(EnterPhase2());
        }

        IEnumerator BossHitFlash()
        {
            if (sr == null) yield break;
            Color current = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            sr.color = current;
        }

        public void ForcePhase2()
        {
            if (isPhase2) return;
            currentHp = maxHp / 2 - 1;
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);
            StartCoroutine(EnterPhase2());
        }

        // ── 2페이즈 진입 연출 ─────────────────────

        IEnumerator EnterPhase2()
        {
            isPhase2     = true;
            isInvincible = true;

            StopAllPatterns();

            // 빨간색으로 깜빡이며 전환
            float elapsed = 0f;
            while (elapsed < phase2TransitionTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / phase2TransitionTime;
                if (sr != null)
                    sr.color = Color.Lerp(Color.white, Color.red, t);
                yield return null;
            }

            if (sr != null) sr.color = Color.red;

            // 이동속도 증가
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveSpeed     = phase2MoveSpeed;
            moveCoroutine = StartCoroutine(MoveRoutine());

            isInvincible = false;
            StartAllPatterns();
        }
    }
}
