using UnityEngine;
using UnityEngine.UI;

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

        [Header("옵션 내부 슬라이더 (선택)")]
        public Slider masterVolumeSlider;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;

        private bool isOpen = false;
        private Coroutine closeCoroutine;

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
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();
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

        public void Close()
        {
            isOpen = false;
            Time.timeScale = 1f;

            if (optionsAnimator != null)
            {
                optionsAnimator.SetTrigger("TurnOff");
                if (closeCoroutine != null) StopCoroutine(closeCoroutine);
                closeCoroutine = StartCoroutine(DisablePanelAfterAnimation());
            }
            else
            {
                if (optionsPanel != null) optionsPanel.SetActive(false);
            }
        }

        private System.Collections.IEnumerator DisablePanelAfterAnimation()
        {
            // Time.timeScale이 0인 상태에서 동작했을 수 있으므로 Realtime으로 대기합니다.
            yield return new WaitForSecondsRealtime(closeDelay);
            if (!isOpen && optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
            closeCoroutine = null;
        }

        // 게임 종료/클리어 시 강제로 닫기
        public void ForceClose()
        {
            isOpen = false;
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
            Close();
            if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsInMiniGame)
                MiniGameManager.Instance.ExitMiniGame();
            else
                Application.Quit();
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
