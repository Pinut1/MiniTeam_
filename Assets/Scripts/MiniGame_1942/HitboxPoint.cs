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

        /// <summary>
        /// When this component is disabled, reset the player's speed multiplier to 1 and disable the focus dot renderer if those components are present.
        /// </summary>
        void OnDisable()
        {
            if (playerController != null)
                playerController.currentSpeedMultiplier = 1f;
            if (dotRenderer != null)
                dotRenderer.enabled = false;
        }

        /// <summary>
        /// Invokes the player's hit handler when a collider tagged "Enemy" or "EnemyBullet" enters this hitbox trigger.
        /// </summary>
        /// <param name="other">The Collider2D that entered the trigger.</param>
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
                playerHit?.TakeHit();
        }
    }
}
