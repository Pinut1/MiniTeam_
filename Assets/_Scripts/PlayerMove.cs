using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 8f;

    [Header("Mouse Distance Settings")]
    public float runDistanceThreshold = 6.0f;
    public float stopDistance = 0.5f;

    [Header("Knockback Settings")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    public float knockbackDuration = 0.5f;

    [Header("Laser Settings")]
    public GameObject laserObject;
    public float maxLaserLength = 10f;
    public float laserWidth = 0.1f;

    private Animator anim;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isKnockbacked = false;
    private bool isInputAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (laserObject != null) laserObject.SetActive(false);
    }

    void Update()
    {
        if (isKnockbacked) return;

        if (Input.GetMouseButton(0))
        {
            isInputAttacking = true;
            if (Input.GetMouseButtonDown(0))
            {
                if (anim != null) anim.SetTrigger("DoBackAttack");
            }
            if (anim != null) anim.SetBool("isAttacking", true);
            if (rb != null) rb.linearVelocity = Vector2.zero;

            UpdateLaser();
        }
        else
        {
            isInputAttacking = false;
            if (anim != null) anim.SetBool("isAttacking", false);
            if (laserObject != null) laserObject.SetActive(false);
        }

        bool isAnimatorInAttackState = anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("BackAttack");

        if (isInputAttacking || isAnimatorInAttackState)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleMovement();
    }

    void UpdateLaser()
    {
        if (laserObject == null) return;

        laserObject.SetActive(true);

        // 1. 마우스 월드 좌표 구하기
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // 2. 마우스와 가장 가까운 NPC 찾기
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        GameObject targetNPC = null;
        float minMouseDistance = 2.0f; // 마우스 주변 2유닛 이내의 NPC만 타겟팅

        foreach (GameObject npc in npcs)
        {
            float distToMouse = Vector2.Distance(mouseWorldPos, npc.transform.position);
            if (distToMouse < minMouseDistance)
            {
                minMouseDistance = distToMouse;
                targetNPC = npc;
            }
        }

        // 3. 레이저 방향 및 길이 조절
        if (targetNPC != null)
        {
            // 타겟 NPC 방향 벡터 계산
            Vector2 dir = (targetNPC.transform.position - laserObject.transform.position);
            float distToTarget = dir.magnitude;

            // 각도 계산 ($$ \theta = \operatorname{atan2}(y, x) \times \frac{180}{\pi} $$)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 캐릭터가 좌측을 보고 있을 때(Scale X = -1) 보정
            if (transform.localScale.x < 0)
            {
                angle += 180f;
            }

            laserObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            laserObject.transform.localScale = new Vector3(distToTarget, laserWidth, 1f);
        }
        else
        {
            // 타겟이 없으면 마우스 방향으로 최대 길이만큼 발사
            Vector2 dir = (mouseWorldPos - laserObject.transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (transform.localScale.x < 0) angle += 180f;

            laserObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            laserObject.transform.localScale = new Vector3(maxLaserLength, laserWidth, 1f);
        }
    }

    void HandleMovement()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

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

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();

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
    }

    public void TriggerKnockback(Transform enemyTransform)
    {
        if (isKnockbacked) return;
        isInputAttacking = false;
        if (anim != null) anim.SetBool("isAttacking", false);
        if (laserObject != null) laserObject.SetActive(false);

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