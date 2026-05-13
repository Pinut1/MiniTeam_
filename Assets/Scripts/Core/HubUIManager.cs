using MiniTeam.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HubUIManager : MonoBehaviour
{
    public EyeOpeningEffect eyeEffect;
    public float openSpeed = 1.5f;
   public static HubUIManager Instance { get; private set; }

    [Header("인게임 UI Panels")]
    [SerializeField] private GameObject warningUI;

    [SerializeField] private Image Judangchi_initiate;
    [SerializeField] private Image Judangchi_default;

    [Header("Judangchi Animation Settings")]
    [SerializeField] private float swapDuration = 0.5f; // 교체되는 데 걸리는 시간
    [SerializeField] private float hiddenPosY = -1000f; // 화면 아래로 숨겨질 Y 좌표
    [SerializeField] private float visiblePosY = 0f;    // 화면 중앙(원래 위치)의 Y 좌표
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        Button judangchiBtn = Judangchi_initiate.GetComponent<Button>();
        if (judangchiBtn != null)
        {
            judangchiBtn.onClick.AddListener(OnJudangchiClicked);
        }
        else
        {
            Debug.LogWarning("Judangchi 이미지에 button 컴포넌트가 없습니다. 클릭 이벤트 구독 실패.");
        }
    }

    private void OnJudangchiClicked()
    {
        Judangchi_initiate.GetComponent<Button>().interactable = false;
        StartCoroutine(SwapJudangchiRoutine());
    }

    private IEnumerator SwapJudangchiRoutine()
    {
        RectTransform jRect = Judangchi_initiate.rectTransform;
        RectTransform jdRect = Judangchi_default.rectTransform;

        Vector2 jStartPos = jRect.anchoredPosition;
        Vector2 jTargetPos = new Vector2(jStartPos.x, hiddenPosY);

        Vector2 jdStartPos = jdRect.anchoredPosition;
        Vector2 jdTargetPos = new Vector2(jdStartPos.x, visiblePosY);

        // 애니메이션 시간을 절반으로 나누어 가각 적용
        float stepDuration = swapDuration / 2f;

        // 1. 조그만 주당치 퇴장
        float t = 0f;
        while (t < stepDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / stepDuration;

            float lerpT = Mathf.SmoothStep(0f, 1f, normalizedTime);

            jRect.anchoredPosition = Vector2.Lerp(jStartPos, jTargetPos, lerpT);
            yield return null;
        }

        // 확실한 위치 고정
        jRect.anchoredPosition = jTargetPos;
        
        //(선택 사항)
        yield return new WaitForSeconds(0.1f);

        LetterBoxManager.Instance.ShowBars();

        // 2. 큰 주당치 입장
        t = 0f;
        while (t < stepDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.SmoothStep(0f, 1f, t / stepDuration);
            jdRect.anchoredPosition = Vector2.Lerp(jdStartPos, jdTargetPos, lerpT);
            yield return null;
        }

        jdRect.anchoredPosition = jdTargetPos;
        MiniGameManager.Instance.DisablePlayerInput();
    }

    // 경고 UI 제어
    public void ToggleWarningUI(bool isActive)
    {
        if (warningUI != null)
        {
            warningUI.SetActive(isActive);
        }
    }
    
    public void WakeUp(Action onComplete = null)
    {
        if (eyeEffect != null)
        {
            // 1. 잠들어 있는 효과를 깨웁니다. 
            // 이게 없으면 OnRenderImage가 호출되지 않아 화면이 계속 암전되거나 변화가 없습니다.
            eyeEffect.enabled = true;

            // 코루틴에서 넘겨받은 onComplete를 전달
            StartCoroutine(WakeUpRoutine(onComplete));
        }
        else
        {
            // 방어 코드 : 이펙트가 없으면 바로 처리
            onComplete?.Invoke();
        }
    }

    private IEnumerator WakeUpRoutine(Action onComplete)
    {
        // 초기화
        eyeEffect.openAmount = 0.001f;
        eyeEffect.expand = 0.0f;

        float t = 0;

        // [1단계] 눈 뜨기 (가느다란 선 -> 타원형)
        // openAmount를 0.001에서 1.0까지 올립니다.
        while (t < 0.8f)
        {
            t += Time.deltaTime * openSpeed;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t > 0.001f)
        {
            t -= Time.deltaTime * openSpeed;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t< 1f)
        {
            t += Time.deltaTime * openSpeed * 2;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        // 잠시 초점을 맞추는 듯한 0.2초의 멈춤 (몰입감 up)
        yield return new WaitForSeconds(0.1f);

        // [2단계] 시야 확장 (타원형 -> 전체 화면)
        // expand를 0.0에서 1.5까지 올려서 화면 구석까지 밝힙니다.
        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (openSpeed * 1.5f); // 확장은 좀 더 빠르게
            eyeEffect.expand = Mathf.Lerp(0.0f, 1.5f, t);
            yield return null;
        }

        // 연출이 끝난 후 효과 스크립트 자체를 꺼서 성능 최적화
        eyeEffect.enabled = false;

        // 연출이 완전히 끝난 후, 넘겨받은 onComplete를 실행
        onComplete?.Invoke();
    }

 
}
