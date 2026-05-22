using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 편대원 개별 사격 컴포넌트 — PlayerController.Shoot() 호출 시 딜레이 후 발사
    // buttercup / bubbles / blossom GameObject에 부착
    public class FormationShooter : MonoBehaviour
    {
        public enum ShooterType { Buttercup, Bubbles, Blossom }

        [Header("설정")]
        public ShooterType shooterType;
        public Transform firePoint;

        [Header("발사 속도 (초, 클수록 느림)")]
        public float fireRate = 0.4f;

        [Header("프리팹")]
        public GameObject bulletPrefab;
        public GameObject piercingBulletPrefab; // 블로썸 전용

        private float nextFireTime = 0f;

        Transform Origin => firePoint != null ? firePoint : transform;

        public void TriggerFire()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + fireRate;

            switch (shooterType)
            {
                case ShooterType.Buttercup:
                    Spawn(bulletPrefab, Quaternion.identity);
                    break;

                case ShooterType.Bubbles:
                    Spawn(bulletPrefab, Quaternion.Euler(0f, 0f,  45f));
                    Spawn(bulletPrefab, Quaternion.Euler(0f, 0f, -45f));
                    break;

                case ShooterType.Blossom:
                    Spawn(piercingBulletPrefab, Quaternion.identity);
                    break;
            }
        }

        void Spawn(GameObject prefab, Quaternion rotation)
        {
            if (prefab == null) return;
            Instantiate(prefab, Origin.position, rotation);
        }
    }
}
