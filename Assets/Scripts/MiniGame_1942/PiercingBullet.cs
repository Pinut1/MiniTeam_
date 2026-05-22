using System.Collections.Generic;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 블로썸 전용 관통탄 — 적을 뚫고 지나가며 동일 적 중복 피격 방지
    public class PiercingBullet : MonoBehaviour
    {
        public float speed = 14f;

        private float destroyY;
        private readonly HashSet<int> hitIds = new();

        void Start()
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            destroyY = cam.ViewportToWorldPoint(new Vector3(0, 1, depth)).y + 1f;
        }

        void Update()
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            if (transform.position.y > destroyY)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            int id = other.gameObject.GetInstanceID();
            if (hitIds.Contains(id)) return;
            hitIds.Add(id);

            if (other.CompareTag("Enemy"))
                other.GetComponent<Enemy>()?.TakeHit();
            else if (other.CompareTag("Boss"))
                other.GetComponent<BossController>()?.TakeHit();
        }
    }
}
