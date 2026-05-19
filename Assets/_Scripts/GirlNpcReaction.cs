using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GirlNpcReaction : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private NpcRandomPatrol moveScript;

    private GameObject surpriseMark;
    private SpriteRenderer markSR;
    private GameObject npcLaser;

    private Coroutine reactionCoroutine;
    private Vector3 lastTargetPos;      // 남학생의 피벗(가슴) 위치를 그대로 저장!
    private bool isLaserActive = false;
    private GameObject targetBoyNpc;

    [Header("레이저 정밀 조준 설정")]
    [SerializeField] private float eyeOffset;         // 여학생의 눈 높이
    [SerializeField] private float eyeForwardOffset;  // 여학생의 눈 앞쪽으로 오프셋
    [SerializeField] private float laserScaleFactor;  // 스프라이트 배율
    [SerializeField] private float laserThickness;   // 레이저 굵기 (Y-Scale)

    [Header("Knockback Flying Settings")]
    private bool isFlyingAway = false;
    public float flyArcHeight = 5f;     // 포물선 높이
    public float flyDuration = 1.2f;    // 날아가는 시간 (살짝 단축해서 쫄깃하게!)
    private Vector3 flyStartPos;
    private Vector3 flyTargetPos;
    private float flyTime = 0f;

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
            if (markSR != null)
            {
                markSR.sortingLayerName = "NPC";
                markSR.sortingOrder = 999;
            }
        }

        Transform laserTransform = transform.Find("Laser_Yellow_0");
        if (laserTransform != null)
        {
            npcLaser = laserTransform.gameObject;
            npcLaser.SetActive(false);
        }
    }

    // 애니메이터 계산이 끝난 후, 강제로 레이저를 눈에 박고 조준시킵니다.
    void LateUpdate()
    {
        if (isLaserActive && npcLaser != null)
        {
            float facingDirection = Mathf.Sign(transform.localScale.x);
            // 1. 발사점(여학생 눈) 계산 (Offset 적용)
            Vector3 firePoint = transform.position + new Vector3(eyeForwardOffset * facingDirection, eyeOffset, 0);

            // 도착점은 넘겨받은 남학생의 피벗 위치 그대로 사용
            Vector3 targetPos = lastTargetPos;

            // 2. 레이저 시작 위치를 여학생 눈으로 고정
            npcLaser.transform.position = firePoint;

            // 3. 눈에서 타겟을 향하는 방향 벡터 계산
            Vector3 dir = targetPos - firePoint;

            // 회전: 레이저 스프라이트의 오른쪽(Right) 면이 타겟을 바라보도록 설정
            npcLaser.transform.right = dir;

            // 4. 완벽한 길이 적용
            SpriteRenderer sr = npcLaser.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                // 타겟까지의 실제 월드 거리
                float currentDist = dir.magnitude;

                float baseWidth = sr.sprite.bounds.size.x;

                float parentScaleX = Mathf.Abs(transform.localScale.x);

                float exactScaleX = (currentDist / baseWidth) / parentScaleX;

                if (transform.localScale.x < 0)
                {
                    exactScaleX *= -1f;
                }

                exactScaleX *= laserScaleFactor;

                npcLaser.transform.localScale = new Vector3(exactScaleX, laserThickness, 1f);
            }
        }
    }

    public void LookAtAttackedNpc(GameObject targetNpc)
    {
        targetBoyNpc = targetNpc; // 남학생 저장
        lastTargetPos = targetNpc.transform.position; // 기존 레이저 조준용 위치 저장

        if (reactionCoroutine != null) StopCoroutine(reactionCoroutine);
        reactionCoroutine = StartCoroutine(ReactionSequence(targetNpc));
    }

    private IEnumerator ReactionSequence(GameObject targetNpc)
    {
        // 1. 방향 전환
        Vector3 targetPos = targetNpc.transform.position;
        Vector3 scale = transform.localScale;
        if (targetPos.x > transform.position.x) scale.x = Mathf.Abs(scale.x);
        else scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // 2. 이동 정지 및 놀람 마크 온
        if (moveScript != null) moveScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
        }

        isLaserActive = false;
        if (npcLaser != null) npcLaser.SetActive(false);
        if (surpriseMark != null) surpriseMark.SetActive(true);

        // 3. 1초 대기 (놀라는 시간)
        yield return new WaitForSeconds(1f);

        // 4. 놀람 마크 끄고 여학생 공격 애니메이션 온!
        if (surpriseMark != null) surpriseMark.SetActive(false);
        if (anim != null) anim.SetBool("isAttacking", true);

        // ★★★ [추가된 핵심 로직] 여학생이 레이저를 쏘는 순간, 남학생을 노란색 불타기로 바꿈! ★★★
        if (targetBoyNpc != null)
        {
            Animator boyAnim = targetBoyNpc.GetComponent<Animator>();
            if (boyAnim != null)
            {
                boyAnim.SetBool("isYellowBurn", true); // 결투 애니메이션 발동!
            }
        }

        if (npcLaser != null) npcLaser.SetActive(true);
        isLaserActive = true;
    }

    public void StartFlyingAway(Vector3 laserOriginPos)
    {
        if (isFlyingAway) return;
        isFlyingAway = true;

        var patrol = GetComponent<NpcRandomPatrol>();
        if (patrol != null) patrol.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Animator girlAnim = GetComponent<Animator>();
        if (girlAnim == null) girlAnim = GetComponentInChildren<Animator>();
        if (girlAnim != null)
        {
            girlAnim.SetBool("isWalking", false);
            girlAnim.SetTrigger("isKnockback");
        }

        float flyDirection = transform.position.x >= laserOriginPos.x ? 1f : -1f;
        flyStartPos = transform.position;
        flyTargetPos = flyStartPos + new Vector3(flyDirection * 15f, -2f, 0f);

        // ★ [에러 해결] 이름이 정확히 일치하는지 대소문자 확인 사살!
        StartCoroutine(FlyAwayCoroutine());
    }

    private IEnumerator FlyAwayCoroutine()
    {
        flyTime = 0f;

        while (flyTime < flyDuration)
        {
            flyTime += Time.deltaTime;
            float progress = flyTime / flyDuration;

            Vector3 currentPos = Vector3.Lerp(flyStartPos, flyTargetPos, progress);
            float height = Mathf.Sin(progress * Mathf.PI) * flyArcHeight;
            currentPos.y += height;

            transform.position = currentPos;
            yield return null;
        }

        Debug.Log(gameObject.name + " 날아가기 완료. 삭제합니다.");
        Destroy(gameObject);
    }

    public void ResumeWalking()
    {
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
            reactionCoroutine = null;
        }

        isLaserActive = false;
        if (surpriseMark != null) surpriseMark.SetActive(false);
        if (npcLaser != null) npcLaser.SetActive(false);
        if (anim != null) anim.SetBool("isAttacking", false);

        if (moveScript != null)
        {
            moveScript.enabled = true;
            moveScript.ResetDirectionAfterReaction();
        }
    }

    // ★ PlayerLaser에서 패배 시 호출하는 함수
    public void WinAndLeaveScene()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("isKnockback");
            anim.SetBool("isAttacking", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isSmile", true);
        }

        // 기존에 NPC를 맴돌게 하던 이동 스크립트가 있다면 꺼서 충돌을 막습니다.
        MonoBehaviour moveScript = GetComponent("NpcRandomPatrol") as MonoBehaviour;
        if (moveScript != null) moveScript.enabled = false;

        // 웃으면서 밖으로 나가는 코루틴 시작!
        StartCoroutine(LeaveSceneCoroutine());
    }

    private System.Collections.IEnumerator LeaveSceneCoroutine()
    {
        // 0.5초 정도 제자리에서 방긋 웃으며 승리를 만끽합니다.
        yield return new WaitForSeconds(0.5f);

        float moveSpeed = 5.0f; // 걸어나가는 속도
        float direction = 1f;   // 기본 방향 (1 = 오른쪽, -1 = 왼쪽)

        // ★ [핵심 1] 카메라를 기준으로 여학생이 어느 쪽에 있는지 파악합니다.
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // 여학생이 카메라 중심보다 오른쪽에 있으면 오른쪽으로, 왼쪽에 있으면 왼쪽으로 나갑니다.
            if (transform.position.x >= mainCam.transform.position.x)
            {
                direction = 1f;  // 화면 오른쪽으로 퇴장
            }
            else
            {
                direction = -1f; // 화면 왼쪽으로 퇴장
            }
        }

        // ★ [핵심 2] 나가는 방향으로 고개를 휙 돌립니다 (좌우 반전 Flip)
        Vector3 currentScale = transform.localScale;

        // (주의: 만약 게임 플레이 시 여학생이 문워크를 한다면 아래 줄의 direction 앞에 마이너스(-)를 붙여주세요!)
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;

        // 약 4초간 화면 밖으로 스르륵 이동하며 나갑니다.
        float time = 0f;
        while (time < 4f)
        {
            time += Time.deltaTime;
            // Space.World를 붙여주면 캐릭터가 뒤집혀있어도 무조건 정해진 월드 방향으로 걸어갑니다.
            transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        // 화면 밖으로 완전히 나갔으면 스스로 삭제
        Destroy(gameObject);
    }

    public void WalkAwayAndDestroy()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isWalk", true); // 걷기 애니메이션 시작
            anim.SetBool("isIdle", false);
        }

        // 이동 스크립트 정지
        MonoBehaviour moveScript = GetComponent("NpcRandomPatrol") as MonoBehaviour;
        if (moveScript != null) moveScript.enabled = false;

        StartCoroutine(WalkAwayCoroutine());
    }

    private System.Collections.IEnumerator WalkAwayCoroutine()
    {
        // 화면 가장자리 방향 계산 (카메라 기준)
        float direction = transform.position.x > Camera.main.transform.position.x ? 1 : -1;

        // 방향 전환 (Flip)
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        float speed = 5.0f;
        float timer = 0;

        // 3초간 걷기
        while (timer < 3f)
        {
            transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // 소멸
    }
}