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

    [Header("플레이어 애니메이션 (직접 연결)")]
    public Animator playerAnim;

    [Header("여학생 이동 설정")]
    public float girlWalkSpeed = 3f;

    private List<GameObject> spawnedGirls = new List<GameObject>();

    private GameObject instanceVanilla;
    private Animator banillaAnimator;

    public void SpawnAndPlayCutscene()
    {
        spawnedGirls.Clear();
        banillaAnimator = null;

        if (pierrePrefab != null) Instantiate(pierrePrefab, pierreSpawnPosition, Quaternion.identity);

        if (banillaPrefab != null)
        {
            instanceVanilla = Instantiate(banillaPrefab, banillaSpawnPosition, Quaternion.identity);

            banillaAnimator = instanceVanilla.GetComponent<Animator>();
            if (banillaAnimator == null)
            {
                banillaAnimator = instanceVanilla.GetComponentInChildren<Animator>();
            }

            SetBanillaCrying(false);
        }

        if (newGirlPrefabs != null)
        {
            for (int i = 0; i < newGirlPrefabs.Length; i++)
            {
                Vector3 spawnPos = banillaSpawnPosition + new Vector3((i + 1) * girlSpawnSpacing, 0, 0);
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

        // 오프닝 컷신: 왼쪽 바라보기
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
        catch (System.Exception e)
        {
            Debug.LogError("플레이어 방향 동기화 실패: " + e.Message);
        }
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
            // 혹시 씬에 미리 배치된 바닐라가 있다면 태그로 찾아봅니다.
            GameObject banillaObj = GameObject.FindWithTag("Banilla");
            if (banillaObj != null)
            {
                banillaAnimator = banillaObj.GetComponent<Animator>();
                if (banillaAnimator != null) banillaAnimator.SetBool("isCry", isCrying);
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
    // ★ 피에르 하트 획득 시 발동하는 컷신 로직 ★
    // =========================================================================

    public void OnPlayerGetPierreHeart()
    {
        Debug.Log("피에르 하트 획득! 엔딩 컷신 코루틴 시작.");

        // 1. 바닐라 울기 시작
        SetBanillaCrying(true);

        // 2. 엔딩 컷신 연출 시작
        StartCoroutine(PierreHeartCutsceneSequence());
    }

    private IEnumerator PierreHeartCutsceneSequence()
    {
        // 1. 플레이어 조작 잠금 및 정지
        playerMoveScript.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        if (playerAnim != null)
        {
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isIdle", true);
            playerAnim.Play("Idle");
        }

        // 2. 플레이어가 왼쪽(-)을 보도록 강제 전환 (문워크 방지 포함)
        Vector3 playerScale = playerRb.transform.localScale;
        playerScale.x = -Mathf.Abs(playerScale.x);
        playerRb.transform.localScale = playerScale;
        SyncPlayerMoveDirection(false);

        // 시네마머신 켜기 및 다른 카메라 초기화
        if (mainBrain != null) mainBrain.enabled = true;
        vcamBanilla.gameObject.SetActive(false);
        vcamPierre.gameObject.SetActive(false);
        vcamGirlsWalk.gameObject.SetActive(false);

        // 3. 플레이어 (왼쪽 바라보는 모습) 2초 비추기
        vcamPlayer.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        // 4. 우는 바닐라 3초 비추기
        vcamPlayer.gameObject.SetActive(false);
        vcamBanilla.gameObject.SetActive(true);
        yield return new WaitForSeconds(6f);

        // 5. 다시 플레이어 1.5초 비추기
        vcamBanilla.gameObject.SetActive(false);
        vcamPlayer.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);

        // 6. 컷신 종료 후 플레이어 조작 복구
        if (playerAnim != null)
        {
            playerAnim.SetBool("isIdle", false);
        }

        SyncPlayerMoveDirection(false);

        if (mainBrain != null) mainBrain.enabled = false;
        playerMoveScript.enabled = true;

        playerMoveScript.gameObject.SendMessage("ForceWakeUpInputInit", SendMessageOptions.DontRequireReceiver);

        Debug.Log("엔딩 컷신 종료, 플레이어 조작 복구 완료!");
    }

    public void StartBanillaCompetition()
    {
        SetBanillaCrying(false);
    }

    public void OnPlayerLoseCompetition()
    {
        SetBanillaCrying(true);
    }

    public void OnPlayerWinCompetition()
    {
        TriggerBanillaSmile();
    }
}