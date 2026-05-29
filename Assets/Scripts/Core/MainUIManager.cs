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
    [Tooltip("PressAnyButton → 메인 메뉴 전환 시간 (초)")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("화면을 가릴 검정 UI 패널")]
    [SerializeField] private Image fadePanel;
    [Tooltip("페이드아웃에 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 2.0f;

    // 중복 클릭 방지 플래그
    private bool isTranstioning = false;
    private bool isPressAnyButtonActive = true;
    private const string SAVE_STAGE_KEY = "SavedCurrentStage";

    private void Start()
    {
        // PressAnyButton 화면 켜기, 메인 메뉴 숨기기
        if (pressAnyButtonPanel != null) pressAnyButtonPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        StartCoroutine(BlinkRoutine());
    }

    private void Update()
    {
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
        if (isTranstioning) return;

        // 기존 저장 데이터가 있는지 확인
        if (PlayerPrefs.HasKey(SAVE_STAGE_KEY))
        {
            Debug.Log($"[MainUIManager] 기존 세이브 데이터 발견 (Key: {SAVE_STAGE_KEY}). 경고 애니메이션 호출 시도.");
            
            // 경고창 애니메이션 실행
            if (uiAnimator != null)
            {
                Debug.Log("[MainUIManager] uiAnimator.SetTrigger(\"NewGameWarning\") 실행");
                uiAnimator.SetTrigger("NewGameWarning");
            }
            else
            {
                Debug.LogError("[MainUIManager] uiAnimator가 연결되어 있지 않습니다! 인스펙터를 확인하세요.");
            }
        }
        else
        {
            Debug.Log("[MainUIManager] 기존 세이브 데이터 없음. 바로 게임 시작.");
            // 바로 시작
            ConfirmStartNewGame();
        }
    }

    // 경고창에서 '예(Yes)'를 눌렀을 때 호출
    public void ConfirmStartNewGame()
    {
        if (isTranstioning) return;
        isTranstioning = true;
        
        // 새로 시작할 때 기존 세이브 데이터 초기화
        PlayerPrefs.DeleteKey(SAVE_STAGE_KEY);
        PlayerPrefs.DeleteKey("SavedCutscenePlayed");
        PlayerPrefs.Save();

        // 페이드아웃 후 Hub 씬으로 전환
        PlayFadeOut(() => SceneManager.LoadScene("Hub"));
    }

    // 경고창에서 '아니오(No)'를 눌렀을 때 호출
    public void CancelStartNewGame()
    {
        Debug.Log("[MainUIManager] CancelStartNewGame() 호출됨. 복귀 애니메이션 실행 시도.");
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

        // 저장된 스테이지 정보가 있는 채로 Hub 씬 로드
        PlayFadeOut(() => SceneManager.LoadScene("Hub"));
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