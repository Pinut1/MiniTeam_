using MiniTeam.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HubUIManager : MonoBehaviour
{
    public static HubUIManager Instance { get; private set; }

    [Header("오프닝 연출 (WakeUp)")]
    public EyeOpeningEffect eyeEffect;
    public float openSpeed = 1.5f;

    [Header("인게임 UI Panels")]
    [SerializeField] private GameObject warningUI;

    [Header("UI Animators (애니메이터 제어용)")]
    [Tooltip("각 UI 객체에 달린 Animator를 연결해주세요")]
    [SerializeField] private Animator judangchiSmallAnim;
    [SerializeField] private Animator digiviceAnim;
    [SerializeField] private Animator judangchiBigAnim;

    [Header("대화창용 큰 주댕치 (표정 변화용)")]
    [SerializeField] private Image judangchiBigImage;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f; // 애니메이션 재생 대기 시간

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 작은 주댕치와 디지바이스의 버튼 컴포넌트에 클릭 이벤트를 연결합니다.
        if (judangchiSmallAnim != null)
            judangchiSmallAnim.GetComponent<Button>().onClick.AddListener(OnBottomUIClicked);

        if (digiviceAnim != null)
            digiviceAnim.GetComponent<Button>().onClick.AddListener(OnBottomUIClicked);

        // 초기 상태 세팅: 작은 주댕치는 켜고, 디지바이스는 끕니다.
        judangchiSmallAnim.gameObject.SetActive(true);
        if (digiviceAnim != null) digiviceAnim.gameObject.SetActive(false);
    }

    // 하단 UI(주댕치 or 디지바이스)가 클릭되었을 때
    private void OnBottomUIClicked()
    {
        // 클릭 중복 방지를 위해 버튼 기능을 잠시 끕니다. (Controller에서 다시 켜줍니다)
        judangchiSmallAnim.GetComponent<Button>().interactable = false;
        if (digiviceAnim != null) digiviceAnim.GetComponent<Button>().interactable = false;

        // 모든 연출의 지휘권은 Controller에게 넘깁니다!
        JudangChiController.Instance.PlaySequenceForCurrentStage();
    }

    // ==========================================
    // 공통 애니메이션 제어 코루틴
    // ==========================================
    private IEnumerator PlayUIAnimation(Animator targetAnim, string triggerName)
    {
        if (targetAnim == null) yield break;

        targetAnim.gameObject.SetActive(true);
        targetAnim.SetTrigger(triggerName);

        // 애니메이션이 재생되는 시간만큼 대기
        yield return new WaitForSeconds(animationDuration);

        // 퇴장 연출이 끝났다면 오브젝트를 비활성화하여 깔끔하게 정리
        if (triggerName == "Hide")
        {
            targetAnim.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // Controller가 호출할 명시적인 UI 제어 메서드들
    // ==========================================

    // 현재 스테이지에 맞춰 알맞은 하단 UI를 등장시킵니다.
    public IEnumerator ShowBottomUI(int currentStage)
    {
        Animator targetAnim = (currentStage >= 1) ? digiviceAnim : judangchiSmallAnim;
        targetAnim.GetComponent<Button>().interactable = true; // 버튼 다시 활성화
        yield return StartCoroutine(PlayUIAnimation(targetAnim, "Show"));
    }

    // 현재 떠있는 하단 UI를 퇴장시킵니다.
    public IEnumerator HideBottomUI(int currentStage)
    {
        Animator targetAnim = (currentStage >= 1) ? digiviceAnim : judangchiSmallAnim;
        yield return StartCoroutine(PlayUIAnimation(targetAnim, "Hide"));
    }

    public IEnumerator ShowBigJudangchi() => PlayUIAnimation(judangchiBigAnim, "Show");
    public IEnumerator HideBigJudangchi() => PlayUIAnimation(judangchiBigAnim, "Hide");

    public void ChangeBigJudangchiExpression(Sprite newSprite)
    {
        if (newSprite != null)
        {
            judangchiBigImage.sprite = newSprite;
        }
    }

    public void ToggleWarningUI(bool isActive)
    {
        if (warningUI != null) warningUI.SetActive(isActive);
    }

    // ==========================================
    // 눈 깜빡임 연출 (기존 로직 완벽 유지)
    // ==========================================
    public void WakeUp(Action onComplete = null)
    {
        if (eyeEffect != null)
        {
            eyeEffect.enabled = true;
            StartCoroutine(WakeUpRoutine(onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator WakeUpRoutine(Action onComplete)
    {
        eyeEffect.openAmount = 0.001f;
        eyeEffect.expand = 0.0f;
        float t = 0;

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

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed * 2;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (openSpeed * 1.5f);
            eyeEffect.expand = Mathf.Lerp(0.0f, 1.5f, t);
            yield return null;
        }

        eyeEffect.enabled = false;
        onComplete?.Invoke();
    }
}