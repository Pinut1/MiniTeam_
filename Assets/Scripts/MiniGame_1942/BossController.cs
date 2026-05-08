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

        [Header("패턴 1 - 직선 탄")]
        public GameObject bulletPrefab;
        public float pattern1Interval = 2f;

        [Header("패턴 2 - 3방향 산탄")]
        public GameObject bossBulletPrefab;
        public float pattern2Interval = 4f;
        public float spreadAngle = 25f;

        [Header("2페이즈")]
        public float phase2MoveSpeed     = 2.5f;
        public float phase2Pattern1Interval = 1f;
        public float phase2Pattern2Interval = 2.5f;
        public int   phase2SpreadCount   = 5;
        public float phase2TransitionTime = 0.8f;

        public event Action OnBossDefeated;

        public bool IsPhase2 => isPhase2;

        private int   currentHp;
        private float startX;
        private bool  isPhase2      = false;
        private bool  isInvincible  = false;
        private bool  isDefeated    = false;

        private Coroutine moveCoroutine;
        private readonly List<Coroutine> patternCoroutines = new();

        void Start()
        {
            currentHp = maxHp;
            startX    = transform.position.x;

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
                yield return null;
            }
        }

        // ── 패턴 1: 직선탄 ───────────────────────

        IEnumerator Pattern1Routine()
        {
            yield return new WaitForSeconds(1f);
            float interval = isPhase2 ? phase2Pattern1Interval : pattern1Interval;
            while (true)
            {
                FireStraight();
                yield return new WaitForSeconds(interval);
            }
        }

        // ── 패턴 2: 산탄 ─────────────────────────

        IEnumerator Pattern2Routine()
        {
            yield return new WaitForSeconds(2.5f);
            int   count    = isPhase2 ? phase2SpreadCount    : 3;
            float interval = isPhase2 ? phase2Pattern2Interval : pattern2Interval;
            while (true)
            {
                FireSpread(count);
                yield return new WaitForSeconds(interval);
            }
        }

        // ── 발사 ─────────────────────────────────

        void FireStraight()
        {
            if (bulletPrefab == null) return;
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        }

        void FireSpread(int count)
        {
            if (bossBulletPrefab == null) return;

            float totalAngle = spreadAngle * (count - 1);
            float startAngle = -totalAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 dir = Quaternion.Euler(0f, 0f, angle) * Vector3.down;
                GameObject b = Instantiate(bossBulletPrefab, transform.position, Quaternion.identity);
                b.GetComponent<BossBullet>()?.SetDirection(dir);
            }
        }

        /// <summary>
        /// Apply one point of damage to the boss and handle hit effects, phase transition, and defeat.
        /// </summary>
        /// <remarks>
        /// If the boss is invincible or already defeated, the call is ignored. Otherwise the boss's HP is decremented and clamped to the range [0, maxHp], a hit sound is played, and the boss HP UI is updated. If HP reaches 0 the boss is marked defeated, awards 200 score, invokes OnBossDefeated, and the GameObject is destroyed. If HP falls to half or below and the boss is not yet in phase 2, begins the phase‑2 transition.
        /// </remarks>

        public void TakeHit()
        {
            if (isInvincible || isDefeated) return;

            currentHp = Mathf.Clamp(currentHp - 1, 0, maxHp);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxBossHit);
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);

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

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

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
