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

        private Coroutine moveCoroutine;
        private readonly List<Coroutine> patternCoroutines = new();

        /// <summary>
        /// Initializes the boss's runtime state and begins movement and firing routines.
        /// </summary>
        /// <remarks>
        /// Sets current HP to the configured maximum, records the starting X position, starts the movement coroutine, and starts all firing pattern coroutines.
        /// </remarks>
        void Start()
        {
            currentHp = maxHp;
            startX    = transform.position.x;

            moveCoroutine = StartCoroutine(MoveRoutine());
            StartAllPatterns();
        }

        /// <summary>
        /// Starts the boss's pattern coroutines and stores their coroutine references for later management.
        /// </summary>
        /// <remarks>
        /// Adds the running coroutine handles for Pattern1Routine and Pattern2Routine to <c>patternCoroutines</c> so they can be stopped or cleared later.
        /// </remarks>

        void StartAllPatterns()
        {
            patternCoroutines.Add(StartCoroutine(Pattern1Routine()));
            patternCoroutines.Add(StartCoroutine(Pattern2Routine()));
        }

        /// <summary>
        /// Stops any active pattern coroutines tracked by this controller and clears the tracking list.
        /// </summary>
        /// <remarks>
        /// Null coroutine references are ignored; after this call the internal pattern coroutine list will be empty.
        /// </remarks>
        void StopAllPatterns()
        {
            foreach (var c in patternCoroutines)
                if (c != null) StopCoroutine(c);
            patternCoroutines.Clear();
        }

        /// <summary>
        /// Moves the GameObject horizontally along a sinusoidal patrol centered on its starting X position; runs continuously while the coroutine is active.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> that advances the patrol each frame.</returns>

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

        /// <summary>
        /// Controls the boss's straight-shot firing pattern, emitting bullets at a phase-dependent interval.
        /// </summary>
        /// <returns>An IEnumerator for Unity's coroutine runner that waits 1 second, then repeatedly fires a straight shot and waits for the current phase's interval.</returns>

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

        /// <summary>
        /// Runs the boss's spread-shot firing pattern, periodically firing a spread of bullets while adapting count and interval for phase 2.
        /// </summary>
        /// <returns>An IEnumerator that, when executed as a Unity coroutine, waits an initial 2.5 seconds then repeatedly fires spread shots using the phase-appropriate count and interval.</returns>

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

        /// <summary>
        /// Instantiates a straight-moving bullet at the boss's current position using the configured bullet prefab.
        /// </summary>
        /// <remarks>
        /// If <c>bulletPrefab</c> is null, the method does nothing.
        /// </remarks>

        void FireStraight()
        {
            if (bulletPrefab == null) return;
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Fires a volley of enemy bullets in a symmetric angular spread centered downward.
        /// </summary>
        /// <param name="count">Number of bullets to spawn in the spread. If less than 1, no bullets are created.</param>
        /// <remarks>
        /// Each adjacent bullet is separated by <c>spreadAngle</c> degrees and the spread is centered on the downward direction.
        /// If <c>bossBulletPrefab</c> is not set, the method does nothing. For each instantiated bullet, if it has a <c>BossBullet</c>
        /// component, <c>SetDirection</c> is called with the computed firing direction.
        /// </remarks>
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
        /// Applies one hit to the boss: decrements health unless invincible, updates UI, handles defeat, and initiates the phase-2 transition when health falls to half or below.
        /// </summary>
        /// <remarks>
        /// If health reaches zero or less, awards score, invokes <c>OnBossDefeated</c>, and destroys the boss GameObject. If the boss is already invincible, this method has no effect.
        /// </remarks>

        public void TakeHit()
        {
            if (isInvincible) return;

            currentHp--;
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);

            if (currentHp <= 0)
            {
                ShootingUIManager.Instance?.AddScore(200);
                OnBossDefeated?.Invoke();
                Destroy(gameObject);
                return;
            }

            if (!isPhase2 && currentHp <= maxHp / 2)
                StartCoroutine(EnterPhase2());
        }

        /// <summary>
        /// Forces the boss to enter phase 2 immediately.
        /// </summary>
        /// <remarks>
        /// If the boss is already in phase 2, this method does nothing. Otherwise it sets the boss's HP to one less than half of max HP, updates the boss HP UI if available, and initiates the phase-2 transition.
        /// </remarks>
        public void ForcePhase2()
        {
            if (isPhase2) return;
            currentHp = maxHp / 2 - 1;
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);
            StartCoroutine(EnterPhase2());
        }

        /// <summary>
        /// Performs the boss's transition into phase 2, including visual cue and behavior changes.
        /// </summary>
        /// <remarks>
        /// Sets the boss to phase 2 and makes it temporarily invincible, stops firing patterns, fades the boss sprite from white to red over <c>phase2TransitionTime</c>, increases movement speed to <c>phase2MoveSpeed</c>, restarts movement, clears invincibility, and restarts firing patterns.
        /// </remarks>
        /// <returns>An enumerator that executes the phase-2 transition sequence.</returns>

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
