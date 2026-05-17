using UnityEngine;

public class GirlNpcReaction : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private NpcRandomPatrol moveScript;

    private GameObject surpriseMark;
    private SpriteRenderer markSR; // 마크의 스프라이트 렌더러

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<NpcRandomPatrol>();

        Transform markTransform = transform.Find("Surprise_Mark");
        if (markTransform != null)
        {
            surpriseMark = markTransform.gameObject;
            markSR = surpriseMark.GetComponent<SpriteRenderer>();

            // [최종 해결] 씬에 배치된 마크가 프리팹 설정을 무시하고 
            // Default 레이어로 렌더링 되어 배경에 가려지는 현상 강제 해결
            if (markSR != null)
            {
                markSR.sortingLayerName = "NPC"; // 백그라운드보다 높은 NPC 레이어로 고정
                markSR.sortingOrder = 999;       // 무조건 제일 앞에 보이게 고정
            }
        }
    }

    public void LookAtAttackedNpc(Vector3 targetPos)
    {
        // 방향 전환
        Vector3 scale = transform.localScale;
        if (targetPos.x > transform.position.x) scale.x = Mathf.Abs(scale.x);
        else scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // 돌아볼 때 마크 활성화
        if (surpriseMark != null)
        {
            surpriseMark.SetActive(true);
        }

        if (moveScript != null) moveScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isWalking", false);
    }

    public void ResumeWalking()
    {
        if (moveScript != null)
        {
            moveScript.enabled = true;
            moveScript.ResetDirectionAfterReaction();
        }

        if (anim != null) anim.SetBool("isWalking", true);

        // 다시 걸어갈 때 마크 비활성화
        if (surpriseMark != null)
        {
            surpriseMark.SetActive(false);
        }
    }
}