using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 맵에서 플레이어 키보드 이동 담당
    public class PlayerMapController : MonoBehaviour
    {
        public float moveSpeed = 4f;

        public bool IsControllable { get; private set; } = true;

        public void SetControllable(bool value) => IsControllable = value;

        private Rigidbody2D rb;
        private Animator anim;

        // 마지막 이동 방향 기억 (정지 시 해당 방향 유지)
        private Vector2 lastDir = Vector2.down;

        void Start()
        {
            rb   = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
            }

            SetAnimDir(lastDir, false);
        }

        void Update()
        {
            if (!IsControllable)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                SetAnimDir(lastDir, false);
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // 포켓몬 스타일: 4방향 우선순위 (대각 입력 시 수평 우선)
            Vector2 dir = Vector2.zero;
            if (h != 0)      dir = new Vector2(h > 0 ? 1 : -1, 0);
            else if (v != 0) dir = new Vector2(0, v > 0 ? 1 : -1);

            if (rb != null) rb.linearVelocity = dir * moveSpeed;

            bool isMoving = dir != Vector2.zero;
            if (isMoving) lastDir = dir;

            SetAnimDir(lastDir, isMoving);
        }

        private void SetAnimDir(Vector2 dir, bool isMoving)
        {
            if (anim == null) return;
            anim.SetFloat("Pos_X", dir.x);
            anim.SetFloat("Pos_Y", dir.y);
            anim.SetBool("isMoving", isMoving);
        }
    }
}
