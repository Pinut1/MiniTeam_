using System.Collections;
using UnityEngine;

public class GirlNPCController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 남자가 공격받았을 때 호출될 함수
    public void ReactToPlayerAttack()
    {
        // 1. 여기서 여자가 뒤를 돌아보는 로직을 실행 (예: 스프라이트 좌우 반전 등)
        // transform.localScale = new Vector3(-1, 1, 1); 

        // 2. 1초 뒤 공격 자세를 취하는 코루틴 시작
        StartCoroutine(AttackAfterDelayCoroutine());
    }

    private IEnumerator AttackAfterDelayCoroutine()
    {
        // 정확히 1초 동안 대기
        yield return new WaitForSeconds(1.0f);

        // 애니메이터의 isAttacking 파라미터를 true로 바꿔서 Transition 실행!
        animator.SetBool("isAttacking", true);
    }
}