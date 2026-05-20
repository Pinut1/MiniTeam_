using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneNpcManager : MonoBehaviour
{
    [Header("스폰할 NPC 프리팹")]
    public GameObject pierrePrefab;
    public GameObject vanillaPrefab;
    public GameObject[] newGirlPrefabs;
    public float girlSpawnSpacing = 1.5f;

    [Header("스폰 좌표")]
    public Vector3 pierreSpawnPosition;
    public Vector3 vanillaSpawnPosition;

    [Header("시네마머신 카메라 설정")]
    public CinemachineBrain mainBrain;
    public CinemachineCamera vcamPierre;
    public CinemachineCamera vcamVanilla;
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
    private Animator vanillaAnimator;

    public void SpawnAndPlayCutscene()
    {
        spawnedGirls.Clear();
        vanillaAnimator = null;

        if (pierrePrefab != null) Instantiate(pierrePrefab, pierreSpawnPosition, Quaternion.identity);

        if (vanillaPrefab != null)
        {
            instanceVanilla = Instantiate(vanillaPrefab, vanillaSpawnPosition, Quaternion.identity);

            vanillaAnimator = instanceVanilla.GetComponent<Animator>();
            if (vanillaAnimator == null)
            {
                vanillaAnimator = instanceVanilla.GetComponentInChildren<Animator>();
            }

            // 스폰 시점에는 무조건 Idle 상태로 대기
            SetVanillaCrying(false);
        }

        if (newGirlPrefabs != null)
        {
            for (int i = 0; i < newGirlPrefabs.Length; i++)
            {
                Vector3 spawnPos = vanillaSpawnPosition + new Vector3((i + 1) * girlSpawnSpacing, 0, 0);
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
        vcamVanilla.gameObject.SetActive(false);
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

        // 1. 컷신 시작 시 플레이어를 강제로 왼쪽(-) 보게 만듭니다.
        Vector3 playerScale = playerRb.transform.localScale;
        playerScale.x = -Mathf.Abs(playerScale.x);
        playerRb.transform.localScale = playerScale;

        // ★ [핵심 추가] 이동 스크립트(PlayerMove)의 내부 방향 기억 변수도 '왼쪽(false)'으로 강제 동기화합니다.
        // 이 코드가 들어가야 조작이 켜졌을 때 문워크를 안 합니다!
        SyncPlayerMoveDirection(false);

        vcamPlayer.gameObject.SetActive(true);
        if (mainBrain != null) mainBrain.enabled = true;

        yield return new WaitForSeconds(1.5f);

        vcamPierre.gameObject.SetActive(true);
        vcamPlayer.gameObject.SetActive(false);

        yield return new WaitForSeconds(4.5f);

        vcamVanilla.gameObject.SetActive(true);
        vcamPierre.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.0f);

        if (spawnedGirls.Count > 0 && spawnedGirls[0] != null)
        {
            vcamGirlsWalk.Follow = spawnedGirls[0].transform;
        }

        vcamGirlsWalk.gameObject.SetActive(true);
        vcamVanilla.gameObject.SetActive(false);

        yield return StartCoroutine(GirlsWalkToPierre());

        vcamPlayer.gameObject.SetActive(true);
        vcamGirlsWalk.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.0f);

        // 유저님 의도대로 방향 전환 코드를 삭제하여 컷신 종료 후에도 왼쪽을 바라봅니다.
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

    // =========================================================================
    // ★ [새로 추가] 플레이어 이동 스크립트의 내부 방향 변수를 강제로 맞춰주는 함수
    // =========================================================================
    private void SyncPlayerMoveDirection(bool faceRight)
    {
        if (playerMoveScript == null) return;
        try
        {
            System.Type type = playerMoveScript.GetType();
            // PlayerMove 스크립트 내부의 모든 변수들을 싹 훑습니다.
            System.Reflection.FieldInfo[] fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            foreach (var field in fields)
            {
                // 변수 타입이 bool 이면서 이름에 right, facing, face 등이 들어가면 전부 강제 세팅해 버립니다.
                if (field.FieldType == typeof(bool))
                {
                    string name = field.Name.ToLower();
                    if (name.Contains("right") || name.Contains("facing") || name.Contains("face"))
                    {
                        field.SetValue(playerMoveScript, faceRight);
                        Debug.Log($"[방향 동기화 완] {field.Name} 변수를 {faceRight}로 강제 변경함.");
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

    private void SetVanillaCrying(bool isCrying)
    {
        if (vanillaAnimator != null)
        {
            vanillaAnimator.SetBool("isCry", isCrying);
        }
    }

    private void TriggerVanillaSmile()
    {
        if (vanillaAnimator != null)
        {
            SetVanillaCrying(false);
            vanillaAnimator.SetTrigger("toSmile");
        }
    }

    // =========================================================================
    // ★ 외부 매니저/판정 스크립트에서 호출할 깔끔하게 정리된 함수들 ★
    // =========================================================================

    public void OnPlayerGetPierreHeart()
    {
        // ★ [수정됨] 하트 획득 시 정말 이 함수가 실행되는지, 바닐라가 있는지 추적합니다!
        Debug.Log("1. 하트 획득 신호가 매니저에 도착했습니다!");

        if (vanillaAnimator != null)
        {
            SetVanillaCrying(true); // 하트를 뺏기고 Crying으로 전환!
            Debug.Log("2. 바닐라에게 울기(isCry) 명령을 내렸습니다!");
        }
        else
        {
            Debug.LogError("에러: 매니저가 생성된 바닐라(vanillaAnimator)를 찾지 못했습니다! 매니저 스크립트가 씬에 2개 이상 중복되지 않았는지 확인하세요.");
        }
    }

    public void StartVanillaCompetition()
    {
        SetVanillaCrying(false);
    }

    public void OnPlayerLoseCompetition()
    {
        SetVanillaCrying(true);
    }

    public void OnPlayerWinCompetition()
    {
        TriggerVanillaSmile();
    }
}