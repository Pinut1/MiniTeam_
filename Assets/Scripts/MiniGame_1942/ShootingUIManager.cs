using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    public class ShootingUIManager : MonoBehaviour
    {
        public static ShootingUIManager Instance { get; private set; }

        [Header("편대원 HP 아이콘 (3개, Inspector 연결)")]
        public GameObject[] hpIcons;

        [Header("보스 HP바")]
        public GameObject bossHpPanel;
        public Slider bossHpSlider;

        [Header("웨이브 안내 텍스트")]
        public GameObject wavePanel;
        public TextMeshProUGUI waveText;

        [Header("점수 텍스트")]
        public TextMeshProUGUI scoreText;

        [Header("일시정지 패널")]
        public GameObject pausePanel;

        [Header("결과 화면")]
        public GameObject resultPanel;
        public TextMeshProUGUI resultTitleText;
        public TextMeshProUGUI resultScoreText;

        private int score = 0;

        /// <summary>
        /// Ensures only one ShootingUIManager exists by setting the singleton Instance to this object; if another Instance already exists, destroys this GameObject.
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Initializes UI by hiding optional panels (boss HP, wave, pause, result) and updating the score display.
        /// </summary>
        void Start()
        {
            if (bossHpPanel  != null) bossHpPanel.SetActive(false);
            if (wavePanel    != null) wavePanel.SetActive(false);
            if (pausePanel   != null) pausePanel.SetActive(false);
            if (resultPanel  != null) resultPanel.SetActive(false);
            UpdateScoreText();
        }

        /// <summary>
        /// Updates squad HP icon visibility so icons with index less than <paramref name="currentHp"/> are active and all others are inactive.
        /// </summary>
        /// <param name="currentHp">Number of HP icons to show; icons at indices 0 through currentHp - 1 will be activated.</param>

        public void UpdateHpIcons(int currentHp)
        {
            for (int i = 0; i < hpIcons.Length; i++)
                if (hpIcons[i] != null) hpIcons[i].SetActive(i < currentHp);
        }

        /// <summary>
        /// Activates the boss HP UI (if assigned) and updates the boss HP slider to reflect the current health proportion.
        /// </summary>
        /// <param name="current">The boss's current hit points.</param>
        /// <param name="max">The boss's maximum hit points; the slider value is set to current / max.</param>

        public void UpdateBossHp(int current, int max)
        {
            if (bossHpPanel  != null) bossHpPanel.SetActive(true);
            if (bossHpSlider != null) bossHpSlider.value = (float)current / max;
        }

        /// <summary>
        /// Shows a wave message on the HUD and automatically hides it after a specified duration.
        /// </summary>
        /// <param name="message">Text to display in the wave message.</param>
        /// <param name="duration">Time in seconds the message remains visible before being hidden.</param>

        public void ShowWaveMessage(string message, float duration = 2f)
        {
            if (wavePanel == null || waveText == null) return;
            waveText.text = message;
            wavePanel.SetActive(true);
            CancelInvoke(nameof(HideWaveMessage));
            Invoke(nameof(HideWaveMessage), duration);
        }

        /// <summary>
        /// Hides the wave message UI by deactivating the wave panel if it is assigned.
        /// </summary>
        void HideWaveMessage()
        {
            if (wavePanel != null) wavePanel.SetActive(false);
        }

        /// <summary>
        /// Adds the specified number of points to the current score and refreshes the score display.
        /// </summary>
        /// <param name="amount">The number of points to add to the score. May be negative to subtract points.</param>

        public void AddScore(int amount)
        {
            score += amount;
            UpdateScoreText();
        }

        /// <summary>
/// Gets the current accumulated score.
/// </summary>
/// <returns>The current accumulated score.</returns>
public int GetScore() => score;

        /// <summary>
        /// Update the on-screen score text to reflect the current score.
        /// </summary>
        /// <remarks>
        /// If the score text UI element is not assigned (`scoreText` is null), no action is taken.
        /// </remarks>
        void UpdateScoreText()
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        /// <summary>
        /// Shows or hides the pause UI panel.
        /// </summary>
        /// <param name="show">`true` to display the pause panel, `false` to hide it.</param>

        public void ShowPause(bool show)
        {
            if (pausePanel != null) pausePanel.SetActive(show);
        }

        /// <summary>
        /// Displays the result screen and updates its title and score based on the outcome.
        /// </summary>
        /// <param name="isCleared">If true, shows "CLEAR!" as the result title; otherwise shows "GAME OVER".</param>

        public void ShowResult(bool isCleared)
        {
            if (resultPanel == null) return;

            if (resultTitleText != null)
                resultTitleText.text = isCleared ? "CLEAR!" : "GAME OVER";

            if (resultScoreText != null)
                resultScoreText.text = $"SCORE: {score}";

            if (wavePanel != null) wavePanel.SetActive(false);
            resultPanel.SetActive(true);
        }
    }
}
