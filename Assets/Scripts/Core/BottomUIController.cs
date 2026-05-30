using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MiniTeam.Core;

public class BottomUIController : MonoBehaviour
{
    public static BottomUIController Instance { get; private set; }

    [Header("UI Objects")]
    [SerializeField] private GameObject judangchiSmallObj;
    [SerializeField] private GameObject digiviceObj;
    public GameObject exclamationMark;
    [SerializeField] private Image[] digiviceBtns;

    [Header("Animators")]
    [SerializeField] private Animator judangchiAnimator;
    [SerializeField] private Animator digiviceAnimator;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (judangchiSmallObj != null)
        {
            judangchiSmallObj.GetComponent<Button>().onClick.AddListener(OnJudangchiClicked);
            judangchiSmallObj.SetActive(false); // 오프닝 중 거슬리지 않게 시작 시 강제 숨김
        }

        if (digiviceObj != null)
        {
            digiviceObj.GetComponent<Button>().onClick.AddListener(OnDigiviceClicked);
            digiviceObj.SetActive(false); // 오프닝 중 거슬리지 않게 시작 시 강제 숨김
        }

        if (digiviceBtns != null)
        {
            foreach (var img in digiviceBtns)
            {
                if (img != null) img.enabled = false;
            }
        }
    }


    public void Show(int stage)
    {
        InitializeLogic(stage);
        if (judangchiAnimator != null) judangchiAnimator.SetTrigger("Show");
        if(digiviceAnimator != null) digiviceAnimator.SetTrigger("Show");
       
    }

    public void PlayClickAnimation(int stage)
    {
        if (stage == 0)
        {
            if (judangchiSmallObj != null) judangchiSmallObj.GetComponent<Button>().interactable = false;
            if (judangchiAnimator != null) judangchiAnimator.SetTrigger("Click");
        }
        else
        {
            if (digiviceObj != null) digiviceObj.GetComponent<Button>().interactable = false;
            if (digiviceAnimator != null) digiviceAnimator.SetTrigger("Click");
        }
    }

    public void PlayHideAnimation(int stage)
    {
       
        if (digiviceObj != null) digiviceObj.GetComponent<Button>().interactable = false;
        if (digiviceAnimator != null) digiviceAnimator.SetTrigger("Hide");
 
    }

    public void PlaySpecialAnimation(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName)) return;
        if (digiviceAnimator != null) digiviceAnimator.SetTrigger(triggerName);
    }

    public void InitializeLogic(int stage)
    {
        if (stage >= 1)
        {
            judangchiSmallObj?.SetActive(false);
            if (digiviceObj != null)
            {
                digiviceObj.SetActive(true);
                digiviceObj.GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            if (digiviceObj != null) digiviceObj.SetActive(false);
            
            if (judangchiSmallObj != null)
            {
                bool wasActive = judangchiSmallObj.activeSelf;
                judangchiSmallObj.SetActive(true);
                judangchiSmallObj.GetComponent<Button>().interactable = true;

                if (!wasActive && AudioManager.Instance != null && AudioManager.Instance.sfxSmallJudangchiAppear != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSmallJudangchiAppear, 0.1f); 
                }
            }
        }

        if (stage == 0 && MiniGameManager.Instance != null && !MiniGameManager.Instance.progressData.isCutscenePlayed)
        {
            StartCoroutine(ShowExclamationWithDelay(0.75f));
        }
        else
        {
            UpdateExclamationMark();
        }

        UpdateDigiviceButtons(stage);
    }

    public void UpdateExclamationMark()
    {
        if (exclamationMark == null || MiniGameManager.Instance == null) return;
        bool shouldShow = (MiniGameManager.Instance.progressData.currentStage == 0) && !MiniGameManager.Instance.progressData.isCutscenePlayed;
        exclamationMark.SetActive(shouldShow);
    }

    private void UpdateDigiviceButtons(int stage)
    {
        if (digiviceBtns == null || digiviceBtns.Length == 0) return;

        for (int i = 0; i < digiviceBtns.Length; i++)
        {
            if (digiviceBtns[i] == null) continue;
            int requiredStage = 2 + i;
            bool isUnlocked = stage >= requiredStage;
            digiviceBtns[i].enabled = isUnlocked;
        }
    }

    private IEnumerator ShowExclamationWithDelay(float delay)
    {
        if (exclamationMark != null) exclamationMark.SetActive(false);
        yield return new WaitForSecondsRealtime(delay);
        UpdateExclamationMark();
    }

    private void OnJudangchiClicked()
    {
        if (judangchiSmallObj != null) judangchiSmallObj.GetComponent<Button>().interactable = false;
        MiniGameManager.Instance?.SetCutscenePlayed(true);
        HubCutsceneDirector.Instance?.PlaySequenceForFirstStage();
    }

    private void OnDigiviceClicked()
    {
        if (digiviceObj != null) digiviceObj.GetComponent<Button>().interactable = false;
        MiniGameManager.Instance?.SetCutscenePlayed(true);
        HubCutsceneDirector.Instance?.PlaySequenceForCurrentStage();
    }
}
