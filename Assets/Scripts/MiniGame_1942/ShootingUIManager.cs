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

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (bossHpPanel  != null) bossHpPanel.SetActive(false);
            if (wavePanel    != null) wavePanel.SetActive(false);
            if (pausePanel   != null) pausePanel.SetActive(false);
            if (resultPanel  != null) resultPanel.SetActive(false);
            UpdateScoreText();
        }

        // ── HP ───────────────────────────────────

        public void UpdateHpIcons(int currentHp)
        {
            for (int i = 0; i < hpIcons.Length; i++)
                if (hpIcons[i] != null) hpIcons[i].SetActive(i < currentHp);
        }

        // ── 보스 HP ──────────────────────────────

        public void UpdateBossHp(int current, int max)
        {
            if (bossHpPanel  != null) bossHpPanel.SetActive(true);
            if (bossHpSlider != null) bossHpSlider.value = (float)current / max;
        }

        // ── 웨이브 메시지 ─────────────────────────

        public void ShowWaveMessage(string message, float duration = 2f)
        {
            if (wavePanel == null || waveText == null) return;
            waveText.text = message;
            wavePanel.SetActive(true);
            CancelInvoke(nameof(HideWaveMessage));
            Invoke(nameof(HideWaveMessage), duration);
        }

        void HideWaveMessage()
        {
            if (wavePanel != null) wavePanel.SetActive(false);
        }

        // ── 점수 ─────────────────────────────────

        public void AddScore(int amount)
        {
            score += amount;
            UpdateScoreText();
        }

        public int GetScore() => score;

        void UpdateScoreText()
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        // ── 일시정지 ──────────────────────────────

        public void ShowPause(bool show)
        {
            if (pausePanel != null) pausePanel.SetActive(show);
        }

        // ── 결과 화면 ─────────────────────────────

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
