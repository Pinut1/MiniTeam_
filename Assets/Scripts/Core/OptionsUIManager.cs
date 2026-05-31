using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Core
{
    // 모든 씬에서 ESC로 열리는 공통 옵션 패널
    // Hub 씬 오브젝트에 붙이고 DontDestroyOnLoad로 유지
    public class OptionsUIManager : MonoBehaviour
    {
        public static OptionsUIManager Instance { get; private set; }

        [Header("패널")]
        public GameObject optionsPanel;

        [Header("애니메이션")]
        [Tooltip("옵션 패널의 애니메이터")]
        public Animator optionsAnimator;
        [Tooltip("닫기(TurnOff) 애니메이션이 완료될 때까지 대기할 시간 (초)")]
        public float closeDelay = 0.5f;

        [Header("옵션 값 슬라이더 (선택)")]
        public Slider masterVolumeSlider;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;

        [Header("Key Guide")]
        public GameObject keyGuidePanel;
        public Image keyGuideImage;
        public TMP_Text keyGuideTitleText;
        public Sprite hubKeyGuideSprite;
        public Sprite[] miniGameKeyGuides;
        public GameObject keyGuideExclamation;

        public bool IsOpen => isOpen;
        private bool isOpen = false;
            private Coroutine closeCoroutine;
    private float lastToggleTime = 0f;
    private const float TOGGLE_COOLDOWN = 1.45f; // 애니메이션 시간(1.3초)보다 살짝 여유있게 방어

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            InitSliders();

            if (keyGuideExclamation != null)
            {
                int hasViewed = PlayerPrefs.GetInt("HasViewedKeyGuide", 0);
                var img = keyGuideExclamation.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = (hasViewed == 0);
            }
        }        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 광클 방지 (쿨다운)
                if (Time.unscaledTime - lastToggleTime < TOGGLE_COOLDOWN) return;
                lastToggleTime = Time.unscaledTime;

                // HubUIManager에 경고창이 떠 있다면 옵션창을 열지 않음
                if (HubUIManager.Instance != null && HubUIManager.Instance.IsWarningUIActive)
                {
                    return;
                }

                if (keyGuidePanel != null && keyGuidePanel.activeSelf)
                    CloseKeyGuide();
                else
                    Toggle();
            }

            // R키를 누르면 강제로 미니게임 실패(Regame 효과) 처리
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsInMiniGame)
                {
                    ForceClose();
                    MiniGameManager.Instance.OnMiniGameFail();
                }
            }
        }

        // ── 패널 열기/닫기 ───────────────────────

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }

            isOpen = true;
            Time.timeScale = 0f;
            if (optionsPanel != null) optionsPanel.SetActive(true);
            if (optionsAnimator != null) optionsAnimator.SetTrigger("TurnOn");
        }

        public void Close(System.Action onClosed = null)
        {
            isOpen = false;

            if (optionsAnimator != null)
            {
                optionsAnimator.SetTrigger("TurnOff");
                if (closeCoroutine != null) StopCoroutine(closeCoroutine);
                closeCoroutine = StartCoroutine(DisablePanelAfterAnimation(onClosed));
            }
            else
            {
                Time.timeScale = 1f;
                if (optionsPanel != null) optionsPanel.SetActive(false);
                onClosed?.Invoke();
            }
        }

        private System.Collections.IEnumerator DisablePanelAfterAnimation(System.Action onClosed)
        {
            // Time.timeScale이 0인 상태에서 동작했을 수 있으므로 Realtime으로 대기합니다.
            yield return new WaitForSecondsRealtime(closeDelay);
            if (!isOpen)
            {
                Time.timeScale = 1f;
                if (optionsPanel != null) optionsPanel.SetActive(false);
                onClosed?.Invoke();
            }
            closeCoroutine = null;
        }

        // 게임 종료/클리어 시 강제로 닫기
        public void ForceClose()
        {
            isOpen = false;
            Time.timeScale = 1f; // 안전하게 시간 복원
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        // ── 버튼 콜백 ─────────────────────────────

        // "게임으로 돌아가기" 버튼
        public void OnResumeClicked() => Close();

        // "나가기" 버튼 — 미니게임 중이면 허브로, 허브면 앱 종료
        public void OnExitClicked()
        {
            Close(() =>
            {
                if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsInMiniGame)
                    MiniGameManager.Instance.ExitMiniGame();
                else
                    Application.Quit();
            });
        }

        // 키 가이드 버튼 콜백
        public void OnKeyGuideClicked()
        {
            if (keyGuideExclamation != null)
            {
                var img = keyGuideExclamation.GetComponent<UnityEngine.UI.Image>();
                if (img != null && img.enabled)
                {
                    PlayerPrefs.SetInt("HasViewedKeyGuide", 1);
                    PlayerPrefs.Save();
                    img.enabled = false;
                }
            }

            if (keyGuidePanel == null || keyGuideImage == null) return;
            
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName == "Hub" || sceneName == "Main")
            {
                keyGuideImage.sprite = hubKeyGuideSprite;
                if (keyGuideTitleText != null) keyGuideTitleText.text = "Hub 조작법";
            }
            else
            {
                int stage = (MiniGameManager.Instance != null) ? MiniGameManager.Instance.currentStage : 1;
                int index = stage - 1;
                
                if (miniGameKeyGuides != null && index >= 0 && index < miniGameKeyGuides.Length && miniGameKeyGuides[index] != null)
                {
                    keyGuideImage.sprite = miniGameKeyGuides[index];
                }
                else
                {
                    keyGuideImage.sprite = hubKeyGuideSprite; // fallback
                }

                if (keyGuideTitleText != null) keyGuideTitleText.text = $"스테이지 {stage} 조작법";
            }
            
            keyGuidePanel.SetActive(true);
        }

        // 키 가이드 패널 닫기 버튼 콜백
        public void CloseKeyGuide()
        {
            if (keyGuidePanel != null) keyGuidePanel.SetActive(false);
        }

        // ── 볼륨 슬라이더 ─────────────────────────

        void OnDestroy()
        {
            var sm = SoundManager.Instance;
            if (sm == null) return;
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(sm.SetMasterVolume);
            if (bgmVolumeSlider    != null) bgmVolumeSlider.onValueChanged.RemoveListener(sm.SetBGMVolume);
            if (sfxVolumeSlider    != null) sfxVolumeSlider.onValueChanged.RemoveListener(sm.SetSFXVolume);
        }

        void InitSliders()
        {
            if (SoundManager.Instance == null) return;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = SoundManager.Instance.masterVolume;
                masterVolumeSlider.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
            }
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = SoundManager.Instance.bgmVolume;
                bgmVolumeSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = SoundManager.Instance.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
            }
        }
    }
}
