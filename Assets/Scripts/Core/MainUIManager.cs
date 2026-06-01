using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MiniTeam.Core;

public class MainUIManager : MonoBehaviour
{
    [Header("Press Any Button Screen")]
    [SerializeField] private GameObject pressAnyButtonPanel;
    [SerializeField] private TMP_Text pressAnyButtonText;
    [Tooltip("텍스트 깜빡임 속도 (높을수록 빠름)")]
    [SerializeField] private float blinkSpeed = 1.5f;

    [Header("UI Animator")]
    [Tooltip("메인 메뉴 애니메이터 (경고창 애니메이션용)")]
    [SerializeField] private Animator uiAnimator;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [Tooltip("이어하기 버튼 (세이브 없으면 비활성화)")]
    [SerializeField] private GameObject continueButton;
    [Tooltip("PressAnyButton → 메인 메뉴 전환 시간 (초)")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("화면을 가릴 검정 UI 패널")]
    [SerializeField] private Image fadePanel;
    [Tooltip("페이드아웃에 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 2.0f;
    
    [Header("Hub Integration Settings")]
    [Tooltip("메인 메뉴 전용 카메라 (게임 시작 시 꺼짐)")]
    public GameObject menuCamera;

    // 상태 플래그
    private bool isTranstioning = false;
    private bool isPressAnyButtonActive = true;
    public bool isWarningActive = false;
    private const string SAVE_STAGE_KEY = "SavedCurrentStage";

    private void Start()
    {
        // 통합 씬(Hub)에서 이미 게임이 진행 중인데 씬이 다시 로드된 경우 (예: 미니게임 끝나고 복귀)
        if (MiniGameManager.Instance != null && !MiniGameManager.Instance.isMainMenuActive)
        {
            if (menuCamera != null) Destroy(menuCamera);
            Destroy(gameObject);
            return;
        }

        // PressAnyButton 화면 켜기, 메인 메뉴 숨기기
        if (pressAnyButtonPanel != null) pressAnyButtonPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        // 세이브 데이터 여부에 따라 이어하기 버튼 자체를 켜고 끄기 (레이아웃 그룹 자동 정렬됨)
        if (continueButton != null)
        {
            continueButton.SetActive(PlayerPrefs.HasKey(SAVE_STAGE_KEY));
        }

        StartCoroutine(BlinkRoutine());
    }

    private void LateUpdate()
    {
        // 새 게임 경고창이 켜져있을 때 ESC를 누르면 취소(No) 처리
        if (isWarningActive && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelStartNewGame();
            return;
        }

        if (!isPressAnyButtonActive || isTranstioning) return;

        // 아무 키 / 마우스 클릭 / 게임패드 버튼 감지
        // ESC는 OptionsUIManager가 처리하므로 제외
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            isPressAnyButtonActive = false;
            StartCoroutine(TransitionToMainMenu());
        }
    }

    // 사인 곡선으로 텍스트를 부드럽게 깜빡이는 코루틴
    private IEnumerator BlinkRoutine()
    {
        if (pressAnyButtonText == null) yield break;

        while (isPressAnyButtonActive)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f;
            Color color = pressAnyButtonText.color;
            color.a = alpha;
            pressAnyButtonText.color = color;
            yield return null;
        }

        // 전환 시 텍스트를 완전히 불투명하게 고정
        Color finalColor = pressAnyButtonText.color;
        finalColor.a = 1f;
        pressAnyButtonText.color = finalColor;
    }

    // PressAnyButton 패널을 페이드아웃하고 메인 메뉴를 페이드인하는 전환 코루틴
    private IEnumerator TransitionToMainMenu()
    {
        isTranstioning = true;

        // 1. PressAnyButton 패널 페이드 아웃
        CanvasGroup pressCg = pressAnyButtonPanel.GetComponent<CanvasGroup>();
        if (pressCg == null) pressCg = pressAnyButtonPanel.AddComponent<CanvasGroup>();

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            pressCg.alpha = Mathf.Lerp(1f, 0f, t / transitionDuration);
            yield return null;
        }
        pressAnyButtonPanel.SetActive(false);

        // 2. 메인 메뉴 패널 페이드 인
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            CanvasGroup menuCg = mainMenuPanel.GetComponent<CanvasGroup>();
            if (menuCg == null) menuCg = mainMenuPanel.AddComponent<CanvasGroup>();

            t = 0f;
            menuCg.alpha = 0f;
            while (t < transitionDuration)
            {
                t += Time.deltaTime;
                menuCg.alpha = Mathf.Lerp(0f, 1f, t / transitionDuration);
                yield return null;
            }
            menuCg.alpha = 1f;
        }

        isTranstioning = false;
    }

    public void GameStart()
    {
        if (isTranstioning || isWarningActive) return;

        //// 세이브 데이터가 있는지 확인
        //if (PlayerPrefs.HasKey(SAVE_STAGE_KEY))
        //{
        //    Debug.Log($"[MainUIManager] 세이브 데이터 발견 (Key: {SAVE_STAGE_KEY}). 경고 애니메이션 호출 시도.");
            
        //    // 경고창 애니메이션 재생
        //    if (uiAnimator != null)
        //    {
        //        isWarningActive = true;
        //        Debug.Log("[MainUIManager] uiAnimator.SetTrigger(\"NewGameWarning\") 호출");
        //        uiAnimator.SetTrigger("NewGameWarning");
        //    }
        //    else
        //    {
        //        Debug.LogError("[MainUIManager] uiAnimator가 할당되어 있지 않습니다! 인스펙터를 확인하세요.");
        //    }
        //}
        //else
        //{
        //    Debug.Log("[MainUIManager] 세이브 데이터 없음. 바로 새 게임 시작.");
        //    // 바로 시작
        //    ConfirmStartNewGame();
        //}

        // 세이브 데이터가 있는지 확인
       
        Debug.Log($"[MainUIManager] 세이브 데이터 발견 (Key: {SAVE_STAGE_KEY}). 경고 애니메이션 호출 시도.");
            
        // 경고창 애니메이션 재생
        if (uiAnimator != null)
        {
            isWarningActive = true;
            Debug.Log("[MainUIManager] uiAnimator.SetTrigger(\"NewGameWarning\") 호출");
            uiAnimator.SetTrigger("NewGameWarning");
        }
        else
        {
            Debug.LogError("[MainUIManager] uiAnimator가 할당되어 있지 않습니다! 인스펙터를 확인하세요.");
        }
       
    }

    // 경고창에서 '예(Yes)'를 눌렀을 때 호출
    public void ConfirmStartNewGame()
    {
        if (isTranstioning) return;
        isTranstioning = true;
        isWarningActive = false;
        
        // 1. 설정 데이터 백업
        float masterVolume = PlayerPrefs.GetFloat("SavedMasterVolume", 1f);
        float bgmVolume = PlayerPrefs.GetFloat("SavedBGMVolume", 0.6f);
        float sfxVolume = PlayerPrefs.GetFloat("SavedSFXVolume", 1f);
        int hasViewedKeyGuide = PlayerPrefs.GetInt("HasViewedKeyGuide", 0);

        // 2. 모든 데이터 완벽 초기화
        PlayerPrefs.DeleteAll();

        // 3. 설정 데이터 복원
        PlayerPrefs.SetFloat("SavedMasterVolume", masterVolume);
        PlayerPrefs.SetFloat("SavedBGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SavedSFXVolume", sfxVolume);
        PlayerPrefs.SetInt("HasViewedKeyGuide", hasViewedKeyGuide);
        PlayerPrefs.Save();

        // 4. 메모리에 살아있는 매니저 초기화
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.ResetSaveData();
        }

        // 이동 대신 Hub 게임플레이 즉시 시작
        PlayFadeOut(() => {
            StartHubGameplay();
        });
    }

    // 경고창에서 '아니오(No)'를 눌렀을 때 호출
    public void CancelStartNewGame()
    {
        isWarningActive = false;
        Debug.Log("[MainUIManager] CancelStartNewGame() 호출. 경고 애니메이션 닫기 시도.");
        if (uiAnimator != null)
        {
            Debug.Log("[MainUIManager] uiAnimator.SetTrigger(\"NewGameReturn\") 실행");
            uiAnimator.SetTrigger("NewGameReturn");
        }
        else
        {
            Debug.LogError("[MainUIManager] uiAnimator가 연결되어 있지 않습니다! 인스펙터를 확인하세요.");
        }
    }

    // 이어하기 버튼 클릭 시 호출되는 메서드
    public void ContinueGame()
    {
        if (isTranstioning) return;
        isTranstioning = true;

        // 씬 이동 대신 Hub 게임플레이 즉시 시작
        PlayFadeOut(() => {
            StartHubGameplay();
        });
    }

    // 통합 씬에서 게임을 실제로 시작하는 로직
    private void StartHubGameplay()
    {
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.StartGameFromMenu();
        }
        
        // 어차피 일회용이므로 메인 카메라와 자기 자신(UI)을 완전히 파괴(Destroy)합니다.
        if (menuCamera != null) Destroy(menuCamera);
        Destroy(gameObject);
    }

    // 게임을 종료하는 함수
    public void QuitGame()
    {
        if (isTranstioning) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("게임 종료 (에디터)");
#else
        Application.Quit();
#endif
    }

    private void PlayFadeOut(Action onComplete)
    {
        StartCoroutine(FadeRoutine(0, 1, onComplete));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, Action onComplete)
    {
        fadePanel?.gameObject.SetActive(true);
        Color color = fadePanel.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadePanel.color = color;

        onComplete?.Invoke();
    }

    // OptionsUIManager가 DontDestroyOnLoad라서 발생하는 연결 끊김 버그 해결용 래퍼 함수
    public void OpenOptions()
    {
        if (OptionsUIManager.Instance != null)
        {
            OptionsUIManager.Instance.Toggle();
        }
    }
}