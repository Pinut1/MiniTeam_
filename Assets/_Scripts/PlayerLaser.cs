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

    void Start()
    {
        if (laserObject != null) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);

        if (clashGaugeObject != null)
            gaugeRectTransform = clashGaugeObject.GetComponent<RectTransform>();
        mainCam = Camera.main;

        if (playerMoveScript == null) playerMoveScript = GetComponent<PlayerMove>();
    }

    void Update()
    {
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
                    playerPinkGauge.fillAmount -= clashDrainSpeed * Time.deltaTime;
                    if (playerPinkGauge.fillAmount <= 0f)
                    {
                        // 1. 여학생에게 승리 신호를 보내 웃으면서 퇴장하게 만듭니다.
                        LetGirlWinAndLeave();

                        // 2. 남학생을 미리 기억해 둡니다. (StartPlayerKnockback 안에서 정보가 날아가기 때문)
                        GameObject targetToDestroy = currentBurningNpc;

                        // 3. 쇼콜라는 넉백되어 날아갑니다.
                        StartPlayerKnockback();

                        // 4. 빼앗긴 남학생은 하트 없이 그냥 뿅! 삭제시켜버립니다.
                        if (targetToDestroy != null)
                        {
                            // 피에르도 태그가 "npc"로 똑같으니, 
                            // 오브젝트 이름에 "Pierre"가 포함되어 있는지로 피에르를 구별합니다.
                            if (targetToDestroy.name.Contains("Pierre"))
                            {
                                // 피에르일 때는 파괴(Destroy)하지 않고 그냥 리턴해서 살려둡니다.
                                return;
                            }

                            // 피에르가 아닌 일반 남성 NPC만 삭제시킵니다.
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
                            currentFillImage.fillAmount += heartFillSpeed * Time.deltaTime;
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

    void StartPlayerKnockback()
    {
        if (isPlayerKnockedBack) return;
        isPlayerKnockedBack = true;

        float pushDirection = -1f;
        if (currentBurningNpc != null)
        {
            pushDirection = transform.position.x >= currentBurningNpc.transform.position.x ? 1f : -1f;
        }

        // 1. 트리거를 가장 먼저 발동시킵니다. (StopFiring보다 먼저)
        if (anim != null)
        {
            anim.SetTrigger("isKnockback");
        }

        // 2. 그 다음 기존 로직들 실행
        StopFiring();
        ResumeAllGirls();

        // ... (이하 나머지 동일)
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

        // 최종 목적지(targetPos)의 Y축에 방금 만든 오프셋을 더해줘서 바닥에 착 달라붙게 만듭니다.
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

        // 목적지 안착 (이제 붕 뜨지 않고 바닥에 붙어있습니다)
        transform.position = targetPos;
        if (rb != null) rb.position = targetPos;

        yield return new WaitForSeconds(playerGroundStunDuration);

        isPlayerKnockedBack = false;

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

        // 피에르인지 확인
        bool isPierre = currentBurningNpc.name.Contains("Pierre");

        // 주변의 여학생들을 찾아서 처리
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
                    // 피에르라면: 웃으며 퇴장시키지 않고 그냥 원래 상태로 복귀시킴
                    girl.ResumeWalking();
                    Debug.Log("피에르 승리! 여학생 퇴장 없이 순찰 복귀.");
                }
                else
                {
                    // 일반 남학생이라면: 기존처럼 웃으며 퇴장
                    girl.WinAndLeaveScene();
                    Debug.Log("일반 남학생 패배! 여학생 웃으며 퇴장.");
                }
            }
        }

        // ★ 중요: 남학생 삭제 로직도 분리해야 합니다.
        if (!isPierre)
        {
            // 일반 남학생은 여학생이 이기면 삭제
            Destroy(currentBurningNpc);
        }
        else
        {
            // 피에르는 삭제하지 않고 순찰 복귀 (ResumeAllGirls가 돌아가면서 자동 복귀됨)
            // 만약 피에르에게도 별도의 '순찰 복귀' 로직이 필요하다면 여기서 호출하세요.
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

        // 1. 피에르 하트 성공 시 바닐라 울기 처리
        if (npcToDestroy.name.Contains("Pierre"))
        {
            GameObject BanillaObj = GameObject.FindWithTag("Banilla");
            if (BanillaObj != null)
            {
                Animator BanillaAnim = BanillaObj.GetComponent<Animator>();
                if (BanillaAnim != null)
                {
                    BanillaAnim.SetBool("isCrying", true);
                }
            }
        }

        // 2. 기본 정리 작업 (레이저 멈춤 및 여학생 복귀)
        StopFiring();
        ResumeAllGirls();

        // 3. 하트 생성 로직 (피에르라면 무지개 하트로 설정)
        if (droppedHeartPrefab != null)
        {
            GameObject droppedHeart = Instantiate(droppedHeartPrefab, npcPos, Quaternion.identity);
            DroppedHeart heartScript = droppedHeart.GetComponent<DroppedHeart>();
            if (heartScript == null) heartScript = droppedHeart.GetComponentInChildren<DroppedHeart>();

            // NPC의 순찰 스크립트를 가져와서 피에르인지 확실하게 체크합니다.
            NpcRandomPatrol patrol = npcToDestroy.GetComponent<NpcRandomPatrol>();

            // 방법 A: 스크립트의 isPierre 체크박스가 켜져있거나
            // 방법 B: 오브젝트 이름에 pierre(대소문자 무시)가 들어가면 피에르로 인정!
            bool isActuallyPierre = (patrol != null && patrol.isPierre) || npcToDestroy.name.ToLower().Contains("pierre");

            if (isActuallyPierre)
            {
                // 1) 하트 스크립트에 피에르 하트라고 마킹 찍기!
                if (heartScript != null)
                {
                    heartScript.isPierreHeart = true;
                    Debug.Log("SuccessAndDropHeart: 피에르 하트 마킹 완료 (isPierreHeart = true)");
                }

                // 2) 무지개 하트 스프라이트로 덮어씌우기
                if (patrol != null && patrol.pierreHeartSprite != null)
                {
                    finalHeartSprite = patrol.pierreHeartSprite;
                }
            }

            if (heartScript != null) heartScript.Initialize(finalHeartSprite, npcPos, floorY);
        }

        // 4. 소멸 처리 (피에르 여부와 관계없이 무조건 삭제)
        Destroy(npcToDestroy);
    }

    void UpdateHovering()
    {
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
        }
    }

    // 중복 없는 하트 이미지를 반환하는 제비뽑기 함수
    private Sprite GetUniqueHeartSprite()
    {
        if (HeartUIManager.instance == null || HeartUIManager.instance.possibleHeartSprites.Length == 0) return null;

        // 주머니가 비었으면 매니저에 있는 이미지들을 다시 채우고 섞어줍니다.
        if (heartSpritePool.Count == 0)
        {
            heartSpritePool.AddRange(HeartUIManager.instance.possibleHeartSprites);

            // 리스트 섞기 (Fisher-Yates Shuffle)
            for (int i = 0; i < heartSpritePool.Count; i++)
            {
                Sprite temp = heartSpritePool[i];
                int randomIndex = Random.Range(i, heartSpritePool.Count);
                heartSpritePool[i] = heartSpritePool[randomIndex];
                heartSpritePool[randomIndex] = temp;
            }
        }

        // 섞인 주머니에서 첫 번째 이미지를 꺼내고 제거합니다.
        Sprite selectedSprite = heartSpritePool[0];
        heartSpritePool.RemoveAt(0);
        return selectedSprite;
    }

    // 마우스를 올렸을 때 무작위가 아닌 제비뽑기 색상을 적용하도록 변경
    void ShowOrSpawnHeart(GameObject npc)
    {
        Transform existingHeart = npc.transform.Find("NpcHeartItem");
        if (existingHeart != null) { existingHeart.gameObject.SetActive(true); return; }
        if (heartItemPrefab == null) return;

        Vector3 spawnPos = npc.transform.position;
        GameObject newHeart = Instantiate(heartItemPrefab, spawnPos, Quaternion.identity);
        newHeart.name = "NpcHeartItem";
        newHeart.transform.SetParent(npc.transform);
        newHeart.transform.localPosition = new Vector3(0, 0, -1f);
        newHeart.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        SpriteRenderer heartSR = newHeart.GetComponent<SpriteRenderer>();
        if (heartSR != null)
        {
            Sprite pickedSprite = GetUniqueHeartSprite();

            // 만약 마우스를 올린 대상이 피에르라면 무조건 무지개 하트로 덮어씁니다!
            NpcRandomPatrol patrol = npc.GetComponent<NpcRandomPatrol>();
            if (patrol != null && patrol.isPierre && patrol.pierreHeartSprite != null)
            {
                pickedSprite = patrol.pierreHeartSprite;
            }

            if (npc.CompareTag("Banilla"))
            {
                BanillaNpcManager banillaManager = npc.GetComponent<BanillaNpcManager>();
                if (banillaManager != null && banillaManager.heartSprite != null)
                {
                    pickedSprite = banillaManager.heartSprite;
                }
            }

            if (pickedSprite != null)
            {
                heartSR.sprite = pickedSprite;
            }
            heartSR.color = new Color(1f, 1f, 1f, 1f);
            heartSR.sortingOrder = 0;
        }
    }

    void HideHeart(GameObject npc)
    {
        Transform existingHeart = npc.transform.Find("NpcHeartItem");
        if (existingHeart != null) existingHeart.gameObject.SetActive(false);
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
                        currentFillImage.fillAmount = 0f;
                        break;
                    }
                }
            }
            var move = currentBurningNpc.GetComponent<NpcRandomPatrol>();
            if (move != null) move.enabled = false;

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
        if (laserObject != null) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);
    }

    public void TriggerAllNpcsExit()
    {
        GameObject[] girlNpcs = GameObject.FindGameObjectsWithTag("GirlNpc");
        foreach (GameObject girlObj in girlNpcs)
        {
            GirlNpcReaction girl = girlObj.GetComponent<GirlNpcReaction>();
            if (girl != null)
            {
                girl.WalkAwayAndDestroy();
            }
        }

        GameObject[] boyNpcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject boyObj in boyNpcs)
        {
            Destroy(boyObj);
        }

        // 스폰 및 시네마머신 컷신 실행
        CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
        if (npcManager != null)
        {
            npcManager.SpawnAndPlayCutscene();
        }
    }

    public void TriggerPierreEnding()
    {
        // 1. 엔딩 컷신 시작 전, 화면에 남은 일반 NPC들을 싹 정리해 줍니다.
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

        // 2. 오프닝(SpawnAndPlayCutscene)이 아닌 엔딩 컷신을 호출합니다!
        CutsceneNpcManager npcManager = FindAnyObjectByType<CutsceneNpcManager>();
        if (npcManager != null)
        {
            npcManager.OnPlayerGetPierreHeart(); // 방금 만든 피에르 엔딩 카메라 연출 시작
        }
    }
}