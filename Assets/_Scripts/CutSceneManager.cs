using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneNpcManager : MonoBehaviour
{
    [Header("스폰할 NPC 프리팹")]
    public GameObject pierrePrefab;
    public GameObject banillaPrefab;
    public GameObject[] newGirlPrefabs;
    public float girlSpawnSpacing = 1.5f;

    [Header("스폰 좌표")]
    public Vector3 pierreSpawnPosition;
    public Vector3 banillaSpawnPosition;

    [Header("시네마머신 카메라 설정")]
    public CinemachineBrain mainBrain;
    public CinemachineCamera vcamPierre;
    public CinemachineCamera vcamBanilla;
    public CinemachineCamera vcamGirlsWalk;
    public CinemachineCamera vcamPlayer;

    [Header("플레이어 제어")]
    public MonoBehaviour playerMoveScript;
    public Rigidbody2D playerRb;
    public PlayerLaser playerLaserScript;

    [Header("플레이어 애니메이션 (직접 연결)")]
    public Animator playerAnim;

    [Header("여학생 이동 설정")]
    public float girlWalkSpeed = 3f;
    public float girlStopSpacing = 1.0f;
    public float girlYSpread = 0.3f;
    public float distanceToPierre = 1.5f;

    [Header("레이저 경쟁 설정")]
    public Color playerCompetitionLaserColor = new Color(1f, 0.4f, 0.7f);

    [Tooltip("플레이어(쇼콜라)가 한 번 연타할 때 밀어내는 힘")]
    public float playerPushPower = 0.05f;
    [Tooltip("가만히 있을 때 바닐라가 밀고 들어오는 속도 (초당)")]
    public float banillaPushSpeed = 0.5f;

    private GameObject banillaLaserObj;
    private List<GameObject> banillaLaserEffects = new List<GameObject>();
    public List<GameObject> spawnedGirls = new List<GameObject>();
    private GameObject instanceBanilla;
    private Animator banillaAnimator;
    private Transform banillaTransform;

    [Header("바닐라 대결 전용 하트 이미지 (승리 후 교체용)")]
    public Sprite banillaBlackHeart;
    public Sprite banillaWhiteHeart;

    private bool isLaserDuelActive = false;
    private Vector2 duelMidwayPoint;

    private float duelProgress = 0.5f;

    public bool canStartDuel = false;

    [Header("마술봉 제어")]
    public Animator magicStickAnim;
    public Animator banillaStickAnim;

    void Start()
    {
        ForceHideBanillaLaser();
    }

    void Update()
    {
        if (!isLaserDuelActive && banillaLaserObj != null && banillaLaserObj.activeSelf)
        {
            banillaLaserObj.SetActive(false);
        }

        if (!isLaserDuelActive) return;

        duelProgress -= banillaPushSpeed * Time.deltaTime;
        duelProgress = Mathf.Clamp(duelProgress, 0f, 1f);

        UpdateDuelLasers();

        if (duelProgress <= 0.0f)
        {
            Debug.Log("바닐라 승리! (쇼콜라가 밀림)");
            OnPlayerLoseCompetition();
        }
    }

    void LateUpdate()
    {
        if (!isLaserDuelActive)
        {
            if (banillaLaserObj != null && banillaLaserObj.activeSelf)
                banillaLaserObj.SetActive(false);

            foreach (GameObject effect in banillaLaserEffects)
            {
                if (effect != null && effect.activeSelf)
                    effect.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject effect in banillaLaserEffects)
            {
                if (effect != null)
                {
                    Vector3 newPos = duelMidwayPoint;
                    newPos.z = effect.transform.position.z;
                    effect.transform.position = newPos;
                }
            }
        }
    }

    private void UpdateDuelLasers()
    {
        if (banillaTransform == null || playerRb == null) return;

        Vector3 playerFirePos = playerLaserScript != null && playerLaserScript.firePoint != null
            ? playerLaserScript.firePoint.position
            : playerRb.transform.position;
        Vector3 banillaFirePos = banillaLaserObj != null
            ? banillaLaserObj.transform.position
            : banillaTransform.position;

        duelMidwayPoint = Vector2.Lerp(playerFirePos, banillaFirePos, duelProgress);

        if (playerLaserScript != null)
        {
            playerLaserScript.EnterCompetitionMode(duelMidwayPoint, playerCompetitionLaserColor);
        }

        if (banillaLaserObj != null)
        {
            FireBanillaLaser(banillaFirePos, duelMidwayPoint);
        }

        if (banillaLaserEffects != null)
        {
            foreach (GameObject effect in banillaLaserEffects)
            {
                if (effect != null)
                {
                    Vector3 newPos = duelMidwayPoint;
                    newPos.z = effect.transform.position.z;
                    effect.transform.position = newPos;
                }
            }
        }
    }

    private void ForceHideBanillaLaser()
    {
        GameObject banillaObj = instanceBanilla;
        if (banillaObj == null) banillaObj = GameObject.FindWithTag("Banilla");

        if (banillaObj != null)
        {
            banillaTransform = banillaObj.transform;
            if (banillaAnimator == null) banillaAnimator = banillaObj.GetComponentInChildren<Animator>();

            banillaLaserEffects.Clear();

            Transform[] allChildren = banillaObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == "Laser_Yellow_0")
                {
                    banillaLaserObj = child.gameObject;
                    banillaLaserObj.SetActive(false);
                }
                else if (child.name == "44x_0" || child.name == "54x_0")
                {
                    child.gameObject.SetActive(false);
                    banillaLaserEffects.Add(child.gameObject);
                }
            }
        }
    }

    public void SpawnAndPlayCutscene()
    {
        spawnedGirls.Clear();
        banillaAnimator = null;
        banillaTransform = null;

        if (pierrePrefab != null) Instantiate(pierrePrefab, pierreSpawnPosition, Quaternion.identity);

        if (banillaPrefab != null)
        {
            instanceBanilla = Instantiate(banillaPrefab, banillaSpawnPosition, Quaternion.identity);
            ForceHideBanillaLaser();
            SetBanillaCrying(false);
        }

        if (newGirlPrefabs != null)
        {
            for (int i = 0; i < newGirlPrefabs.Length; i++)
            {
                float zigzagY = 0f;
                if (i != 0)
                {
                    zigzagY = (i % 2 == 0) ? girlYSpread : -girlYSpread;
                }

                float yOffset = -1.5f;

                Vector3 spawnPos = new Vector3(
                    banillaSpawnPosition.x + ((i + 1) * girlSpawnSpacing),
                    pierreSpawnPosition.y + yOffset + zigzagY,
                    0
                );

                GameObject girl = Instantiate(newGirlPrefabs[i], spawnPos, Quaternion.identity);
                SetGirlAIEnabled(girl, false);
                spawnedGirls.Add(girl);
            }
        }
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        vcamPlayer.gameObject.SetActive(false);
        vcamPierre.gameObject.SetActive(false);
        vcamBanilla.gameObject.SetActive(false);
        vcamGirlsWalk.gameObject.SetActive(false);

        playerMoveScript.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isIdle", true);
            playerAnim.Play("Idle");
        }

        Vector3 playerScale = playerRb.transform.localScale;
        playerScale.x = -Mathf.Abs(playerScale.x);
        playerRb.transform.localScale = playerScale;
        SyncPlayerMoveDirection(false);

        vcamPlayer.gameObject.SetActive(true);
        if (mainBrain != null) mainBrain.enabled = true;

        yield return new WaitForSeconds(2f);

        vcamPierre.gameObject.SetActive(true);
        vcamPlayer.gameObject.SetActive(false);

        yield return new WaitForSeconds(4f);

        vcamBanilla.gameObject.SetActive(true);
        vcamPierre.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        if (spawnedGirls.Count > 0 && spawnedGirls[0] != null)
        {
            vcamGirlsWalk.Follow = spawnedGirls[0].transform;
        }

        vcamGirlsWalk.gameObject.SetActive(true);
        vcamBanilla.gameObject.SetActive(false);

        yield return StartCoroutine(GirlsWalkToPierre());

        vcamPlayer.gameObject.SetActive(true);
        vcamGirlsWalk.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.0f);

        if (playerAnim != null)
        {
            playerAnim.SetBool("isIdle", false);
        }

        SyncPlayerMoveDirection(false);
        if (mainBrain != null) mainBrain.enabled = false;
        playerMoveScript.enabled = true;

        foreach (GameObject girl in spawnedGirls)
        {
            if (girl != null) SetGirlAIEnabled(girl, true);
        }
    }

    private void SyncPlayerMoveDirection(bool faceRight)
    {
        if (playerMoveScript == null) return;
        try
        {
            System.Type type = playerMoveScript.GetType();
            System.Reflection.FieldInfo[] fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(bool))
                {
                    string name = field.Name.ToLower();
                    if (name.Contains("right") || name.Contains("facing") || name.Contains("face"))
                    {
                        field.SetValue(playerMoveScript, faceRight);
                    }
                }
            }
        }
        catch { }
    }

    private IEnumerator GirlsWalkToPierre()
    {
        foreach (GameObject girl in spawnedGirls)
        {
            if (girl != null)
            {
                Animator anim = girl.GetComponent<Animator>();
                if (anim != null) anim.SetBool("isWalking", true);
            }
        }

        bool allArrived = false;
        while (!allArrived)
        {
            allArrived = true;

            for (int i = 0; i < spawnedGirls.Count; i++)
            {
                GameObject girl = spawnedGirls[i];
                if (girl != null)
                {
                    float targetX = pierreSpawnPosition.x - distanceToPierre - ((spawnedGirls.Count - 1 - i) * girlStopSpacing);

                    Vector3 targetPos = new Vector3(targetX, girl.transform.position.y, 0);

                    girl.transform.position = Vector3.MoveTowards(
                        girl.transform.position,
                        targetPos,
                        girlWalkSpeed * Time.deltaTime
                    );

                    if (Mathf.Abs(girl.transform.position.x - targetPos.x) > 0.05f)
                    {
                        allArrived = false;
                    }
                    else
                    {
                        Animator anim = girl.GetComponent<Animator>();
                        if (anim != null) anim.SetBool("isWalking", false);
                    }
                }
            }
            yield return null;
        }
    }

    private void SetGirlAIEnabled(GameObject girl, bool isEnabled)
    {
        MonoBehaviour[] scripts = girl.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            string n = s.GetType().Name;
            if (n == "GirlNpcController" || n == "GirlNpcReaction" || n == "NpcRandomPatrol" || n == "NpcMove")
                s.enabled = isEnabled;
        }
    }

    private void SetBanillaCrying(bool isCrying)
    {
        if (banillaAnimator != null)
        {
            banillaAnimator.SetBool("isCrying", isCrying);
        }
        else
        {
            GameObject banillaObj = GameObject.FindWithTag("Banilla");
            if (banillaObj != null)
            {
                banillaAnimator = banillaObj.GetComponent<Animator>();
                if (banillaAnimator != null) banillaAnimator.SetBool("isCrying", isCrying);
            }
        }
    }

    private void TriggerBanillaSmile()
    {
        if (banillaAnimator != null)
        {
            banillaAnimator.SetTrigger("toSmile");

            try
            {
                banillaAnimator.Play("Banilla_Smile");
                Debug.Log("바닐라 미소 애니메이션(Banilla_Smile) 강제 재생 완료!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("애니메이션 강제 재생 실패: " + e.Message);
            }
        }
    }

    public void OnPlayerGetPierreHeart(Vector3 heartPos)
    {
        canStartDuel = false;
        ForceHideBanillaLaser();
        StartCoroutine(PierreHeartCutsceneSequence(heartPos));
    }

    private IEnumerator PierreHeartCutsceneSequence(Vector3 heartPos)
    {
        playerMoveScript.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isIdle", true);
            playerAnim.Play("Idle");
        }

        Vector3 playerScale = playerRb.transform.localScale;
        playerScale.x = -Mathf.Abs(playerScale.x);
        playerRb.transform.localScale = playerScale;
        SyncPlayerMoveDirection(false);

        if (mainBrain != null) mainBrain.enabled = true;
        vcamBanilla.gameObject.SetActive(false);
        vcamPierre.gameObject.SetActive(false);
        vcamGirlsWalk.gameObject.SetActive(false);

        vcamPlayer.gameObject.SetActive(true);

        if (magicStickAnim != null)
        {
            Vector3 centerPos = Camera.main.transform.position;
            centerPos.z = magicStickAnim.transform.position.z;

            magicStickAnim.transform.position = centerPos;

            magicStickAnim.gameObject.SetActive(true);
            if (magicStickAnim.isActiveAndEnabled)
            {
                magicStickAnim.Play("MagicStick");
            }
        }

        yield return new WaitForSeconds(2f);

        if (magicStickAnim != null && magicStickAnim.isActiveAndEnabled)
        {
            magicStickAnim.Play("Chocolate_Stick");
        }

        yield return new WaitForSeconds(2f);

        if (HeartUIManager.instance != null)
        {
            HeartUIManager.instance.ChangeHeartToStickUI(true);
        }

        if (magicStickAnim != null)
        {
            magicStickAnim.gameObject.SetActive(false);
        }

        SetBanillaCrying(true);

        vcamPlayer.gameObject.SetActive(false);
        vcamBanilla.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);

        vcamBanilla.gameObject.SetActive(false);
        vcamPlayer.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);

        if (playerAnim != null)
        {
            playerAnim.SetBool("isIdle", false);
        }

        SyncPlayerMoveDirection(false);
        if (mainBrain != null) mainBrain.enabled = false;

        canStartDuel = true;

        try
        {
            if (playerMoveScript != null)
            {
                playerMoveScript.enabled = true;
                playerMoveScript.gameObject.SendMessage("ForceWakeUpInputInit", SendMessageOptions.DontRequireReceiver);
            }
        }
        catch { }
        yield return null;
    }

    public void StartLaserDuel()
    {
        if (!canStartDuel && !isLaserDuelActive)
        {
            Debug.Log("이미 대결이 승리로 종료되었습니다.");
            return;
        }
        if (banillaTransform == null) return;

        if (isLaserDuelActive)
        {
            duelProgress += playerPushPower;
            duelProgress = Mathf.Clamp(duelProgress, 0f, 1f);
            UpdateDuelLasers();
            if (duelProgress >= 1.0f)
            {
                Debug.Log("쇼콜라 연타로 1.0 도달! 승리 함수 즉시 실행!");
                OnPlayerWinCompetition();
            }
            return;
        }

        isLaserDuelActive = true;
        duelProgress = 0.5f;
        Debug.Log("플레이어 vs 바닐라 레이저 경쟁 시작!");

        if (playerMoveScript != null) playerMoveScript.enabled = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        Vector3 playerScale = playerRb.transform.localScale;
        if (banillaTransform.position.x < playerRb.transform.position.x)
        {
            playerScale.x = -Mathf.Abs(playerScale.x);
            SyncPlayerMoveDirection(false);
        }
        else
        {
            playerScale.x = Mathf.Abs(playerScale.x);
            SyncPlayerMoveDirection(true);
        }
        playerRb.transform.localScale = playerScale;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isIdle", false);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isAttacking", true);

            try { playerAnim.Play("BackAttack"); } catch { }
        }

        if (banillaAnimator != null)
        {
            banillaAnimator.SetBool("isCrying", false);
            try { banillaAnimator.Play("Banilla_Idle"); } catch { }
        }

        UpdateDuelLasers();
    }

    private void FireBanillaLaser(Vector3 startPos, Vector3 targetPos)
    {
        if (banillaLaserObj == null) return;

        banillaLaserObj.SetActive(true);
        foreach (GameObject effect in banillaLaserEffects)
        {
            if (effect != null) effect.SetActive(true);
        }

        SpriteRenderer sr = banillaLaserObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }

        Vector2 direction = targetPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        banillaLaserObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        float baseWidth = sr != null && sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
        float currentYScale = banillaLaserObj.transform.localScale.y;

        float parentScaleX = 1f;
        if (banillaLaserObj.transform.parent != null && banillaLaserObj.transform.parent.lossyScale.x != 0)
        {
            parentScaleX = Mathf.Abs(banillaLaserObj.transform.parent.lossyScale.x);
        }

        float finalXScale = (distance / baseWidth) / parentScaleX;

        float currentZScale = banillaLaserObj.transform.localScale.z;
        banillaLaserObj.transform.localScale = new Vector3(finalXScale, currentYScale, currentZScale);
    }

    public void StartBanillaCompetition() { SetBanillaCrying(false); }

    public void OnPlayerLoseCompetition()
    {
        SetBanillaCrying(true);
        ExitLaserDuel();

        if (playerLaserScript != null && banillaTransform != null)
        {
            playerLaserScript.StartPlayerKnockback(banillaTransform);
            Debug.Log("쇼콜라 패배! 넉백 애니메이션 실행");
        }
    }

    public void OnPlayerWinCompetition()
    {
        canStartDuel = false;
        ExitLaserDuel();
        TriggerBanillaSmile();

        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true;

            try
            {
                playerMoveScript.gameObject.SendMessage("ForceWakeUpInputInit", SendMessageOptions.DontRequireReceiver);
            }
            catch { }
        }

        if (playerAnim != null)
        {
            playerAnim.SetBool("isAttacking", false);
            playerAnim.SetBool("isIdle", true);
        }

        StartCoroutine(BanillaFadeOutAndDropHeart());
    }

    private IEnumerator BanillaFadeOutAndDropHeart()
    {
        GameObject banillaObj = instanceBanilla;
        if (banillaObj == null) banillaObj = GameObject.FindWithTag("Banilla");
        if (banillaObj == null) yield break;

        yield return new WaitForSeconds(0.3f);

        SpriteRenderer[] renderers = banillaObj.GetComponentsInChildren<SpriteRenderer>();

        float fadeDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        Vector3 dropPos = banillaObj.transform.position;

        float floorY = playerRb.transform.position.y - 1.5f;
        if (playerLaserScript != null)
        {
            floorY = playerRb.transform.position.y + playerLaserScript.dropYOffset;
        }

        if (playerLaserScript != null && playerLaserScript.droppedHeartPrefab != null && banillaWhiteHeart != null)
        {
            GameObject droppedHeart = Instantiate(playerLaserScript.droppedHeartPrefab, dropPos, Quaternion.identity);

            DroppedHeart heartScript = droppedHeart.GetComponent<DroppedHeart>();
            if (heartScript == null) heartScript = droppedHeart.GetComponentInChildren<DroppedHeart>();

            if (heartScript != null)
            {
                heartScript.isBanillaWhiteHeart = true;
                heartScript.Initialize(banillaWhiteHeart, dropPos, floorY);
            }
        }

        Destroy(banillaObj);

        Debug.Log("바닐라 정화 완료: 페이드아웃 및 하얀 하트 드롭 성공!");
    }

    public void OnPlayerGetBanillaHeart()
    {
        StartCoroutine(BanillaHeartCutsceneSequence());
    }

    private IEnumerator BanillaHeartCutsceneSequence()
    {
        // 1. 쇼콜라 강제 정지
        playerMoveScript.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isIdle", true);
            playerAnim.Play("Idle");
        }

        vcamPlayer.gameObject.SetActive(true);

        // 2. 바닐라 요술봉을 화면 중앙으로 부르고 MagicStick_B 실행
        if (banillaStickAnim != null)
        {
            Vector3 centerPos = Camera.main.transform.position;
            centerPos.z = banillaStickAnim.transform.position.z;
            banillaStickAnim.transform.position = centerPos;

            banillaStickAnim.gameObject.SetActive(true);
            if (banillaStickAnim.isActiveAndEnabled)
            {
                banillaStickAnim.Play("MagicStick_B");
            }
        }

        // 3. 3초 대기
        yield return new WaitForSeconds(3f);

        // 4. Banilla_Stick 애니메이션으로 강제 전환
        if (banillaStickAnim != null && banillaStickAnim.isActiveAndEnabled)
        {
            banillaStickAnim.Play("Banilla_Stick");
        }

        // 5. 2초 대기 후 요술봉 UI 교체
        yield return new WaitForSeconds(2f);

        if (HeartUIManager.instance != null)
            HeartUIManager.instance.ChangeHeartToStickUI(false);

        if (banillaStickAnim != null)
        {
            banillaStickAnim.gameObject.SetActive(false);
        }

        // ★★★ [추가] 두 요술봉 화면 중앙 확대 연출 ★★★
        yield return new WaitForSeconds(0.5f);

        bool zoomComplete = false;

        if (HeartUIManager.instance != null)
        {
            HeartUIManager.instance.PlaySticksZoomToCenter(() => {
                zoomComplete = true;
            });
        }
        else
        {
            zoomComplete = true;
        }

        // 확대 연출이 끝날 때까지 대기
        yield return new WaitUntil(() => zoomComplete);

        // 확대된 상태 잠시 유지
        yield return new WaitForSeconds(1f);

        // 미니게임 클리어 처리
        if (MiniTeam.Core.MiniGameManager.Instance != null)
        {
            MiniTeam.Core.MiniGameManager.Instance.OnMiniGameClear();
        }
    }

    private void ExitLaserDuel()
    {
        isLaserDuelActive = false;

        if (playerLaserScript != null)
        {
            playerLaserScript.ExitCompetitionMode();
        }

        if (banillaLaserObj != null)
        {
            banillaLaserObj.SetActive(false);
        }

        foreach (GameObject effect in banillaLaserEffects)
        {
            if (effect != null) effect.SetActive(false);
        }

        if (playerAnim != null)
        {
            playerAnim.SetBool("isAttacking", false);
        }
    }
}
