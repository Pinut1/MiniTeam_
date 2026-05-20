using UnityEngine;
using System.Collections;

public class DroppedHeart : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool isCollected = false;

    // 레이더 범위 (Update 스캔 방식 유지)
    public float pickupRadius = 0.8f;

    public bool isPierreHeart = false;

    [Header("피에르 이펙트 설정")]
    public GameObject pierreEffectPrefab;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ★★★ [수정됨] 매개변수에 float targetHorizontalLineY 를 추가했습니다! ★★★
    public void Initialize(Sprite heartSprite, Vector3 spawnPos, float targetHorizontalLineY)
    {
        if (sr != null) sr.sprite = heartSprite;
        transform.position = spawnPos; // 남학생 가슴 위치에서 시작

        // 포동! 튀었다가 "전달받은 바닥선"으로 툭 떨어지는 애니메이션 시작
        StartCoroutine(DropAnimation(targetHorizontalLineY));
    }

    IEnumerator DropAnimation(float targetY)
    {
        Vector3 startPos = transform.position; // 남학생 위치

        // ★ [핵심] 최종 목표 좌표 설정: X는 그대로, Y는 플레이어 발밑선!
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        float time = 0;
        float duration = 0.4f; // 떨어지는 시간

        // 남학생과 바닥의 거리 계산 (포물선 높이 조절용)
        float distance = Mathf.Abs(startPos.y - targetY);
        float curveHeight = distance * 0.3f; // 거리에 비례해서 살짝 위로 튀어오르게 함

        // 1. 공중에서 바닥으로 포물선 그리며 떨어지기
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            // 예쁜 포물선 높이 계산
            float height = Mathf.Sin(progress * Mathf.PI) * curveHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += height; // 포물선 추가

            transform.position = currentPos;
            yield return null;
        }

        // 2. 바닥에 닿았을 때 "통~" 하고 한 번 튕기는 맛 살리기 (툭! 느낌 극대화)
        time = 0;
        float bounceDuration = 0.1f;
        Vector3 bounceTarget = targetPos + new Vector3(0, 0.15f, 0); // 위로 살짝 튕길 높이

        // 위로 튕기기
        while (time < bounceDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(targetPos, bounceTarget, time / bounceDuration);
            yield return null;
        }
        // 다시 바닥으로 안착
        time = 0;
        while (time < bounceDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(bounceTarget, targetPos, time / bounceDuration);
            yield return null;
        }

        // 최종 바닥 위치 고정
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
                Debug.Log($"하트 획득 완료! 이 하트가 피에르 하트인가요? : {isPierreHeart}");

                if (isPierreHeart)
                {
                    if (pierreEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(pierreEffectPrefab, transform.position, Quaternion.identity);
                        SpriteRenderer[] allRenderers = effect.GetComponentsInChildren<SpriteRenderer>(true);
                        foreach (SpriteRenderer renderer in allRenderers)
                        {
                            renderer.sortingLayerName = "Magic"; // 레이어를 Magic으로 고정
                            renderer.sortingOrder = 10;          // 숫자를 확 높여서 무조건 맨 앞에 오게 강제 설정
                        }
                        Destroy(effect, 2.0f); // 1.5초 후 이펙트 자동 삭제 (애니메이션 길이에 맞춰 조절하세요)
                    }

                    PlayerLaser laser = FindAnyObjectByType<PlayerLaser>();
                    if (laser != null)
                    {
                        Debug.Log("PlayerLaser를 찾아서 TriggerPierreEnding을 호출합니다!");
                        laser.TriggerPierreEnding();
                    }
                }

                if (HeartUIManager.instance != null)
                {
                    HeartUIManager.instance.CollectHeart(sr.sprite);
                }

                Destroy(gameObject);
                break;
            }
        }
    }
}