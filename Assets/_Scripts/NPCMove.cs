using UnityEngine;

public class NpcRandomPatrol : MonoBehaviour
{
    [Header("이동 반경 설정")]
    public float patrolDistance = 3.0f;

    [Header("속도 설정 (랜덤 범위)")]
    public float minSpeed = 1.0f;
    public float maxSpeed = 2.5f;

    [Header("대기 시간 설정 (랜덤 범위)")]
    public float minIdleTime = 1.0f;
    public float maxIdleTime = 3.0f;

    [Header("피에르 전용 설정")]
    public bool isPierre = false;
    public GameObject heartPrefab;
    public Sprite pierreHeartSprite;

    private float startPosX;
    private float targetPosX;
    private float currentSpeed;
    private float waitTimer;

    private bool isWaiting = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        startPosX = transform.position.x;
        SetNewTarget();
    }

    void Update()
    {
        // ★ [수정됨] 문제의 에러를 유발하던 GetBool 코드를 완전히 삭제했습니다!
        // PlayerLaser에서 레이저를 쏘면 어차피 이 스크립트 자체를 끄기 때문에 에러를 유발하며 여기서 검사할 필요가 없습니다.

        if (isWaiting)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;
                SetNewTarget();
            }
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);

            float step = currentSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(targetPosX, transform.position.y), step);

            if (Mathf.Abs(transform.position.x - targetPosX) < 0.05f)
            {
                isWaiting = true;
                waitTimer = Random.Range(minIdleTime, maxIdleTime);
            }
        }
    }

    void SetNewTarget()
    {
        targetPosX = startPosX + Random.Range(-patrolDistance, patrolDistance);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        UpdateScale();
    }

    void UpdateScale()
    {
        Vector3 currentScale = transform.localScale;
        if (targetPosX > transform.position.x)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        else if (targetPosX < transform.position.x)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        transform.localScale = currentScale;
    }

    public void ResetDirectionAfterReaction()
    {
        if (transform.localScale.x > 0)
        {
            targetPosX = transform.position.x + Random.Range(1.0f, patrolDistance);
        }
        else
        {
            targetPosX = transform.position.x - Random.Range(1.0f, patrolDistance);
        }

        targetPosX = Mathf.Clamp(targetPosX, startPosX - patrolDistance, startPosX + patrolDistance);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        isWaiting = false;
    }

    // =====================================================================
    // ★ 애니메이터에 해당 파라미터가 존재하는지 검사하는 안전장치
    // =====================================================================
    private bool HasParameter(string paramName)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    public void SetRedBurn()
    {
        if (anim != null)
        {
            if (HasParameter("isYellowBurn")) anim.SetBool("isYellowBurn", false);
            if (HasParameter("isBurn")) anim.SetBool("isBurn", true);
            if (HasParameter("isWalking")) anim.SetBool("isWalking", false);
        }
    }

    public void SetYellowBurn()
    {
        if (anim != null)
        {
            if (HasParameter("isBurn")) anim.SetBool("isBurn", false);
            if (HasParameter("isYellowBurn")) anim.SetBool("isYellowBurn", true);
            if (HasParameter("isWalking")) anim.SetBool("isWalking", false);
        }
    }

    public void OnPlayerLose()
    {
        if (isPierre)
        {
            if (anim != null)
            {
                if (HasParameter("isBurn")) anim.SetBool("isBurn", false);
                if (HasParameter("isYellowBurn")) anim.SetBool("isYellowBurn", false);
            }
            isWaiting = true;
            waitTimer = 0.5f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ★ 플레이어가 이겼을 때 (모든 남학생 공통 처리지만 피에르만 무지개 하트 드롭)
    public void DropHeartAndDie()
    {
        if (heartPrefab != null)
        {
            // 피에르 체크박스가 켜져있든 꺼져있든, 인스펙터에 직접 넣어둔 그 하트 프리팹을 생성합니다.
            GameObject droppedHeart = Instantiate(heartPrefab, transform.position, Quaternion.identity);

            // ★ [핵심] 피에르일 경우에만 하트의 스프라이트를 무지개 하트(Heart_9)로 강제 변경합니다.
            if (isPierre)
            {
                // 생성된 하트 오브젝트에서 DroppedHeart 스크립트를 찾습니다.
                DroppedHeart heartScript = droppedHeart.GetComponent<DroppedHeart>();
                if (heartScript == null) heartScript = droppedHeart.GetComponentInChildren<DroppedHeart>();

                if (heartScript != null)
                {
                    // PlayerLaser에서 주머니 연동 처리를 하므로, 
                    // 하트가 생성된 직후 SpriteRenderer를 무지개 스프라이트로 바로 덮어씌워 줍니다.
                    SpriteRenderer heartSR = droppedHeart.GetComponent<SpriteRenderer>();
                    if (heartSR == null) heartSR = droppedHeart.GetComponentInChildren<SpriteRenderer>();

                    // 프로젝트 창의 "Heart_9" 스프라이트 텍스처를 인스펙터로 받아와서 꽂아줍니다.
                    if (pierreHeartSprite != null && heartSR != null)
                    {
                        heartSR.sprite = pierreHeartSprite;
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}