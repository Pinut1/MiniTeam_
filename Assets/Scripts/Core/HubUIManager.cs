using MiniTeam.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubUIManager : MonoBehaviour
{
    public static HubUIManager Instance { get; private set; }

    [Header("WakeUp & Setup (오프닝 연출)")]
    public EyeOpeningEffect eyeEffect;
    [SerializeField] private float openSpeed = 1.5f;

    [Header("Dialogue UI (대화 및 알림 패널)")]
    [SerializeField] private Animator cinemaAnimator;
    [SerializeField] private Image judangchiBigImage;
    [SerializeField] private GameObject warningUI;
    [SerializeField] private TMP_Text warningText;

    [Header("Interaction Objects (하단 상호작용)")]
    [SerializeField] private GameObject judangchiSmallObj;
    [SerializeField] private GameObject digiviceObj;
    public GameObject exclamationMark;
    [SerializeField] private Image[] digiviceBtns;

    [Header("Stage Clear Reward (클리어 연출)")]
    [SerializeField] private Image objectImg;
    [SerializeField] private Sprite[] stageClearSprites;

    
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

        // 시작 시 모든 디지바이스 버튼 이미지 비활성화
        if (digiviceBtns != null)
        {
            foreach (var img in digiviceBtns)
            {
                if (img != null) img.enabled = false;
            }
        }

        // 씬이 처음 로드되거나 복귀했을 때, 현재 스테이지에 맞춰 초기 UI를 띄워줍니다.
        // InitializeBottomUI(MiniGameManager.Instance.currentStage);
    }

    #endregion
    public void OnBottomUIClickedJudangchi()
    {
        judangchiSmallObj.GetComponent<Button>().interactable = false;
        MiniGameManager.Instance?.SetCutscenePlayed(true);
        JudangChiController.Instance.PlaySequenceForFirstStage();
    }

    public void OnBottomUIClickedDigivice()
    {
        digiviceObj.GetComponent<Button>().interactable = false;
        MiniGameManager.Instance?.SetCutscenePlayed(true);
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

    public void StageClear_ObjectGet()
    {
        int currentStage = MiniGameManager.Instance != null ? MiniGameManager.Instance.currentStage : 0;

        // 클리어한 스테이지에 맞춰 획득 오브젝트 스프라이트 교체 (stage 0 클리어 시 currentStage=1이 되므로 index 0 대입)
        int spriteIndex = currentStage - 1;
        if (objectImg != null && stageClearSprites != null)
        {
            if (spriteIndex >= 0 && spriteIndex < stageClearSprites.Length)
            {
                objectImg.sprite = stageClearSprites[spriteIndex];
            }
        }

        if (currentStage >= 2)
        {
            InitializeBottomUI(currentStage);
        }
        else
        {
            if (judangchiSmallObj != null) judangchiSmallObj.SetActive(false);
            if (digiviceObj != null) digiviceObj.SetActive(false);
        }

        if (digiviceObj != null)
        {
            digiviceObj.GetComponent<Button>().interactable = false;
        }

        isNormalCinemaEnterDone = false;
        cinemaAnimator.SetTrigger("StageClear_ObjectGet");
        
        StartCoroutine(RestoreDigiviceButtonAfterDelay(2.5f, currentStage));
    }

    private IEnumerator RestoreDigiviceButtonAfterDelay(float delay, int stage)
    {
        yield return new WaitForSeconds(delay);
        if (stage >= 2)
        {
            if (digiviceObj != null)
            {
                digiviceObj.GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            if (judangchiSmallObj != null) judangchiSmallObj.GetComponent<Button>().interactable = true;
            if (digiviceObj != null) digiviceObj.GetComponent<Button>().interactable = true;
        }
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

    public void InitializeBottomUI(int stage)
    {
        if (stage >= 1)
        {
            judangchiSmallObj.SetActive(false);
        
            if (digiviceObj != null)
            {
                digiviceObj.SetActive(true);
                digiviceObj.GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            if (digiviceObj != null) digiviceObj.SetActive(false);
            
            bool wasActive = judangchiSmallObj.activeSelf;
            judangchiSmallObj.SetActive(true);
            judangchiSmallObj.GetComponent<Button>().interactable = true;

            // 안 보이다가 새로 켜질 때만 효과음 재생
            if (!wasActive && MiniTeam.Core.AudioManager.Instance != null && MiniTeam.Core.AudioManager.Instance.sfxSmallJudangchiAppear != null)
            {
                MiniTeam.Core.AudioManager.Instance.PlaySFX(MiniTeam.Core.AudioManager.Instance.sfxSmallJudangchiAppear, 0.1f); 
            }
        }

        // 오직 0단계이고 아직 컷신을 안 봤을 때만 최초 지연(0.75초) 출현 연출 적용
        if (stage == 0 && MiniGameManager.Instance != null && !MiniGameManager.Instance.isCutscenePlayed)
        {
            StartCoroutine(ShowExclamationWithDelay(0.75f));
        }
        else
        {
            UpdateExclamationMark();
        }

        UpdateDigiviceButtons(stage);
    }

    private void UpdateDigiviceButtons(int stage)
    {
        if (digiviceBtns == null || digiviceBtns.Length == 0) return;

        for (int i = 0; i < digiviceBtns.Length; i++)
        {
            if (digiviceBtns[i] == null) continue;

            // 0-based stage 기준 매핑:
            // 1스테이지 클리어 시(stage = 2) -> digiviceBtns[0] 온 (Image.enabled = true)
            // 2스테이지 클리어 시(stage = 3) -> digiviceBtns[1] 온
            // 3스테이지 클리어 시(stage = 4) -> digiviceBtns[2] 온
            // 4스테이지 클리어 시(stage = 5) -> digiviceBtns[3] 온
            int requiredStage = 2 + i;
            bool isUnlocked = stage >= requiredStage;
            
            digiviceBtns[i].enabled = isUnlocked;
        }
    }
    // 갱신 함수: 오직 '0단계'에서만 보이며, 0단계 컷신을 아직 안 본 상태여야 활성화
    public void UpdateExclamationMark()
    {
        if (exclamationMark == null || MiniGameManager.Instance == null) return;
        bool shouldShow = (MiniGameManager.Instance.currentStage == 0) && !MiniGameManager.Instance.isCutscenePlayed;
        exclamationMark.SetActive(shouldShow);
    }
    // 1회성 지연 출현 코루틴
    private IEnumerator ShowExclamationWithDelay(float delay)
    {
        exclamationMark.SetActive(false); // 먼저 꺼둠
        yield return new WaitForSecondsRealtime(delay);
        UpdateExclamationMark();
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

    public void ToggleWarningUI(bool isOn, string message = "")
    {
        
        if (warningUI != null) warningUI.SetActive(isOn);

        // 창을 켤 때 전달받은 메세지가 비어있지 않다면 텍스트를 업데이트.
        if (isOn && !string.IsNullOrEmpty(message) && warningText != null)
        {
            warningText.text = message;
        }
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

        int currentStage = MiniGameManager.Instance != null ? MiniGameManager.Instance.currentStage : 0;
        if (currentStage == 1)
        {
            InitializeBottomUI(1);
        }
    }

    public void PlaySpecialAnimation(string triggerName)
    {
        if (cinemaAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            cinemaAnimator.SetTrigger(triggerName);
        }
    }
    

}