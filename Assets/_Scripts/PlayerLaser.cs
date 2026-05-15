using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject laserObject;
    [Range(0.01f, 1f)]
    public float laserWidth = 0.05f;

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Target Point (Heart)")]
    public GameObject targetPoint;

    public Animator anim;
    private Vector3 lockedTargetPos;
    private bool isFiring = false;

    private GameObject currentBurningNpc; // 현재 불타고 있는 남학생 저장용

    void Start()
    {
        if (laserObject != null) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);
    }

    void Update()
    {
        if (!isFiring)
        {
            UpdateHovering();
        }

        // 클릭 시작 시 타겟 고정 및 여학생들 정지
        if (Input.GetMouseButtonDown(0))
        {
            CheckAndLockTarget();
        }

        // 클릭 유지 시 레이저 출력
        if (isFiring && Input.GetMouseButton(0))
        {
            // 만약 레이저는 쏘고 있는데 타겟 NPC 정보가 날아갔거나 반응이 없다면 재보정
            if (currentBurningNpc != null)
            {
                Animator targetAnim = currentBurningNpc.GetComponent<Animator>();

                // 만약 애니메이션이 idle이나 walk로 돌아가버렸다면 강제로 다시 켬
                if (targetAnim != null && targetAnim.GetBool("isBurning") == false)
                {
                    targetAnim.SetBool("isBurning", true);

                    // 이동 스크립트도 확실히 꺼져있는지 재확인
                    var move = currentBurningNpc.GetComponent<NpcRandomPatrol>();
                    if (move != null && move.enabled) move.enabled = false;
                }

                // 애니메이션 상태 확인 후 레이저 그리기
                bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("BackAttack");
                if (isAttacking)
                {
                    if (targetPoint != null)
                    {
                        targetPoint.SetActive(true);
                        targetPoint.transform.position = lockedTargetPos;
                    }
                    DrawLaser(lockedTargetPos);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopFiring();
            ResumeAllGirls();
        }
    }

    void UpdateHovering()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("NPC"))
        {
            if (targetPoint != null)
            {
                targetPoint.SetActive(true);
                targetPoint.transform.position = hit.transform.position;
            }
        }
        else
        {
            if (targetPoint != null) targetPoint.SetActive(false);
        }
    }

    // 다른 스크립트에서 접근할 수 있도록 public으로 설정
    public void CheckAndLockTarget()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("NPC"))
        {
            currentBurningNpc = hit.collider.gameObject;
            lockedTargetPos = currentBurningNpc.transform.position;
            isFiring = true;

            Animator targetAnim = currentBurningNpc.GetComponent<Animator>();
            if (targetAnim != null)
            {
                // [수정] 걷기 애니메이션을 명시적으로 끄고 불타기 애니메이션을 켭니다.
                targetAnim.SetBool("isWalking", false);
                targetAnim.SetBool("isBurning", true);
            }

            // 이동 스크립트 정지
            var move = currentBurningNpc.GetComponent<NpcRandomPatrol>();
            if (move != null) move.enabled = false;

            // 3. 주변 여학생 탐색 및 정지 명령 (기존 코드)
            Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(lockedTargetPos, 0.5f);
            foreach (Collider2D col in overlappingColliders)
            {
                GirlNpcReaction girl = col.GetComponentInParent<GirlNpcReaction>();
                if (girl != null && girl.gameObject != hit.collider.gameObject)
                {
                    girl.LookAtAttackedNpc(lockedTargetPos);
                }
            }
        }
    }

    // 다른 스크립트에서 접근할 수 있도록 public으로 설정
    public void ResumeAllGirls()
    {
        GirlNpcReaction[] allGirls = FindObjectsByType<GirlNpcReaction>(FindObjectsSortMode.None);
        foreach (GirlNpcReaction girl in allGirls)
        {
            girl.ResumeWalking();
        }
    }

    void DrawLaser(Vector3 targetPos)
    {
        if (laserObject == null || firePoint == null) return;

        laserObject.SetActive(true);
        laserObject.transform.position = firePoint.position;

        Vector2 direction = targetPos - firePoint.position;
        float distance = direction.magnitude;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        laserObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        SpriteRenderer sr = laserObject.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float baseWidth = sr.sprite.bounds.size.x;
            laserObject.transform.localScale = new Vector3(distance / baseWidth, laserWidth, 1f);
        }
    }

    void StopFiring()
    {
        isFiring = false;

        // [수정] 좌표로 다시 찾는 대신, 저장해둔 currentBurningNpc를 직접 끕니다.
        if (currentBurningNpc != null)
        {
            Animator targetAnim = currentBurningNpc.GetComponent<Animator>();
            NpcRandomPatrol targetMove = currentBurningNpc.GetComponent<NpcRandomPatrol>();

            if (targetAnim != null) targetAnim.SetBool("isBurning", false);
            if (targetMove != null)
            {
                targetMove.enabled = true;
                targetMove.ResetDirectionAfterReaction();
            }

            // 작업이 끝났으니 변수를 비워줍니다.
            currentBurningNpc = null;
        }

        if (laserObject != null) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);
    }
}