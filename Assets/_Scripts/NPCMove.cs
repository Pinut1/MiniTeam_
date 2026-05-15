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

    // 방향에 맞춰 스케일을 조절하는 로직을 별도 함수로 분리
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

    // [핵심 추가] 반응 종료 후 다시 걷기 시작할 때 호출할 함수
    public void ResetDirectionAfterReaction()
    {
        // 현재 바라보고 있는 방향(Scale.x)에 따라 새로운 타겟을 앞쪽에 설정하여 문워크 방지
        if (transform.localScale.x > 0)
        {
            targetPosX = transform.position.x + Random.Range(1.0f, patrolDistance);
        }
        else
        {
            targetPosX = transform.position.x - Random.Range(1.0f, patrolDistance);
        }

        // 전체 순찰 범위를 벗어나지 않게 제한
        targetPosX = Mathf.Clamp(targetPosX, startPosX - patrolDistance, startPosX + patrolDistance);

        currentSpeed = Random.Range(minSpeed, maxSpeed);
        isWaiting = false;
    }
}