using UnityEngine;

public class GirlNpcReaction : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private NpcRandomPatrol moveScript;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<NpcRandomPatrol>();
    }

    public void LookAtAttackedNpc(Vector3 targetPos)
    {
        // 1. 방향 전환 (남학생 쪽 쳐다보기)
        Vector3 scale = transform.localScale;
        // 남학생이 오른쪽에 있으면 스케일 X를 양수로, 왼쪽에 있으면 음수로 (캐릭터 원본 방향에 따라 부호 조절)
        if (targetPos.x > transform.position.x) scale.x = Mathf.Abs(scale.x);
        else scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // 2. 이동 스크립트 비활성화
        if (moveScript != null) moveScript.enabled = false;

        // 3. 물리적 관성 제거
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 4. 애니메이션 멈춤
        if (anim != null) anim.SetBool("isWalking", false);
    }

    public void ResumeWalking()
    {
        if (moveScript != null)
        {
            // 이동 스크립트 다시 켜기
            moveScript.enabled = true;

            // [수정] 현재 보고 있는 방향으로 목표지점을 즉시 재설정하여 뒷걸음질 방지
            moveScript.ResetDirectionAfterReaction();
        }

        // 애니메이션 다시 걷기
        if (anim != null) anim.SetBool("isWalking", true);
    }
}