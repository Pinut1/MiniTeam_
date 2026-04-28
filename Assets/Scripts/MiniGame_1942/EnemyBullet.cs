using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 적 총알 이동 / 화면 밖 삭제
    // 플레이어 충돌 처리는 PlayerHit.cs에서 Tag로 감지
    public class EnemyBullet : MonoBehaviour
    {
        public float speed = 5f;

        private float destroyY;

        void Start()
        {
            gameObject.tag = "EnemyBullet";

            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            destroyY = cam.ViewportToWorldPoint(new Vector3(0, 0, depth)).y - 1f;
        }

        void Update()
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);

            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }
    }
}
