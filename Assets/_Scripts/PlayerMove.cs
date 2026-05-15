using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 8f;

    [Header("Mouse Distance Settings")]
    public float runDistanceThreshold = 6.0f;
    public float stopDistance = 0.5f;

    [Header("Flip Settings (Pivot Fix)")]
    // 이 값을 0.1단위로 천천히 늘리면서 맞춰보세요. 
    // 캐릭터가 오른쪽으로 튀면 값을 줄이고, 왼쪽으로 튀면 값을 늘려야 합니다.
    public float flipOffset = 1.2f;

    [Header("Knockback Settings")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    public float knockbackDuration = 0.5f;

    public Animator anim;
    public Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isKnockbacked = false;
    private bool isInputAttacking = false;


    void Start()
    {
        // anim = GetComponentInChildren<Animator>();
        // rb = GetComponentInChildren<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockbacked) return;

        // 마우스 클릭(공격) 처리
        if (Input.GetMouseButton(0))
        {
            isInputAttacking = true;
            if (Input.GetMouseButtonDown(0))
            {
                if (anim != null) anim.SetTrigger("DoBackAttack");
            }
            if (anim != null) anim.SetBool("isAttacking", true);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            isInputAttacking = false;
            if (anim != null) anim.SetBool("isAttacking", false);
        }

        bool isAnimatorInAttackState = anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("BackAttack");

        if (isInputAttacking || isAnimatorInAttackState)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // --- [방향 전환 로직: 세로선 기준 즉시 전환] ---
        // 이동 거리(stopDistance)와 상관없이 마우스가 캐릭터 왼쪽/오른쪽인지에 따라 즉시 Flip
        if (mouseWorldPos.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (mouseWorldPos.x < transform.position.x && isFacingRight)
        {
            Flip();
        }

        // --- [이동 로직] ---
        float distanceX = Mathf.Abs(mouseWorldPos.x - transform.position.x);
        float moveInput = 0f;
        bool isMoving = false;
        bool isRunning = false;
        float currentSpeed = 0f;

        if (distanceX > stopDistance)
        {
            isMoving = true;
            moveInput = (mouseWorldPos.x > transform.position.x) ? 1f : -1f;
            currentSpeed = (distanceX >= runDistanceThreshold) ? runSpeed : walkSpeed;
            isRunning = (distanceX >= runDistanceThreshold);
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
        }

        if (anim != null)
        {
            anim.SetBool("isIdle", !isMoving);
            anim.SetBool("isWalk", isMoving && !isRunning);
            anim.SetBool("isRun", isMoving && isRunning);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        // 1. 스케일 반전 (여기서 캐릭터가 휙 돌아갑니다)
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;

        // 2. 위치 보정 (중요: 방향 로직 수정)
        // 왼쪽을 보게 될 때(isFacingRight=false) 캐릭터가 왼쪽으로 튀었다면 
        // 부모의 위치를 오른쪽(+)으로 밀어줘야 합니다.
        float offsetDir = isFacingRight ? -1f : 1f;

        // Rigidbody를 사용 중이므로 rb.position을 직접 수정하는 것이 훨씬 정확하고 부드럽습니다.
        if (rb != null)
        {
            rb.position += new Vector2(flipOffset * offsetDir, 0);
        }
        else
        {
            transform.position += new Vector3(flipOffset * offsetDir, 0, 0);
        }
    }

    public void TriggerKnockback(Transform enemyTransform)
    {
        if (isKnockbacked) return;
        isInputAttacking = false;
        if (anim != null) anim.SetBool("isAttacking", false);

        isKnockbacked = true;
        if (anim != null) anim.SetTrigger("doKnockback");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            float pushDirection = (enemyTransform.position.x > transform.position.x) ? -1f : 1f;
            rb.AddForce(new Vector2(pushDirection * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);
        }
        Invoke("EndKnockback", knockbackDuration);
    }

    void EndKnockback() { isKnockbacked = false; }
}