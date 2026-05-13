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
        private SpriteRenderer sr;
        private Animator anim;

        void Start()
        {
            rb   = GetComponent<Rigidbody2D>();
            sr   = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
            }
        }

        void Update()
        {
            if (!IsControllable)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                if (anim != null) anim.SetBool("isWalking", false);
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(h, v).normalized;

            if (rb != null) rb.linearVelocity = dir * moveSpeed;

            if (h != 0 && sr != null) sr.flipX = h < 0;

            if (anim != null) anim.SetBool("isWalking", dir.magnitude > 0.01f);
        }
    }
}
