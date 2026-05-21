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
        // ★ 대결 모드 중일 때는 마우스 클릭을 무시하고 중앙으로만 레이저를 쏩니다.
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
                    playerPinkGauge.fillAmount -= clashDrainSpeed * Time.deltaTime;
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

        if (anim != null) anim.SetTrigger("isKnockback");

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

            if (pickedSprite != null) heartSR.sprite = pickedSprite;
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
        if (laserObject != null && !isInCompetitionMode) laserObject.SetActive(false);
        if (targetPoint != null) targetPoint.SetActive(false);
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
        laserWidth = 0.2f; 

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