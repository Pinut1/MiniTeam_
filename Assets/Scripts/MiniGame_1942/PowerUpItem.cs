using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 적 사망 시 확률 드롭 — 플레이어가 먹으면 편대원 1명 복구
    public class PowerUpItem : MonoBehaviour
    {
        public float fallSpeed = 2.5f;

        private float destroyY;

        void Start()
        {
            Camera cam = Camera.main;
            float depth = Mathf.Abs(cam.transform.position.z);
            destroyY = cam.ViewportToWorldPoint(new Vector3(0, 0, depth)).y - 1f;
        }

        void Update()
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var fm = other.GetComponentInParent<FormationManager>()
                  ?? FindAnyObjectByType<FormationManager>();
            fm?.Recover();

            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxPowerUp);
            Destroy(gameObject);
        }
    }
}
