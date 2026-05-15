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

    [Header("시네마틱 통합 애니메이터 (부모 객체)")]

    [SerializeField] private Animator cinemaAnimator;

    [Header("하단 상호작용 객체 ")]
    [SerializeField] private GameObject judangchiSmallObj; // Judanchi_초기
    [SerializeField] private GameObject digiviceObj;       // 디지바이스 GameObject

    [Header("대화창용 큰 주댕치 ")]
    [SerializeField] private Image judangchiBigImage;      // Judanchi_기본의 Image 컴포넌트

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f; // 애니메이션 재생 대기 시간

    #region UNITY LIFE CYCLE
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 버튼 이벤트 연결 (GameObject에서 Button 컴포넌트 추출)
        if (judangchiSmallObj != null)
            judangchiSmallObj.GetComponent<Button>().onClick.AddListener(OnBottomUIClickedJudangchi);

        if (digiviceObj != null)
            digiviceObj.GetComponent<Button>().onClick.AddListener(OnBottomUIClickedDigivice);

        // 씬이 처음 로드되거나 복귀했을 때, 현재 스테이지에 맞춰 초기 UI를 띄워둡니다.
        // InitializeBottomUI(MiniGameManager.Instance.currentStage);
    }

    #endregion
    public void OnBottomUIClickedJudangchi()
    {
      
        // 클릭 중복 방지를 위해 버튼 기능을 잠시 끕니다.
        judangchiSmallObj.GetComponent<Button>().interactable = false;
       
        // Controller에게 연출 시작을 보고
        JudangChiController.Instance.PlaySequenceForFirstStage();
    }

    public void OnBottomUIClickedDigivice()
    {
        digiviceObj.GetComponent<Button>().interactable = false;
        // Controller에게 연출 시작을 보고
        JudangChiController.Instance.PlaySequenceForCurrentStage();
    }

   
    public void FirstCinemaEnter()
    {
        // Event Marker에 의해 작동하는 bool trigger를 초기화
        isFirstCinemaEnterDone = false;
        // 하나의 트리거로 입장 연출(하단 퇴장 -> 레터박스 -> 큰 주댕치)을 한방에 재생!
        cinemaAnimator.SetTrigger("FirstCinemaEnter");
      
    }

    public IEnumerator FirstCinemaExit(int currentStage)
    {
        // 퇴장 연출을 재생하기 직전에, 다시 올라와야 할 하단 UI를 미리 세팅해줍니다.
        InitializeBottomUI(currentStage);

        // 하나의 트리거로 퇴장 연출(큰 주댕치 퇴장 -> 레터박스 치우기 -> 하단 입장)을 한방에 재생!
        cinemaAnimator.SetTrigger("FirstCinemaExit");
        yield return new WaitForSeconds(1.2f);
    }

    public void StageClearOnCinema()
    {
        InitializeBottomUI(1);
        digiviceObj.GetComponent<Button>().interactable = false;

        isNormalCinemaEnterDone = false;
        cinemaAnimator.SetTrigger("StageClearOnCinema");
      
    }

    public void PlayCinemaEnter()
    {
        digiviceObj.GetComponent<Button>().interactable = false;
        isNormalCinemaEnterDone = false;
        cinemaAnimator.SetTrigger("CinemaEnter");
       
    }

    public IEnumerator PlayCinemaExit(int currentStage)
    {
        // 퇴장 연출을 재생하기 직전에, 다시 올라와야 할 하단 UI를 미리 세팅해줍니다.
        InitializeBottomUI(currentStage);

        // 하나의 트리거로 퇴장 연출(큰 주댕치 퇴장 -> 레터박스 치우기 -> 하단 입장)을 한방에 재생!
        cinemaAnimator.SetTrigger("CinemaExit");
        yield return new WaitForSeconds(1.9f);
        digiviceObj.GetComponent<Button>().interactable = true;
    }

    private void InitializeBottomUI(int stage)
    {
        if (stage >= 1)
        {
            judangchiSmallObj.SetActive(false);
            if (digiviceObj != null)
            {
                digiviceObj.SetActive(true);
               
            }
        }
        else
        {
            if (digiviceObj != null) digiviceObj.SetActive(false);
            judangchiSmallObj.SetActive(true);
            judangchiSmallObj.GetComponent<Button>().interactable = true;
        }
    }


   


    // ==========================================
    // 눈 깜빡임 연출 
    // ==========================================
    #region EyeBlank
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


        if (judangchiSmallObj.activeSelf)
        {
            judangchiSmallObj.SetActive(false);
        }

        else if (digiviceObj.activeSelf)
        {
            digiviceObj.SetActive(false);
        }
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
        InitializeBottomUI(MiniGameManager.Instance.currentStage);
        onComplete?.Invoke();
    }
    #endregion

    #region ETC
    // 표정 변경 함수 (유지)
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
    #endregion


    // Animator Event Marker 제어
    public bool isFirstCinemaEnterDone { get; private set; } = false;
    public void CompleteFirstCinemaEnter()
    {
        isFirstCinemaEnterDone = true;
    }

    public bool isNormalCinemaEnterDone { get; private set; } = false;
    public void CompleteNormalCinemaEnter()
    {
        isNormalCinemaEnterDone = true;
    }
    

}