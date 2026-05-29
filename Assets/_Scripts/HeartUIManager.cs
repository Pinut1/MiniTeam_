using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartUIManager : MonoBehaviour
{
    public static HeartUIManager instance;

    [Header("UI Settings")]
    public Image[] heartSlots;
    public float uncollectedAlpha = 0.3f;
    public Sprite[] possibleHeartSprites;

    [Header("보스 페이즈용 하트 스킨")]
    public Sprite rainbowHeartSprite;
    public Sprite vanillaWhiteHeartSprite;

    [Header("최종 마술봉 UI 아이콘")]
    public Sprite chocolatStickIcon;
    public Sprite vanillaStickIcon;

    [Header("보스 페이즈 UI 크기/위치 설정")]
    public float bossHeartScale = 1.8f;
    public float magicStickScale = 2.8f;
    public float centerSpacing = 70f;

    [Header("요술봉 확대 연출 설정")]
    public float zoomDuration = 1.5f;        // 확대+이동 시간
    public float finalZoomScale = 5f;        // 최종 확대 배율
    public float delayBetweenSticks = 0f;    // 두 봉 시간차 (0이면 동시)
    public float stickMergeSpacing = 40f;    // 중앙에서 두 봉 사이 간격

    private bool[] isCollected;
    private bool isBossPhaseMode = false;

    // ★ 원래 위치/크기 저장용 (연출 후 리셋에 사용)
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Vector3 boardCenterPos;

    [Header("종료 연출")]
    public GameObject finalMagicStickObject;
    public float fadeOutDuration = 1.5f; // 페이드 아웃 걸리는 시간 (길수록 천천히)
    public float postFadeDelay = 1.0f;

    void Awake()
    {
        if (instance == null) instance = this;
        if (heartSlots != null)
        {
            isCollected = new bool[heartSlots.Length];
            originalPositions = new Vector3[heartSlots.Length];
            originalScales = new Vector3[heartSlots.Length];

            for (int i = 0; i < heartSlots.Length; i++)
            {
                if (heartSlots[i] != null)
                {
                    SetAlpha(heartSlots[i], uncollectedAlpha);
                    isCollected[i] = false;
                    originalPositions[i] = heartSlots[i].transform.localPosition;
                    originalScales[i] = heartSlots[i].transform.localScale;
                }
            }
        }
    }

    public bool CollectHeart(Sprite collectedSprite)
    {
        if (collectedSprite == null) return false;

        if (isBossPhaseMode)
        {
            if (rainbowHeartSprite != null && collectedSprite.name == rainbowHeartSprite.name)
            {
                SetAlpha(heartSlots[0], 1f);
                isCollected[0] = true;
                return true;
            }
            else if (collectedSprite.name.Contains("White") || (vanillaWhiteHeartSprite != null && collectedSprite.name == vanillaWhiteHeartSprite.name))
            {
                heartSlots[1].sprite = collectedSprite;
                SetAlpha(heartSlots[1], 1f);
                isCollected[1] = true;
                return true;
            }
            return false;
        }

        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] != null && heartSlots[i].sprite != null)
            {
                if (!isCollected[i] && heartSlots[i].sprite.name == collectedSprite.name)
                {
                    isCollected[i] = true;
                    SetAlpha(heartSlots[i], 1f);
                    CheckAllHeartsCollected();
                    return true;
                }
            }
        }
        return false;
    }

    private void CheckAllHeartsCollected()
    {
        for (int i = 0; i < isCollected.Length; i++)
        {
            if (!isCollected[i]) return;
        }
        SetupBossHeartsUI();

        PlayerLaser laser = FindAnyObjectByType<PlayerLaser>();
        if (laser != null) laser.TriggerAllNpcsExit();
    }

    private void SetupBossHeartsUI()
    {
        isBossPhaseMode = true;

        if (heartSlots.Length > 0 && heartSlots[0] != null)
        {
            LayoutGroup layout = heartSlots[0].transform.parent.GetComponent<LayoutGroup>();
            if (layout != null) layout.enabled = false;
        }

        // 보드판 정중앙 좌표 계산 및 저장
        boardCenterPos = Vector3.zero;
        int activeCount = 0;
        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] != null)
            {
                boardCenterPos += heartSlots[i].transform.localPosition;
                activeCount++;
            }
        }
        if (activeCount > 0) boardCenterPos /= activeCount;

        for (int i = 0; i < heartSlots.Length; i++)
        {
            isCollected[i] = false;
            if (heartSlots[i] != null)
            {
                heartSlots[i].sprite = null;
                SetAlpha(heartSlots[i], 0f);
            }
        }

        if (heartSlots.Length > 0 && heartSlots[0] != null && rainbowHeartSprite != null)
        {
            heartSlots[0].sprite = rainbowHeartSprite;
            SetAlpha(heartSlots[0], uncollectedAlpha);
            heartSlots[0].transform.localPosition = boardCenterPos + new Vector3(-centerSpacing, 0f, 0f);
            heartSlots[0].transform.localScale = new Vector3(bossHeartScale, bossHeartScale, 1f);
        }

        if (heartSlots.Length > 1 && heartSlots[1] != null && vanillaWhiteHeartSprite != null)
        {
            heartSlots[1].sprite = vanillaWhiteHeartSprite;
            SetAlpha(heartSlots[1], uncollectedAlpha);
            heartSlots[1].transform.localPosition = boardCenterPos + new Vector3(centerSpacing, 0f, 0f);
            heartSlots[1].transform.localScale = new Vector3(bossHeartScale, bossHeartScale, 1f);
        }
    }

    public void ChangeHeartToStickUI(bool isChocolat)
    {
        if (isChocolat)
        {
            if (heartSlots[0] != null && chocolatStickIcon != null)
            {
                heartSlots[0].sprite = chocolatStickIcon;
                heartSlots[0].transform.localScale = new Vector3(magicStickScale, magicStickScale, 1f);
            }
        }
        else
        {
            if (heartSlots[1] != null && vanillaStickIcon != null)
            {
                heartSlots[1].sprite = vanillaStickIcon;
                heartSlots[1].transform.localScale = new Vector3(magicStickScale, magicStickScale, 1f);
            }
        }
    }

    // ★★★ [추가] 두 요술봉이 화면 중앙으로 확대되며 이동하는 연출 ★★★
    public void PlaySticksZoomToCenter(System.Action onComplete = null)
    {
        StartCoroutine(ZoomSticksToCenterCoroutine(onComplete));
    }

    private IEnumerator ZoomSticksToCenterCoroutine(System.Action onComplete)
    {
        if (heartSlots.Length < 2 || heartSlots[0] == null || heartSlots[1] == null)
        {
            onComplete?.Invoke();
            yield return null;
        }

        RectTransform pinkRect = heartSlots[0].GetComponent<RectTransform>();
        RectTransform blueRect = heartSlots[1].GetComponent<RectTransform>();

        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.PlayMagicStickGrowingBGM();
        }

        // 1. 위치 계산 (중앙으로 모으기 위한 타겟 설정)
        Vector2 pos0 = pinkRect.anchoredPosition;
        Vector2 pos1 = blueRect.anchoredPosition;
        Vector2 spacingVector = pos1 - pos0;
        Vector2 pinkTargetPos = -spacingVector / 2f;
        Vector2 blueTargetPos = spacingVector / 2f;

        // 2. 이동 시작 (이 부분을 다시 넣어줘야 움직입니다!)
        Coroutine pinkRoutine = StartCoroutine(ZoomSingleStick(pinkRect, pinkTargetPos, finalZoomScale, zoomDuration));

        if (delayBetweenSticks > 0)
            yield return new WaitForSeconds(delayBetweenSticks);

        Coroutine blueRoutine = StartCoroutine(ZoomSingleStick(blueRect, blueTargetPos, finalZoomScale, zoomDuration));

        // 3. 이동이 완전히 끝날 때까지 대기
        yield return new WaitForSeconds(zoomDuration);

        // 4. 이제 페이드 아웃 실행
        yield return StartCoroutine(FadeOutHearts(fadeOutDuration));

        // 5. 뜸들이기
        yield return new WaitForSeconds(postFadeDelay);

        // 6. MagicStick 활성화
        if (finalMagicStickObject != null)
        {
            if (BgmManager.Instance != null)
            {
                BgmManager.Instance.PlayMagicStickTransformBGM();
            }
            finalMagicStickObject.SetActive(true);
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeOutHearts(float duration)
    {
        float elapsed = 0f;
        Color[] startColors = new Color[2];

        // 현재 알파값 저장
        for (int i = 0; i < 2; i++)
            if (heartSlots[i] != null) startColors[i] = heartSlots[i].color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            for (int i = 0; i < 2; i++)
            {
                if (heartSlots[i] != null)
                {
                    Color c = startColors[i];
                    c.a = alpha;
                    heartSlots[i].color = c;
                }
            }
            yield return null;
        }
    }

    private IEnumerator ZoomSingleStick(RectTransform stick, Vector2 targetPos, float targetScaleValue, float duration)
    {
        Vector2 startPos = stick.anchoredPosition;
        Vector3 startScale = stick.localScale;
        Vector3 endScale = new Vector3(targetScaleValue, targetScaleValue, 1f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // EaseOutQuad: 처음 빠르고 끝에서 부드럽게 감속
            float easeT = 1f - (1f - t) * (1f - t);

            stick.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            stick.localScale = Vector3.Lerp(startScale, endScale, easeT);

            yield return null;
        }

        // 최종값 확정
        stick.anchoredPosition = targetPos;
        stick.localScale = endScale;
    }

    // ★ 연출 후 원래 상태로 리셋 (필요 시 호출)
    public void ResetSticksToOriginal()
    {
        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (heartSlots[i] != null)
            {
                heartSlots[i].transform.localPosition = originalPositions[i];
                heartSlots[i].transform.localScale = originalScales[i];
            }
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
