using System;
using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 모죠죠 보스 - HP / 공격 패턴 2개
    public class BossController : MonoBehaviour
    {
        [Header("HP")]
        public int maxHp = 30;

        [Header("이동")]
        public float moveSpeed = 1.5f;
        public float moveRange = 3f;   // 좌우 이동 범위

        [Header("패턴 1 - 직선 탄")]
        public GameObject bulletPrefab;
        public float pattern1Interval = 2f;

        [Header("패턴 2 - 3방향 산탄")]
        public float pattern2Interval = 4f;
        public float spreadAngle = 25f;

        public event Action OnBossDefeated;

        private int currentHp;
        private float startX;

        void Start()
        {
            currentHp = maxHp;
            startX = transform.position.x;
            StartCoroutine(MoveRoutine());
            StartCoroutine(Pattern1Routine());
            StartCoroutine(Pattern2Routine());
        }

        // 좌우 왕복 이동
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

        // 패턴 1: 직선 탄 1발
        IEnumerator Pattern1Routine()
        {
            yield return new WaitForSeconds(1f);
            while (true)
            {
                FireStraight();
                yield return new WaitForSeconds(pattern1Interval);
            }
        }

        // 패턴 2: 3방향 산탄
        IEnumerator Pattern2Routine()
        {
            yield return new WaitForSeconds(2.5f);
            while (true)
            {
                FireSpread();
                yield return new WaitForSeconds(pattern2Interval);
            }
        }

        void FireStraight()
        {
            if (bulletPrefab == null) return;
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        }

        void FireSpread()
        {
            if (bulletPrefab == null) return;
            float[] angles = { -spreadAngle, 0f, spreadAngle };
            foreach (float angle in angles)
            {
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                Instantiate(bulletPrefab, transform.position, rot);
            }
        }

        // Bullet.cs의 TakeHit 대신 보스는 여기서 처리
        public void TakeHit()
        {
            currentHp--;
            ShootingUIManager.Instance?.UpdateBossHp(currentHp, maxHp);

            if (currentHp <= 0)
            {
                OnBossDefeated?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
