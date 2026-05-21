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

    [Header("레이저 경쟁 설정")]
    public Color playerCompetitionLaserColor = new Color(1f, 0.4f, 0.7f);

    private GameObject banillaLaserObj;
    private List<GameObject> spawnedGirls = new List<GameObject>();
    private GameObject instanceBanilla;
    private Animator banillaAnimator;
    private Transform banillaTransform;

    private bool isLaserDuelActive = false;
    private Vector2 duelMidwayPoint;

    public bool canStartDuel = false;

    // 1. 시작 시 레이저 강제 숨김
    void Start()
    {
        ForceHideBanillaLaser();
    }

    private void ForceHideBanillaLaser()
    {
        GameObject banillaObj = GameObject.FindWithTag("Banilla");
        if (banillaObj != null)
        {
            banillaTransform = banillaObj.transform;
            if (banillaAnimator == null) banillaAnimator = banillaObj.GetComponentInChildren<Animator>();

            Transform[] allChildren = banillaObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == "Laser_Yellow_0")
                {
                    banillaLaserObj = child.gameObject;
                    banillaLaserObj.SetActive(false);
                    break;
                }
            }
        }
    }

    // 2. 오프닝 컷신
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
                Vector3 spawnPos = new Vector3(banillaSpawnPosition.x + ((i + 1) * girlSpawnSpacing), pierreSpawnPosition.y - 1.1f, 0);
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

        yield return new WaitForSeconds(4.5f);

        vcamBanilla.gameObject.SetActive(true);
        vcamPierre.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.0f);

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

    // 3. 서브 유틸 함수들
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
            foreach (GameObject girl in spawnedGirls)
            {
                if (girl != null)
                {
                    Vector3 targetPos = new Vector3(pierreSpawnPosition.x, girl.transform.position.y, 0);
                    girl.transform.position = Vector3.MoveTowards(
                        girl.transform.position,
                        targetPos,
                        girlWalkSpeed * Time.deltaTime
                    );
                    if (Mathf.Abs(girl.transform.position.x - targetPos.x) > 0.05f)
                    {
                        allArrived = false;
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
            SetBanillaCrying(false);
            banillaAnimator.SetTrigger("toSmile");
        }
    }

    // =========================================================================
    // ★ 제가 멍청하게 지워버렸던 엔딩 컷신 함수 (완벽 복구) ★
    // =========================================================================
    public void OnPlayerGetPierreHeart()
    {
        canStartDuel = false;
        ForceHideBanillaLaser();
        StartCoroutine(PierreHeartCutsceneSequence());
    }

    private IEnumerator PierreHeartCutsceneSequence()
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
        yield return new WaitForSeconds(2.5f);
        yield return new WaitForSeconds(1.0f);

        SetBanillaCrying(true);

        vcamPlayer.gameObject.SetActive(false);
        vcamBanilla.gameObject.SetActive(true);
        yield return new WaitForSeconds(6f);

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
        Debug.Log("엔딩 컷신 종료, 바닐라를 클릭하여 대결을 시작할 수 있습니다!");
        yield return null;
    }

    // =========================================================================
    // ★ 클릭 시 레이저 대결 시작 함수 ★
    // =========================================================================
    public void StartLaserDuel()
    {
        if (isLaserDuelActive || banillaTransform == null) return;

        isLaserDuelActive = true;
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
            try { playerAnim.Play("BackAttack"); } catch { }
        }

        if (banillaAnimator != null)
        {
            banillaAnimator.SetBool("isCrying", false);
            try { banillaAnimator.Play("Banilla_Idle"); } catch { }
        }

        Vector3 playerFirePos = playerLaserScript != null && playerLaserScript.firePoint != null ? playerLaserScript.firePoint.position : playerRb.transform.position;
        Vector3 banillaFirePos = banillaLaserObj != null ? banillaLaserObj.transform.position : banillaTransform.position;
        duelMidwayPoint = Vector2.Lerp(playerFirePos, banillaFirePos, 0.5f);

        if (playerLaserScript != null)
        {
            playerLaserScript.EnterCompetitionMode(duelMidwayPoint, playerCompetitionLaserColor);
            Debug.Log("플레이어 레이저 발사 성공!");
        }
        else
        {
            Debug.LogError("[치명적 오류] 플레이어 레이저가 나가지 않습니다! CutSceneManager 인스펙터의 'Player Laser Script' 칸이 비어있습니다. 하이어라키의 Player 오브젝트를 드래그해서 넣어주세요!");
        }

        if (banillaLaserObj != null)
        {
            FireBanillaLaser(banillaFirePos, duelMidwayPoint);
            Debug.Log("바닐라 레이저 발사 성공!");
        }
        else
        {
            Debug.LogError("[치명적 오류] 바닐라 레이저가 나가지 않습니다! 바닐라 안에서 'Laser_Yellow_0'을 찾지 못했습니다.");
        }
    }

    private void FireBanillaLaser(Vector3 startPos, Vector3 targetPos)
{
    if (banillaLaserObj == null) return;
    
    banillaLaserObj.SetActive(true);

    SpriteRenderer sr = banillaLaserObj.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.enabled = true;
        sr.sortingLayerName = "Foreground"; // 배경보다 무조건 앞! (없으면 "Default"로 유지)
        sr.sortingOrder = 999;             // 999번으로 맨 앞으로!
    }

    // (기존 위치/크기 조절 코드 그대로 유지...)
    Vector2 direction = targetPos - startPos;
    float distance = direction.magnitude;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    banillaLaserObj.transform.rotation = Quaternion.Euler(0, 0, angle);
    
    float baseWidth = sr != null && sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
    float currentYScale = banillaLaserObj.transform.localScale.y; 
    banillaLaserObj.transform.localScale = new Vector3(distance / baseWidth, currentYScale, 1f);
}

    // =========================================================================
    // ★ 기존 일반 대결용 함수들 ★
    // =========================================================================
    public void StartBanillaCompetition() { SetBanillaCrying(false); }
    public void OnPlayerLoseCompetition() { SetBanillaCrying(true); ExitLaserDuel(); }
    public void OnPlayerWinCompetition() { TriggerBanillaSmile(); ExitLaserDuel(); }

    private void ExitLaserDuel()
    {
        isLaserDuelActive = false;
        if (playerLaserScript != null) playerLaserScript.ExitCompetitionMode();
        if (banillaLaserObj != null) banillaLaserObj.SetActive(false);
    }
}