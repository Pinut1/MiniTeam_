using MiniTeam.Core;
using System.Collections;
using UnityEngine;

public class DroppedHeart : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool isCollected = false;

    // 레이더 범위 (Update 스캔 방식 유지)
    public float pickupRadius = 0.8f;

    public bool isPierreHeart = false;

    [Header("바닐라 정화 하트 설정")]
    public bool isBanillaWhiteHeart = false; // ★ 추가됨: 이 하트가 바닐라가 떨어뜨린 하얀 하트인지 여부

    [Header("피에르 이펙트 설정")]
    public GameObject pierreEffectPrefab;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Sprite heartSprite, Vector3 spawnPos, float targetHorizontalLineY)
    {
        if (sr != null) sr.sprite = heartSprite;
        transform.position = spawnPos;

        // 포동! 튀었다가 "전달받은 바닥선"으로 툭 떨어지는 애니메이션 시작
        StartCoroutine(DropAnimation(targetHorizontalLineY));
    }

    IEnumerator DropAnimation(float targetY)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        float time = 0;
        float duration = 0.4f;

        float distance = Mathf.Abs(startPos.y - targetY);
        float curveHeight = distance * 0.3f;

        // 1. 공중에서 바닥으로 포물선 그리며 떨어지기
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            float height = Mathf.Sin(progress * Mathf.PI) * curveHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += height;

            transform.position = currentPos;
            yield return null;
        }

        // 2. 바닥에 닿았을 때 통 튕기는 맛 살리기
        time = 0;
        float bounceDuration = 0.1f;
        Vector3 bounceTarget = targetPos + new Vector3(0, 0.15f, 0);

        while (time < bounceDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(targetPos, bounceTarget, time / bounceDuration);
            yield return null;
        }

        time = 0;
        while (time < bounceDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(bounceTarget, targetPos, time / bounceDuration);
            yield return null;
        }

        transform.position = targetPos;
    }

    // --- [Update 레이더 스캔 방식 유지] ---
    void Update()
    {
        if (isCollected) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                isCollected = true;
                Debug.Log($"하트 획득 완료! 이 하트가 피에르 하트인가요? : {isPierreHeart} / 하얀 하트인가요? : {isBanillaWhiteHeart}");

                // --- 1. 피에르 하트 처리 로직 ---
                if (isPierreHeart)
                {
                    if (pierreEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(pierreEffectPrefab, transform.position, Quaternion.identity);
                        SpriteRenderer[] allRenderers = effect.GetComponentsInChildren<SpriteRenderer>(true);
                        foreach (SpriteRenderer renderer in allRenderers)
                        {
                            renderer.sortingLayerName = "Magic";
                            renderer.sortingOrder = 10;
                        }
                        Destroy(effect, 2.0f);
                    }

                    PlayerLaser laser = FindAnyObjectByType<PlayerLaser>();
                    if (laser != null)
                    {
                        Debug.Log("PlayerLaser를 찾아서 TriggerPierreEnding을 호출합니다!");
                        laser.TriggerPierreEnding();
                    }
                }

                // --- 2. ★ 바닐라 하얀 하트 (게임 클리어) 처리 로직 ---
                if (isBanillaWhiteHeart)
                {
                    Debug.Log("게임 클리어");

                    if (MiniGameManager.Instance != null)
                    {
                        MiniGameManager.Instance.OnMiniGameClear();
                    }
                    else
                    {
                        Debug.LogWarning("MiniGameManager.Instance를 찾을 수 없습니다. (디버깅용 로그: MiniGameManager.Instance.OnMiniGameClear();)");
                    }

                    // ★ [수정 완료] 플레이어(쇼콜라) 강제 정지 - 씬 전체에서 메인 컴포넌트를 직접 찾아 확실하게 멈춥니다.
                    PlayerMove playerMove = FindAnyObjectByType<PlayerMove>();
                    if (playerMove != null)
                    {
                        playerMove.enabled = false;

                        // 기존에 쓰시던 입력 초기화(미끄러짐 방지) 함수 강제 호출
                        try { playerMove.gameObject.SendMessage("ForceWakeUpInputInit", SendMessageOptions.DontRequireReceiver); } catch { }

                        // 물리 이동 강제 정지 (simulated를 끄지 않아서 공중에 멈추는 버그 방지)
                        Rigidbody2D playerRb = playerMove.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            playerRb.linearVelocity = Vector2.zero;
                        }

                        // 애니메이션을 강제로 Idle 상태로 고정
                        Animator playerAnim = playerMove.GetComponentInChildren<Animator>();
                        if (playerAnim == null) playerAnim = playerMove.GetComponent<Animator>();
                        if (playerAnim != null)
                        {
                            playerAnim.SetBool("isWalk", false);
                            playerAnim.SetBool("isRun", false);
                            playerAnim.SetBool("isAttacking", false);
                            playerAnim.SetBool("isIdle", true);
                            playerAnim.Play("Idle");
                        }
                    }

                    // 레이저 스크립트 끄기 (공격 차단)
                    PlayerLaser playerLaserScript = FindAnyObjectByType<PlayerLaser>();
                    if (playerLaserScript != null)
                    {
                        playerLaserScript.enabled = false;
                    }
                }

                // --- 3. 공통 처리 (UI 추가 및 삭제) ---
                if (HeartUIManager.instance != null)
                {
                    HeartUIManager.instance.CollectHeart(sr.sprite);
                }

                Destroy(gameObject);
                break; // 루프 탈출
            }
        }
    }
}