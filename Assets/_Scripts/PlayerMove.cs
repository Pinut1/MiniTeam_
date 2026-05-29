using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 8f;

    [Header("Mouse Distance Settings")]
    public float runDistanceThreshold = 6.0f;
    public float stopDistance = 0.5f;

    [Header("Boundary Settings")]
    public float minBoundaryX = -14f;
    public float maxBoundaryX = 35f;

    [Header("Flip Settings (Pivot Fix)")]
    public float flipOffset = 1.2f;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.5f;
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;

    public Animator anim;
    public Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isInputAttacking = false;

    // 마우스 눈치보기 플래그
    private bool waitForMouseMovement = false;
    private Vector3 lastMousePos;

    private PlayerLaser playerLaser; // PlayerLaser 참조

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        playerLaser = GetComponent<PlayerLaser>();
    }

    void Update()
    {
        // 클래시 모드(경쟁 중)일 때는 아예 움직이거나 마우스 공격 입력을 받지 못하도록 차단
        if (playerLaser != null && playerLaser.isClashMode)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetBool("isWalk", false);
                anim.SetBool("isRun", false);
                anim.SetBool("isIdle", true);
            }
            return;
        }

        if (waitForMouseMovement)
        {
            if (Vector3.Distance(Input.mousePosition, lastMousePos) > 10f)
            {
                waitForMouseMovement = false;
            }
            else
            {
                if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetBool("isIdle", true);
                return;
            }
        }

        if (Input.GetMouseButton(0))
        {
            isInputAttacking = true;
            if (Input.GetMouseButtonDown(0))
            {
                if (anim != null) anim.SetTrigger("DoBackAttack");
                
                // BackAttack 효과음 재생 시작
                if (BgmManager.Instance != null)
                {
                    BgmManager.Instance.PlayBackAttackSFX();
                }
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
        
        // 공격 중이 아닐 때 오디오 정지
        if (!isInputAttacking && !isAnimatorInAttackState)
        {
            if (BgmManager.Instance != null)
            {
                BgmManager.Instance.StopBackAttackSFX();
            }
        }

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

        if (mouseWorldPos.x > transform.position.x && !isFacingRight) Flip();
        else if (mouseWorldPos.x < transform.position.x && isFacingRight) Flip();

        float distanceX = Mathf.Abs(mouseWorldPos.x - transform.position.x);
        float moveInput = 0f;
        bool isMoving = false;
        bool isRunning = false;
        float currentSpeed = 0f;

        if (distanceX > stopDistance)
        {
            isMoving = true;
            moveInput = (mouseWorldPos.x > transform.position.x) ? 1f : -1f;
            
            // 경계 밖으로 나가려 하면 이동력 강제 0 처리
            if (transform.position.x <= minBoundaryX && moveInput < 0)
            {
                moveInput = 0f;
                isMoving = false;
            }
            if (transform.position.x >= maxBoundaryX && moveInput > 0)
            {
                moveInput = 0f;
                isMoving = false;
            }
            
            isRunning = (distanceX >= runDistanceThreshold) && isMoving;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }

        // 플레이어가 이미 경계를 넘어갔을 경우 억지로라도 안으로 밀어넣기
        Vector3 pos = transform.position;
        if (pos.x < minBoundaryX) pos.x = minBoundaryX;
        if (pos.x > maxBoundaryX) pos.x = maxBoundaryX;
        transform.position = pos;

        if (rb != null) rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

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
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
        float offsetDir = isFacingRight ? -1f : 1f;

        if (rb != null) rb.position += new Vector2(flipOffset * offsetDir, 0);
    }

    public void ForceWakeUpInputInit()
    {
        waitForMouseMovement = true;
        lastMousePos = Input.mousePosition;
        isInputAttacking = false;

        if (anim != null)
        {
            // ★ [최종 필살기] 기절 애니메이션이 완전히 끝난 지금! 
            // 멍때리고 있는 애니메이터를 강제로 0초부터 'Idle'로 꽂아버려 버그를 날립니다.
            anim.Play("Idle", 0, 0f);

            anim.ResetTrigger("isKnockback");
            anim.ResetTrigger("DoBackAttack");

            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttacking", false);
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        if (mouseWorldPos.x > transform.position.x && !isFacingRight) Flip();
        else if (mouseWorldPos.x < transform.position.x && isFacingRight) Flip();
    }

    public void TriggerKnockback(Transform enemyTransform)
    {
        ForceWakeUpInputInit();

        if (anim != null) anim.SetTrigger("doKnockback");
        if (rb != null)
        {
            float pushDirection = (enemyTransform.position.x > transform.position.x) ? -1f : 1f;
            rb.AddForce(new Vector2(pushDirection * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);
        }
        Invoke("OnBattleKnockbackEnd", knockbackDuration);
    }

    void OnBattleKnockbackEnd()
    {
    }
}