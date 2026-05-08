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

        [Header("옵션 내부 슬라이더 (선택)")]
        public Slider masterVolumeSlider;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;

        private bool isOpen = false;

        /// <summary>
        /// Enforces a single persistent instance of this manager and prevents duplicates.
        /// </summary>
        /// <remarks>
        /// If another instance already exists, this GameObject is destroyed. Otherwise the instance
        /// reference is set and the GameObject is marked to persist across scene loads.
        /// </remarks>
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initializes the options UI: hides the options panel (if assigned) and configures volume sliders.
        /// </summary>
        /// <remarks>
        /// Called by Unity when the component becomes active at the start of its lifecycle.
        /// </remarks>
        void Start()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            InitSliders();
        }

        /// <summary>
        /// Monitors input each frame and toggles the options panel when the Escape key is pressed.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();
        }

        /// <summary>
        /// Toggles the options panel between open and closed states.
        /// </summary>

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        /// <summary>
        /// Opens the options panel and pauses game time.
        /// </summary>
        /// <remarks>
        /// Marks the manager as open, sets Time.timeScale to 0, and activates the assigned optionsPanel if present.
        /// </remarks>
        public void Open()
        {
            isOpen = true;
            Time.timeScale = 0f;
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        /// <summary>
        /// Closes the options panel, resumes normal game time, and marks the panel as not open.
        /// </summary>
        public void Close()
        {
            isOpen = false;
            Time.timeScale = 1f;
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        /// <summary>
        /// Forcefully closes the options panel and marks it as closed without altering the game's time scale.
        /// </summary>
        /// <remarks>
        /// Deactivates the configured optionsPanel GameObject if assigned. This method does not modify Time.timeScale.
        /// </remarks>
        public void ForceClose()
        {
            isOpen = false;
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        // ── 버튼 콜백 ─────────────────────────────

        /// <summary>
/// Closes the options panel in response to the resume button.
/// </summary>
        public void OnResumeClicked() => Close();

        /// <summary>
        /// Closes the options panel and either exits the active mini-game or quits the application.
        /// </summary>
        public void OnExitClicked()
        {
            Close();
            if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsInMiniGame)
                MiniGameManager.Instance.ExitMiniGame();
            else
                Application.Quit();
        }

        /// <summary>
        /// Initializes assigned volume sliders with current values from the SoundManager and registers their change listeners to update SoundManager volumes.
        /// </summary>
        /// <remarks>
        /// If <c>SoundManager.Instance</c> is null, the method returns without modifying any sliders. Only sliders that are assigned (non-null) are updated and wired.
        /// </remarks>

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
