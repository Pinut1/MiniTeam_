using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 보스 확산탄 — 방향 지정 가능
    // Boss.cs의 FireSpread에서 SetDirection() 호출 후 사용
    public class BossBullet : MonoBehaviour
    {
        public float speed = 5f;

        private Vector3 direction = Vector3.down;
        private float destroyY;

        void Start()
        {
            gameObject.tag = "EnemyBullet";

            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            destroyY = cam.ViewportToWorldPoint(new Vector3(0, 0, depth)).y - 1f;
        }

        public void SetDirection(Vector3 dir)
        {
            direction = dir.normalized;
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;

            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }
    }
}
