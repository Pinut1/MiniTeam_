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

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
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
            if (Input.GetMouseButtonDown(0) && anim != null) anim.SetTrigger("DoBackAttack");
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
            isRunning = (distanceX >= runDistanceThreshold);
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }

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