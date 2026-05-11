using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 동방 스타일 피탄점 - 플레이어 자식 오브젝트에 부착
    // 실제 충돌 판정은 이 오브젝트의 작은 콜라이더만 담당
    public class HitboxPoint : MonoBehaviour
    {
        [Header("포커스 모드")]
        public float focusSpeedMultiplier = 0.5f;

        private PlayerHit playerHit;
        private PlayerController playerController;
        private SpriteRenderer dotRenderer;

        void Start()
        {
            playerHit        = GetComponentInParent<PlayerHit>();
            playerController = GetComponentInParent<PlayerController>();
            dotRenderer      = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            bool isFocus = Input.GetKey(KeyCode.LeftShift);

            // 무적 중에는 PlayerHit의 깜빡임이 dotRenderer를 제어하므로 건드리지 않음
            if (dotRenderer != null && playerHit != null && !playerHit.IsInvincible)
                dotRenderer.enabled = isFocus;

            if (playerController != null)
                playerController.currentSpeedMultiplier = isFocus ? focusSpeedMultiplier : 1f;

            // 피탄점 천천히 회전
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
                playerHit?.TakeHit();
        }

    }
}
