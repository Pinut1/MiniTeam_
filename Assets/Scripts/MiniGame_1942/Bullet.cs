using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 이동 / 적 충돌 시 삭제
    public class Bullet : MonoBehaviour
    {
        public float speed = 10f;

        private float destroyY;

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

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                other.GetComponent<Enemy>()?.TakeHit();
                Destroy(gameObject);
            }
        }
    }
}
