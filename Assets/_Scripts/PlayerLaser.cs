using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    private GameObject currentBurningNpc;

    [Header("Hover Heart Settings")]
    public GameObject heartItemPrefab;
    public float heartSpawnHeight = 1.0f;
    private GameObject currentHoveredNpc;

    [Header("Heart Fill Settings")]
    public float heartFillSpeed = 0.5f;
    private Image currentFillImage;
    public GameObject droppedHeartPrefab;
    public float dropYOffset = -1.5f;

    [Header("Clash Gauge Settings (Balanced)")]
    public GameObject clashGaugeObject;
    public Image playerPinkGauge;

    public float clashClickPower = 0.1f;
    public float clashDrainSpeed = 0.15f;

    public Vector3 gaugeOffset = new Vector3(0, 1.5f, 0);
    private RectTransform gaugeRectTransform;
    private Camera mainCam;
    private bool isClashMode = false;

    [Header("Difficulty by Girls (여학생 난이도 설정)")]
    public float difficultyRadius = 3.0f; // 주변 여학생을 탐색할 반경
    public float penaltyFillSpeedPerGirl = 0.1f; // 여학생 1명당 '홀드 채우기' 속도 감소량
    public float minHeartFillSpeed = 0.05f; // 아무리 여학생이 많아도 최소한 보장되는 채우기 속도
    public float penaltyDrainSpeedPerGirl = 0.1f; // 여학생 1명당 '연타 방어(게이지 깎임)' 속도 증가량

    [Header("Player Knockback Settings")]
    public float playerKnockbackDistance = 3f;
    public float playerKnockbackHeight = 1.5f;
    public float playerGroundStunDuration = 2.0f; // 기절 대기 시간
    public float knockbackYOffset = -0.5f;
    private bool isPlayerKnockedBack = false;

    [Header("Scripts")]
    public PlayerMove playerMoveScript;

    // 중복 방지를 위한 하트 이미지 제비뽑기 주머니
    private List<Sprite> heartSpritePool = new List<Sprite>();

    // ==========================================
    // ★ 바닐라와의 대결 모드 변수 ★
    // ==========================================
    private bool isInCompetitionMode = false;
    private Vector2 competitionTarget;
    private Color originalLaserColor = Color.white;
    private float originalWidth;

    void Start()
    {
        // 원본 레이저의 두께와 색상을 저장해둡니다.
        originalWidth = laserWidth;
        if (laserObject != null)
        {
            SpriteRenderer sr = laserObject.GetComponent<SpriteRenderer>();
            if (sr != null) originalLaserColor = sr.color;
            laserObject.SetActive(false);
        }
        
        if (targetPoint != null) targetPoint.SetActive(false);

        if (clashGaugeObject != null)
            gaugeRectTransform = clashGaugeObject.GetComponent<RectTransform>();
        mainCam = Camera.main;

        if (playerMoveScript == null) playerMoveScript = GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (isInCompetitionMode)
        {
            DrawLaser(competitionTarget);
            return;
        }

        if (isPlayerKnockedBack)
        {
            if (targetPoint != null && targetPoint.activeSelf) targetPoint.SetActive(false);
            if (laserObject != null && laserObject.activeSelf) laserObject.SetActive(false);
            return;
        }

        if (!isFiring)
        {
            UpdateHovering();
            if (Input.GetMouseButtonDown(0)) CheckAndLockTarget();
        }
        else
        {
            if (!isClashMode && currentBurningNpc != null)
            {
                Animator targetAnim = currentBurningNpc.GetComponent<Animator>();
                if (targetAnim != null && targetAnim.GetBool("isYellowBurn") == true) StartClashMode();
            }

            if (isClashMode)
            {
                UpdateClashGaugePosition();
                if (targetPoint != null) { targetPoint.SetActive(true); targetPoint.transform.position = lockedTargetPos; }
                DrawLaser(lockedTargetPos);

                if (Input.GetMouseButtonDown(0) && playerPinkGauge != null)
                {
                    playerPinkGauge.fillAmount += clashClickPower;
                    if (playerPinkGauge.fillAmount >= 1f)
                    {
                        KnockbackNearbyGirls();
                        SuccessAndDropHeart();
                        return;
                    }
                }

                if (playerPinkGauge != null)
                {
                    // =======================================================
                    // ★ [난이도 적용] 주변 여학생 수만큼 연타 게이지 깎이는 속도가 빨라집니다! (어려워짐)
                    int girlCount = GetNearbyGirlCount();
                    float currentDrainSpeed = clashDrainSpeed + (penaltyDrainSpeedPerGirl * girlCount);

                    playerPinkGauge.fillAmount -= currentDrainSpeed * Time.deltaTime;
                    // =======================================================

                    if (playerPinkGauge.fillAmount <= 0f)
                    {
                        LetGirlWinAndLeave();
                        GameObject targetToDestroy = currentBurningNpc;
                        StartPlayerKnockback();

                        if (targetToDestroy != null)
                        {
                            if (targetToDestroy.name.Contains("Pierre")) return;
                            Destroy(targetToDestroy);
                        }
                        return;
                    }
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (currentBurningNpc != null)
                    {
                        Animator targetAnim = currentBurningNpc.GetComponent<Animator>();
                        if (targetAnim != null && targetAnim.GetBool("isBurning") == false)
                        {
                            targetAnim.SetBool("isBurning", true);
                            var move = currentBurningNpc.GetComponent<NpcRandomPatrol>();
                            if (move != null) move.enabled = false;
                        }

                        if (targetPoint != null) { targetPoint.SetActive(true); targetPoint.transform.position = lockedTargetPos; }
                        DrawLaser(lockedTargetPos);

                        if (currentFillImage != null)
                        {
                            // =======================================================
                            // ★ [난이도 적용] 주변 여학생 수만큼 홀드 게이지 차오르는 속도가 느려집니다! (어려워짐)
                            int girlCount = GetNearbyGirlCount();
                            float currentFillSpeed = heartFillSpeed - (penaltyFillSpeedPerGirl * girlCount);

                            // 아무리 여학생이 많아도 속도가 마이너스가 되거나 멈추지 않게 최소 속도(minHeartFillSpeed)를 보장합니다.
                            currentFillSpeed = Mathf.Max(minHeartFillSpeed, currentFillSpeed);

                            currentFillImage.fillAmount += currentFillSpeed * Time.deltaTime;
                            // =======================================================

                            if (currentFillImage.fillAmount >= 1f) SuccessAndDropHeart();
                        }
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    StopFiring();
                    ResumeAllGirls();
                }
            }
        }
    }

    // 타겟 주변의 여학생 수를 계산하는 함수
    private int GetNearbyGirlCount()
    {
        if (currentBurningNpc == null) return 0;

        int count = 0;
        // 타겟을 중심으로 difficultyRadius 반경 내의 모든 콜라이더를 찾습니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(currentBurningNpc.transform.position, difficultyRadius);

        foreach (Collider2D col in colliders)
        {
            // GirlNpcReaction 컴포넌트가 있는지 확인하여 여학생인지 판별합니다.
            GirlNpcReaction girl = col.GetComponent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInParent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInChildren<GirlNpcReaction>();

            if (girl != null)
            {
                count++;
            }
        }
        return count;
    }

    // ★ 매니저에서 넉백을 호출할 수 있도록 public으로 열고, 공격자(바닐라)의 위치를 받을 수 있게 수정했습니다.
    public void StartPlayerKnockback(Transform attacker = null)
    {
        if (isPlayerKnockedBack) return;
        isPlayerKnockedBack = true;

        if (PlayerUIScene.instance != null)
        {
            PlayerUIScene.instance.SetKnockbackPortrait();
        }

        float pushDirection = -1f;

        // attacker(바닐라)가 따로 지정되었으면 그 위치를, 아니면 기존 일반 NPC(currentBurningNpc) 위치를 기준으로 삼습니다.
        Transform target = attacker != null ? attacker : (currentBurningNpc != null ? currentBurningNpc.transform : null);

        if (target != null)
        {
            // 공격자가 쇼콜라보다 오른쪽에 있으면 왼쪽(-1)으로, 왼쪽에 있으면 오른쪽(1)으로 날아갑니다.
            pushDirection = transform.position.x >= target.position.x ? 1f : -1f;
        }

        if (anim != null) anim.SetTrigger("isKnockback");

        if (currentHoveredNpc != null)
        {
            HideHeart(currentHoveredNpc);
            currentHoveredNpc = null;
        }

        StopFiring();
        ResumeAllGirls();

        PlayerMove[] allMoves = GetComponentsInChildren<PlayerMove>();
        foreach (PlayerMove pm in allMoves) { pm.enabled = false; }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }

        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        if (cam != null) cam.SetKnockbackMode(true);

        StartCoroutine(PlayerKnockbackCoroutine(pushDirection));
    }

    private System.Collections.IEnumerator PlayerKnockbackCoroutine(float pushDir)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(pushDir * playerKnockbackDistance, knockbackYOffset, 0f);

        float duration = 0.5f;
        if (playerMoveScript != null) duration = playerMoveScript.knockbackDuration;

        float time = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (time < duration)
        {
            time += Time.fixedDeltaTime;
            float progress = time / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            float height = Mathf.Sin(progress * Mathf.PI) * playerKnockbackHeight;
            currentPos.y += height;

            transform.position = currentPos;
            if (rb != null) rb.position = currentPos;

            yield return new WaitForFixedUpdate();
        }

        transform.position = targetPos;
        if (rb != null) rb.position = targetPos;

        // 기절해서 누워있는 시간 대기
        yield return new WaitForSeconds(playerGroundStunDuration);

        isPlayerKnockedBack = false;

        // 넉백 스턴 시간이 끝나고 일어났으니 초상화를 다시 평소 표정(UI_IDLE)으로 복구합니다.
        if (PlayerUIScene.instance != null)
        {
            PlayerUIScene.instance.SetIdlePortrait();
        }

        Vector3 wakeUpPos = new Vector3(transform.position.x, startPos.y, transform.position.z);
        transform.position = wakeUpPos;
        if (rb != null) rb.position = wakeUpPos;

        if (rb != null) rb.simulated = true;

        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true;
            playerMoveScript.ForceWakeUpInputInit();
        }
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        if (cam != null) cam.SetKnockbackMode(false);
    }

    void KnockbackNearbyGirls()
    {
        if (currentBurningNpc == null) return;
        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(currentBurningNpc.transform.position, 2.0f);
        foreach (Collider2D col in overlappingColliders)
        {
            GirlNpcReaction girl = col.GetComponent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInParent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInChildren<GirlNpcReaction>();
            if (girl != null) girl.StartFlyingAway(transform.position);
        }
    }

    void LetGirlWinAndLeave()
    {
        if (currentBurningNpc == null) return;
        bool isPierre = currentBurningNpc.name.Contains("Pierre");

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(currentBurningNpc.transform.position, 2.0f);
        foreach (Collider2D col in overlappingColliders)
        {
            GirlNpcReaction girl = col.GetComponent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInParent<GirlNpcReaction>();
            if (girl == null) girl = col.GetComponentInChildren<GirlNpcReaction>();

            if (girl != null)
            {
                if (isPierre)
                {
                    girl.ResumeWalking();
                }
                else
                {
                    girl.WinAndLeaveScene();
                }
            }
        }

        if (!isPierre)
        {
            Destroy(currentBurningNpc);
        }
        else
        {
            var patrol = currentBurningNpc.GetComponent<NpcRandomPatrol>();
            if (patrol != null)
            {
                patrol.enabled = true;
                patrol.ResetDirectionAfterReaction();
            }
        }
    }

    void StartClashMode()
    {
        isClashMode = true;
        if (clashGaugeObject != null)
        {
            clashGaugeObject.SetActive(true);
            UpdateClashGaugePosition();
        }
        if (playerPinkGauge != null) playerPinkGauge.fillAmount = 0.5f;
        if (currentFillImage != null && currentFillImage.transform.parent != null) currentFillImage.transform.parent.gameObject.SetActive(false);
    }

    void UpdateClashGaugePosition()
    {
        if (currentBurningNpc == null || clashGaugeObject == null || mainCam == null || gaugeRectTransform == null) return;
        Vector3 worldPos = currentBurningNpc.transform.position + gaugeOffset;
        Vector2 screenPoint = mainCam.WorldToScreenPoint(worldPos);
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gaugeRectTransform.parent as RectTransform, screenPoint, null, out canvasPos);
        gaugeRectTransform.anchoredPosition = canvasPos;
    }

    void SuccessAndDropHeart()
    {
        if (currentBurningNpc == null || currentFillImage == null) return;

        Vector3 npcPos = currentBurningNpc.transform.position;
        Sprite finalHeartSprite = currentFillImage.sprite;
        float floorY = transform.position.y + dropYOffset;
        GameObject npcToDestroy = currentBurningNpc;

        if (npcToDestroy.name.Contains("Pierre"))
        {
            GameObject BanillaObj = GameObject.FindWithTag("Banilla");
            if (BanillaObj != null)
            {
                Animator BanillaAnim = BanillaObj.GetComponent<Animator>();
                if (BanillaAnim != null) BanillaAnim.SetBool("isCrying", true);
            }
        }

        // =======================================================
        // ★ [추가] 하트를 성공적으로 뽑아냈으니 UI 하트 이미지를 강제로 지웁니다!
        if (PlayerUIScene.instance != null)
        {
            PlayerUIScene.instance.HideUIHeart();
        }

        // NPC가 삭제될 예정이므로 마우스 오버(Hover) 타겟도 비워줍니다.
        if (currentHoveredNpc == npcToDestroy)
        {
            currentHoveredNpc = null;
        }
        // =======================================================

        StopFiring();
        ResumeAllGirls();

        if (droppedHeartPrefab != null)
        {
            GameObject droppedHeart = Instantiate(droppedHeartPrefab, npcPos, Quaternion.identity);
            DroppedHeart heartScript = droppedHeart.GetComponent<DroppedHeart>();
            if (heartScript == null) heartScript = droppedHeart.GetComponentInChildren<DroppedHeart>();

            NpcRandomPatrol patrol = npcToDestroy.GetComponent<NpcRandomPatrol>();
            bool isActuallyPierre = (patrol != null && patrol.isPierre) || npcToDestroy.name.ToLower().Contains("pierre");

            if (isActuallyPierre)
            {
                if (heartScript != null) heartScript.isPierreHeart = true;
                if (patrol != null && patrol.pierreHeartSprite != null)
                {
                    finalHeartSprite = patrol.pierreHeartSprite;
                }
            }

            if (heartScript != null) heartScript.Initialize(finalHeartSprite, npcPos, floorY);
        }

        Destroy(npcToDestroy);
    }

    void UpdateHovering()
    {
        // =======================================================
        // ★ [추가 1] 넉백 중일 때는 마우스 감지를 아예 건너뛰어서 표정(UI_2)을 유지합니다.
        if (isPlayerKnockedBack) return;
        // =======================================================

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && (hit.collider.CompareTag("NPC") || hit.collider.CompareTag("Banilla")))
        {
            GameObject hitNpc = hit.collider.gameObject;
            if (targetPoint != null)
            {
                targetPoint.SetActive(true);
                targetPoint.transform.position = hitNpc.transform.position;
            }

            if (PlayerUIScene.instance != null)
            {
                // 배경 교체
                if (hit.collider.CompareTag("Banilla"))
                {
                    PlayerUIScene.instance.SetBanillaBG();
                }
                else if (hit.collider.CompareTag("NPC"))
                {
                    PlayerUIScene.instance.SetExtraBG();
                }

                // =======================================================
                // ★ [추가 2] 타겟에 커서를 댔으니 초상화를 Checking(UI_Chacking)으로 바꿉니다.
                PlayerUIScene.instance.SetCheckingPortrait();
                // =======================================================
            }

            if (currentHoveredNpc != hitNpc)
            {
                if (currentHoveredNpc != null) HideHeart(currentHoveredNpc);
                currentHoveredNpc = hitNpc;
                ShowOrSpawnHeart(hitNpc);
            }
        }
        else
        {
            if (targetPoint != null) targetPoint.SetActive(false);
            if (currentHoveredNpc != null)
            {
                HideHeart(currentHoveredNpc);
                currentHoveredNpc = null;
            }

            if (PlayerUIScene.instance != null)
            {
                // 허공에 커서를 두면 배경 복구
                PlayerUIScene.instance.SetNormalBG();

                // =======================================================
                // ★ [추가 3] 허공에 커서를 두면 초상화도 기본(UI_IDLE)으로 복구합니다.
                PlayerUIScene.instance.SetIdlePortrait();
                // =======================================================
            }
        }
    }

    private Sprite GetUniqueHeartSprite()
    {
        if (HeartUIManager.instance == null || HeartUIManager.instance.possibleHeartSprites.Length == 0) return null;

        if (heartSpritePool.Count == 0)
        {
            heartSpritePool.AddRange(HeartUIManager.instance.possibleHeartSprites);
            for (int i = 0; i < heartSpritePool.Count; i++)
            {
                Sprite temp = heartSpritePool[i];
                int randomIndex = Random.Range(i, heartSpritePool.Count);
                heartSpritePool[i] = heartSpritePool[randomIndex];
                heartSpritePool[randomIndex] = temp;
            }
        }

        Sprite selectedSprite = heartSpritePool[0];
        heartSpritePool.RemoveAt(0);
        return selectedSprite;
    }

    void ShowOrSpawnHeart(GameObject npc)
    {
        Transform existingHeart = npc.transform.Find("NpcHeartItem");

        // ★ [기존 로직 원상복구] 이미 하트가 있고, 바닐라가 아니라면 
        // 더 이상 계산하지 않고 기존 하트를 그대로 활성화만 시키고 즉시 함수를 끝냅니다!
        if (existingHeart != null && !npc.CompareTag("Banilla"))
        {
            existingHeart.gameObject.SetActive(true);
            if (PlayerUIScene.instance != null)
            {
                SpriteRenderer sr = existingHeart.GetComponent<SpriteRenderer>();
                if (sr != null) PlayerUIScene.instance.SetUIHeart(sr.sprite);
            }
            return;
        }

        SpriteRenderer heartSR = null;
        bool isNewHeart = false;

        // 하트 오브젝트 확보 단계
        if (existingHeart != null)
        {
            existingHeart.gameObject.SetActive(true);
            heartSR = existingHeart.GetComponent<SpriteRenderer>();
        }
        else
        {
            if (heartItemPrefab == null) return;
            Vector3 spawnPos = npc.transform.position;
            GameObject newHeart = Instantiate(heartItemPrefab, spawnPos, Quaternion.identity);
            newHeart.name = "NpcHeartItem";
            newHeart.transform.SetParent(npc.transform);
            newHeart.transform.localPosition = new Vector3(0, 0, -1f);
            newHeart.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

            heartSR = newHeart.GetComponent<SpriteRenderer>();
            isNewHeart = true; // 새로 생성됨을 표시
        }

        // 하트 이미지 결정 단계
        if (heartSR != null)
        {
            // 기본적으로 기존 하트의 이미지를 유지합니다.
            Sprite pickedSprite = heartSR.sprite;

            // 아예 새로 생성하는 경우에만 랜덤 풀이나 피에르 고정 하트를 집어넣습니다.
            if (isNewHeart)
            {
                pickedSprite = GetUniqueHeartSprite();

                NpcRandomPatrol patrol = npc.GetComponent<NpcRandomPatrol>();
                if (patrol != null && patrol.isPierre && patrol.pierreHeartSprite != null)
                {
                    pickedSprite = patrol.pierreHeartSprite;
                }
            }

            // ★ 바닐라 전용 처리: 한 번 하얀 하트로 바뀌면 더 이상 애니메이터를 검사하지 않고 유지합니다.
            if (npc.CompareTag("Banilla"))
            {
                CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
                if (npcManager != null)
                {
                    // 1. 현재 하트가 '하얀 하트'라면? -> 검사 끝! (애니메이터 확인할 필요 없이 유지)
                    if (pickedSprite == npcManager.banillaWhiteHeart && npcManager.banillaWhiteHeart != null)
                    {
                        // do nothing (이미 pickedSprite가 하얀 하트이므로 그대로 내려감)
                    }
                    else
                    {
                        // 2. 아직 하얀 하트가 아닐 때만 한 번 애니메이션 상태를 확인합니다.
                        Animator banillaAnim = npc.GetComponentInChildren<Animator>();
                        if (banillaAnim == null) banillaAnim = npc.GetComponentInParent<Animator>();

                        bool isSmiling = false;
                        if (banillaAnim != null)
                        {
                            isSmiling = banillaAnim.GetCurrentAnimatorStateInfo(0).IsName("Banilla_Smile");
                        }

                        // 웃고 있다면 하얀 하트로 교체! (다음 마우스 Hover부터는 위 1번 조건에 걸려서 이 검사를 안 함)
                        if (isSmiling && npcManager.banillaWhiteHeart != null)
                        {
                            pickedSprite = npcManager.banillaWhiteHeart;
                        }
                        else if (npcManager.banillaBlackHeart != null)
                        {
                            pickedSprite = npcManager.banillaBlackHeart;
                        }
                    }
                }
            }
            if (pickedSprite != null)
            {
                heartSR.sprite = pickedSprite; // NPC 머리 위 하트에 이미지 적용

                // UI 하트 연동
                if (PlayerUIScene.instance != null)
                {
                    PlayerUIScene.instance.SetUIHeart(pickedSprite);
                }
            }
            heartSR.color = new Color(1f, 1f, 1f, 1f);
            heartSR.sortingOrder = 0;
        }
    }

    void HideHeart(GameObject npc)
    {
        Transform existingHeart = npc.transform.Find("NpcHeartItem");
        if (existingHeart != null) existingHeart.gameObject.SetActive(false);

        // ==========================================
        // ★ NPC 머리 위 하트가 꺼질 때 UI 하트도 같이 숨김
        if (PlayerUIScene.instance != null)
        {
            PlayerUIScene.instance.HideUIHeart();
        }
        // ==========================================
    }

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
                targetAnim.SetBool("isWalking", false);
                targetAnim.SetBool("isBurning", true);

                Transform hoverHeart = currentBurningNpc.transform.Find("NpcHeartItem");
                Sprite assignedSprite = null;
                if (hoverHeart != null)
                {
                    assignedSprite = hoverHeart.GetComponent<SpriteRenderer>().sprite;
                    hoverHeart.gameObject.SetActive(false);
                }

                Image[] allImages = currentBurningNpc.GetComponentsInChildren<Image>(true);
                foreach (Image img in allImages)
                {
                    if (img.gameObject.name == "FillImage")
                    {
                        currentFillImage = img;
                        currentFillImage.sprite = assignedSprite;
                        currentFillImage.fillAmount = 0f;
                        break;
                    }
                }
            }
            var move = currentBurningNpc.GetComponent<NpcRandomPatrol>();
            if (move != null) move.enabled = false;

            // ★ [수정] 피에르(보스)일 때와 일반 NPC일 때 방해꾼 호출 방식을 나눕니다.
            if (currentBurningNpc.name.Contains("Pierre"))
            {
                // 1. 타겟이 피에르인 경우: 거리 무시하고 컷신에 스폰된 7명 전원 호출!
                CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
                if (npcManager != null)
                {
                    foreach (GameObject girlObj in npcManager.spawnedGirls)
                    {
                        if (girlObj != null)
                        {
                            GirlNpcReaction girl = girlObj.GetComponent<GirlNpcReaction>();
                            if (girl != null) girl.LookAtAttackedNpc(currentBurningNpc);
                        }
                    }
                }
            }
            else
            {
                // 2. 일반 남학생인 경우: 기존처럼 클릭한 주변(0.5f 반경)의 여학생만 감지!
                Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(lockedTargetPos, 0.5f);
                foreach (Collider2D col in overlappingColliders)
                {
                    GirlNpcReaction girl = col.GetComponentInParent<GirlNpcReaction>();
                    if (girl != null && girl.gameObject != hit.collider.gameObject)
                    {
                        girl.LookAtAttackedNpc(currentBurningNpc);
                    }
                }
            }
        }
    }

    public void ResumeAllGirls()
    {
        GirlNpcReaction[] allGirls = FindObjectsByType<GirlNpcReaction>(FindObjectsSortMode.None);
        foreach (GirlNpcReaction girl in allGirls) girl.ResumeWalking();
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
        isClashMode = false;
        if (clashGaugeObject != null) clashGaugeObject.SetActive(false);

        if (currentBurningNpc != null)
        {
            Animator targetAnim = currentBurningNpc.GetComponent<Animator>();
            NpcRandomPatrol targetMove = currentBurningNpc.GetComponent<NpcRandomPatrol>();

            if (targetAnim != null)
            {
                targetAnim.SetBool("isBurning", false);
                targetAnim.SetBool("isYellowBurn", false);
            }
            if (targetMove != null)
            {
                targetMove.enabled = true;
                targetMove.ResetDirectionAfterReaction();
            }
            if (currentFillImage != null)
            {
                currentFillImage.fillAmount = 0f;
                currentFillImage = null;
            }
            currentBurningNpc = null;
        }
        if (laserObject != null && !isInCompetitionMode) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);

        // =======================================================
        // ★ [추가] 행동이 끝나고 타겟팅이 풀리면 무조건 기본 배경으로 복구
        if (PlayerUIScene.instance != null)
        {
            PlayerUIScene.instance.SetNormalBG();
        }
        // =======================================================
    }

    public void TriggerAllNpcsExit()
    {
        GameObject[] girlNpcs = GameObject.FindGameObjectsWithTag("GirlNpc");
        foreach (GameObject girlObj in girlNpcs)
        {
            GirlNpcReaction girl = girlObj.GetComponent<GirlNpcReaction>();
            if (girl != null) girl.WalkAwayAndDestroy();
        }

        GameObject[] boyNpcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject boyObj in boyNpcs)
        {
            Destroy(boyObj);
        }

        CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
        if (npcManager != null) npcManager.SpawnAndPlayCutscene();
    }

    public void TriggerPierreEnding()
    {
        GameObject[] girlNpcs = GameObject.FindGameObjectsWithTag("GirlNpc");
        foreach (GameObject girlObj in girlNpcs)
        {
            GirlNpcReaction girl = girlObj.GetComponent<GirlNpcReaction>();
            if (girl != null) girl.WalkAwayAndDestroy();
        }

        GameObject[] boyNpcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject boyObj in boyNpcs) Destroy(boyObj);

        CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
        if (npcManager != null) npcManager.OnPlayerGetPierreHeart();
    }

    // ==========================================
    // ★ 바닐라 대결 모드 함수 (SpriteRenderer 대응) ★
    // ==========================================
    public void EnterCompetitionMode(Vector2 target, Color color)
    {
        isInCompetitionMode = true;
        competitionTarget = target;

        if (laserObject != null)
        {
            SpriteRenderer sr = laserObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
        // 대결 중 레이저 두께를 키웁니다.
        laserWidth = 0.1f; 

        if (laserObject != null) laserObject.SetActive(true);
        DrawLaser(competitionTarget);
    }

    public void ExitCompetitionMode()
    {
        isInCompetitionMode = false;

        if (laserObject != null)
        {
            SpriteRenderer sr = laserObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = originalLaserColor;
        }
        // 레이저 두께 원상복구
        laserWidth = originalWidth; 
        
        if (laserObject != null) laserObject.SetActive(false);
    }
}