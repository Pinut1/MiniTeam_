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
    [SerializeField] private float swapDuration = 0.8f; // 교체되는 데 걸리는 시간
    [SerializeField] private float hiddenPosY = -1000f; // 화면 아래로 숨겨질 Y 좌표
    [SerializeField] private float visiblePosY = 0f;    // 화면 중앙(원래 위치)의 Y 좌표

    [Header("대사 목록")]
    public DialogueData judangchiIntroData;

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

        // 0 . 플레이어 동작 비할성화
         MiniGameManager.Instance.DisablePlayerInput();

        // 1. 조그만 주당치 퇴장
        yield return StartCoroutine(MoveUIRoutine(Judangchi_initiate.rectTransform, false, stepDuration));

        // 확실한 위치 고정
        jRect.anchoredPosition = jTargetPos;
        
        //(선택 사항)
        yield return new WaitForSeconds(0.1f);

        // 2. 시네마틱 레터박스 등장 및 대기
        bool isLetterBoxDone = false;
        LetterBoxManager.Instance.ShowBars(() => isLetterBoxDone = true);

        // 콜백이 실행되어 true가 될때까지 여기서 코루틴을 멈추고 기다림
        yield return new WaitUntil(() => isLetterBoxDone);

        //(선택 사항)
        yield return new WaitForSeconds(0.1f);

        // 3. 큰 주당치 입장
        yield return StartCoroutine(MoveUIRoutine(Judangchi_default.rectTransform, true, stepDuration));

        yield return new WaitForSeconds(0.2f);

        // 4. 주댕치 대사 시작 및 다음 플로우 연결

        bool isDialogueDone = false;

        // 대사를 다 읽으면 isDialogueDone을 true로 바꿈
        JudangChiDialogueManager.Instance.StartDialogue(judangchiIntroData, () => isDialogueDone = true);
        
        // 대사를 다 읽고 isDialogueDone이 ture가 될때까지 대기
        yield return new WaitUntil(() => isDialogueDone);

        //TODO 대사 전부 끝난 뒤
        // 큰 주댕치 퇴장
        yield return StartCoroutine(MoveUIRoutine(Judangchi_default.rectTransform, false, stepDuration));

        //시네마틱 레터박스 치우기
        LetterBoxManager.Instance.HideBars();

        //플레이어 조작 다시 활성화
        MiniGameManager.Instance.EnablePlayerInput();

 
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

    // bool 변수로 주댕치 입장/퇴장을 제어하는 재사용 코루틴
    private IEnumerator MoveUIRoutine(RectTransform rect, bool isEnter, float duration)
    {
        Vector2 startPos = rect.anchoredPosition;

        // isEnter가 true면 visiblePosY, false면 hiddenPosY를 목표로 설정
        float targetY = isEnter ? visiblePosY : hiddenPosY;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            // Mathf.SmoothStep 안에 바로 나눗셈을 넣어서 변수 선언을 줄였습니다.
            float lerpT = Mathf.SmoothStep(0f, 1f, t / duration);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, lerpT);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
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
