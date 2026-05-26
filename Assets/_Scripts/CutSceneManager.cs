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
    public float playerPushPower = 0.04f;
    [Tooltip("가만히 있을 때 바닐라가 밀고 들어오는 속도 (초당)")]
    public float banillaPushSpeed = 0.15f;

    private GameObject banillaLaserObj;
    private List<GameObject> banillaLaserEffects = new List<GameObject>();
    public List<GameObject> spawnedGirls = new List<GameObject>();
    private GameObject instanceBanilla;
    private Animator banillaAnimator;
    private Transform banillaTransform;

    // (기존 변수들 아래쪽 적당한 곳에 추가하세요)
    [Header("바닐라 대결 전용 하트 이미지 (승리 후 교체용)")]
    public Sprite banillaBlackHeart; // 원래의 검은 하트
    public Sprite banillaWhiteHeart; // 이겼을 때 하얀 하트

    private bool isLaserDuelActive = false;
    private Vector2 duelMidwayPoint;

    // 줄다리기 진행도 (0.0f = 바닐라 완승 / 0.5f = 정중앙 시작 / 1.0f = 쇼콜라 완승)
    private float duelProgress = 0.5f;

    public bool canStartDuel = false;

    // 1. 시작 시 레이저 강제 숨김
    void Start()
    {
        ForceHideBanillaLaser();
    }

    // 대결 중일 때 실시간으로 레이저 줄다리기 계산 및 애니메이션 트리거 감지
    void Update()
    {
        // ★ 대결 중이 아닐 때는 레이저가 켜져 있다면 애니메이션이 켜더라도 무조건 강제로 끕니다.
        if (!isLaserDuelActive && banillaLaserObj != null && banillaLaserObj.activeSelf)
        {
            banillaLaserObj.SetActive(false);
        }

        if (!isLaserDuelActive) return;

        // 1. 가만히 있으면 바닐라가 플레이어(쇼콜라) 쪽으로 점점 밀고 들어옴 (진행도 감소)
        duelProgress -= banillaPushSpeed * Time.deltaTime;
        duelProgress = Mathf.Clamp(duelProgress, 0f, 1f);

        // 바뀐 진행도에 맞춰 실시간으로 충돌 지점 계산
        UpdateDuelLasers();

        // ★ 패배 조건: 시간이 지나서 게이지가 0이 되면 패배 처리
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
            // 1. 대결 상태가 아닐 때: 레이저 본체와 스파크(44, 54) 무조건 숨김
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
            // 2. ★ 대결 중일 때: 애니메이터의 위치 고정을 무시하고 매 프레임 레이저 충돌 지점으로 강제 멱살캐리!
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

        Vector3 playerFirePos = playerLaserScript != null && playerLaserScript.firePoint != null ? playerLaserScript.firePoint.position : playerRb.transform.position;
        Vector3 banillaFirePos = banillaLaserObj != null ? banillaLaserObj.transform.position : banillaTransform.position;

        duelMidwayPoint = Vector2.Lerp(playerFirePos, banillaFirePos, duelProgress);

        // 플레이어 레이저 실시간 갱신
        if (playerLaserScript != null)
        {
            playerLaserScript.EnterCompetitionMode(duelMidwayPoint, playerCompetitionLaserColor);
        }

        // 바닐라 레이저 실시간 갱신
        if (banillaLaserObj != null)
        {
            FireBanillaLaser(banillaFirePos, duelMidwayPoint);
        }

        // ★ [추가] 44x_0, 54x_0 이펙트들을 실시간 충돌 지점(가운데)으로 위치 이동
        if (banillaLaserEffects != null)
        {
            foreach (GameObject effect in banillaLaserEffects)
            {
                if (effect != null)
                {
                    // 2D 환경에서 기존 Z축(깊이) 값은 유지하면서 X, Y 좌표만 충돌 지점으로 따라가게 합니다.
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

            banillaLaserEffects.Clear(); // 리스트 초기화

            Transform[] allChildren = banillaObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == "Laser_Yellow_0")
                {
                    banillaLaserObj = child.gameObject;
                    banillaLaserObj.SetActive(false);
                }
                // ★ [추가] 44x_0, 54x_0 이름이 보이면 끄고 리스트에 보관
                else if (child.name == "44x_0" || child.name == "54x_0")
                {
                    child.gameObject.SetActive(false);
                    banillaLaserEffects.Add(child.gameObject);
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
                float zigzagY = 0f;
                if (i != 0)
                {
                    zigzagY = (i % 2 == 0) ? girlYSpread : -girlYSpread;
                }

                float yOffset = -1.5f;

                Vector3 spawnPos = new Vector3(banillaSpawnPosition.x + ((i + 1) * girlSpawnSpacing), pierreSpawnPosition.y + yOffset + zigzagY, 0);

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
            // foreach 대신 for문을 사용해 각 여학생의 번호(i)를 파악합니다.
            for (int i = 0; i < spawnedGirls.Count; i++)
            {
                GameObject girl = spawnedGirls[i];
                if (girl != null)
                {
                    // ★ 핵심: 피에르의 위치에서 학생 수와 인덱스를 계산해 자기만의 자리를 찾습니다.
                    // 이렇게 하면 서로 뚫고 지나가지 않고 원래 줄 서있던 순서대로 간격을 두고 멈춥니다.
                    float targetX = pierreSpawnPosition.x - distanceToPierre - ((spawnedGirls.Count - 1 - i) * girlStopSpacing);

                    Vector3 targetPos = new Vector3(targetX, girl.transform.position.y, 0);

                    girl.transform.position = Vector3.MoveTowards(
                        girl.transform.position,
                        targetPos,
                        girlWalkSpeed * Time.deltaTime
                    );

                    // 아직 도착하지 않은 학생이 있다면 루프를 계속 돕니다.
                    if (Mathf.Abs(girl.transform.position.x - targetPos.x) > 0.05f)
                    {
                        allArrived = false;
                    }
                    else
                    {
                        // ★ [추가] 자기 자리에 완벽히 도착한 학생은 제자리걸음을 하지 않게 애니메이션을 끕니다.
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

    // ★ 쇼콜라 승리 시 호출되어 바닐라의 미소 애니메이션을 켜는 핵심 기능
    // ★ 쇼콜라가 완전히 이겼을 때 바닐라를 미소 상태로 '강제 전환'하는 함수
    private void TriggerBanillaSmile()
    {
        if (banillaAnimator != null)
        {
            // 1. 기존에 세팅해두신 'toSmile' 트리거를 작동시킵니다.
            banillaAnimator.SetTrigger("toSmile");

            // 2. ★[강제 해결책] 만약 트리거 조건이 씹히더라도 무조건 미소를 짓도록 
            //    애니메이션 상태 이름을 직접 호출하여 강제로 틀어버립니다.
            //    (애니메이터 창에 있는 미소 애니메이션 블록 이름이 "Banilla_Smile"이 맞는지 꼭 확인하세요!)
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
        yield return new WaitForSeconds(4f);

        SetBanillaCrying(true);

        vcamPlayer.gameObject.SetActive(false);
        vcamBanilla.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.5f);

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

    // 클릭 시 레이저 대결 시작 함수
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

        // 대결 시작 시 레이저와 스파크 이펙트들을 켭니다.
        banillaLaserObj.SetActive(true);
        foreach (GameObject effect in banillaLaserEffects)
        {
            if (effect != null) effect.SetActive(true);
        }

        // 레이저 본체의 SpriteRenderer 컴포넌트를 가져옵니다.
        SpriteRenderer sr = banillaLaserObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // ★ [수정] sortingLayerName과 sortingOrder를 변경하던 코드를 완전히 삭제합니다.
            // 이제 유니티 에디터에서 설정한 BurnEffect / Order 0 설정을 그대로 사용합니다.
            sr.enabled = true;
        }

        // --- (아래쪽 레이저 각도/길이 조절 코드는 그대로 유지) ---
        Vector2 direction = targetPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        banillaLaserObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 이미지 길이에 따른 X 스케일 계산 (스프라이트 Pivot이 Left일 때 기준)
        float baseWidth = sr != null && sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
        float currentYScale = banillaLaserObj.transform.localScale.y;

        float parentScaleX = 1f;
        if (banillaLaserObj.transform.parent != null && banillaLaserObj.transform.parent.lossyScale.x != 0)
        {
            parentScaleX = Mathf.Abs(banillaLaserObj.transform.parent.lossyScale.x);
        }

        float finalXScale = (distance / baseWidth) / parentScaleX;

        // Z축은 그대로 유지
        float currentZScale = banillaLaserObj.transform.localScale.z;
        banillaLaserObj.transform.localScale = new Vector3(finalXScale, currentYScale, currentZScale);
    }

    public void StartBanillaCompetition() { SetBanillaCrying(false); }
    // 플레이어가 졌을 때
    public void OnPlayerLoseCompetition()
    {
        // 1. 바닐라가 다시 우는 애니메이션을 틀어줍니다.
        SetBanillaCrying(true);

        // 2. 대결 상태를 끄고 화면의 레이저를 싹 지웁니다.
        ExitLaserDuel();

        // 3. ★ 쇼콜라에게 바닐라의 위치(banillaTransform)를 넘겨주면서 넉백을 실행합니다!
        if (playerLaserScript != null && banillaTransform != null)
        {
            playerLaserScript.StartPlayerKnockback(banillaTransform);
            Debug.Log("쇼콜라 패배! 넉백 애니메이션 실행");
        }
    }

    // 플레이어가 완승하면 이 함수가 호출되어 미소 짓는 애니메이션이 나갑니다!
    public void OnPlayerWinCompetition()
    {
        // 승리 컷씬이 나오면 바닐라를 더이상 클릭할수없게 잠금 처리
        canStartDuel = false;

        // 1. 레이저 오브젝트들을 먼저 화면에서 즉시 지웁니다.
        ExitLaserDuel();

        // 2. 그 직후 바닐라에게 'toSmile' 트리거를 던져 미소 애니메이션을 실행합니다.
        TriggerBanillaSmile();

        // 3. ★ 대결이 성공적으로 끝났으니 쇼콜라의 이동 스크립트를 켜서 조작권을 돌려줍니다.
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true;

            // 넉백 복구 때 쓰셨던 백무빙(방향 오류) 방지 초기화 함수를 여기서도 쏴줍니다.
            try
            {
                playerMoveScript.gameObject.SendMessage("ForceWakeUpInputInit", SendMessageOptions.DontRequireReceiver);
            }
            catch { }
        }

        // 쇼콜라가 자연스럽게 대기 상태로 돌아가도록 파라미터를 정리해 줍니다.
        if (playerAnim != null)
        {
            playerAnim.SetBool("isAttacking", false);
            playerAnim.SetBool("isIdle", true);
        }

        StartCoroutine(BanillaFadeOutAndDropHeart());
    }

    // ★ 새롭게 추가되는 바닐라 정화 소멸 및 하얀 하트 드롭 코루틴
    private IEnumerator BanillaFadeOutAndDropHeart()
    {
        // 대결 중이던 바닐라 오브젝트 확보 (기존 스폰 인스턴스 또는 태그 검색)
        GameObject banillaObj = instanceBanilla;
        if (banillaObj == null) banillaObj = GameObject.FindWithTag("Banilla");
        if (banillaObj == null) yield break;

        // 잠시 미소 짓는 모습을 아주 잠깐(예: 0.5초) 보여준 뒤 페이드아웃 하려면 여기에 추가 가능합니다.
        yield return new WaitForSeconds(0.3f);

        // 1. 바닐라와 자식 오브젝트들의 모든 SpriteRenderer를 싹 긁어옵니다.
        SpriteRenderer[] renderers = banillaObj.GetComponentsInChildren<SpriteRenderer>();

        float fadeDuration = 1.5f; // 1.5초 동안 서서히 페이드 아웃
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            // 모든 스프라이트의 알파(투명도)값을 동시에 줄여나갑니다.
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

        // 2. 완전히 사라진 바닐라의 마지막 월드 좌표를 정확히 기록합니다.
        Vector3 dropPos = banillaObj.transform.position;

        // 3. 기존 PlayerLaser에 설정된 바닥 오프셋 값을 가져와 동일한 높이의 바닥 라인(floorY)을 계산합니다.
        float floorY = playerRb.transform.position.y - 1.5f; // 기본 방어용 최소값
        if (playerLaserScript != null)
        {
            floorY = playerRb.transform.position.y + playerLaserScript.dropYOffset;
        }

        // 4. 기존 남자 NPC들과 동일한 프리랩을 생성하고, 하트 스크립트를 추출해 초기화합니다.
        if (playerLaserScript != null && playerLaserScript.droppedHeartPrefab != null && banillaWhiteHeart != null)
        {
            // 하트 프리팹 생성
            GameObject droppedHeart = Instantiate(playerLaserScript.droppedHeartPrefab, dropPos, Quaternion.identity);

            // 하트 컴포넌트 획득
            DroppedHeart heartScript = droppedHeart.GetComponent<DroppedHeart>();
            if (heartScript == null) heartScript = droppedHeart.GetComponentInChildren<DroppedHeart>();

            if (heartScript != null)
            {
                heartScript.isBanillaWhiteHeart = true;
                // ★ 핵심: 매니저 창고에 등록해 둔 '하얀 하트(banillaWhiteHeart)' 스킨을 주입하여 툭 떨어뜨립니다!
                heartScript.Initialize(banillaWhiteHeart, dropPos, floorY);
            }
        }

        // 5. 연출이 완벽하게 끝났으므로 투명해진 바닐라 본체를 씬에서 완전히 삭제(소멸)합니다.
        Destroy(banillaObj);

        Debug.Log("바닐라 정화 완료: 페이드아웃 및 하얀 하트 드롭 성공!");
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

        // ★ [추가] 대결 종료 시 44x_0, 54x_0 끄기
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