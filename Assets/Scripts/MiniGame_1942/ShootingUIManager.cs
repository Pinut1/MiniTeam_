using UnityEngine;
using UnityEngine.UI;

namespace MiniTeam.Shooting1942
{
    // HP 표시 / 보스 HP바 / 웨이브 안내
    public class ShootingUIManager : MonoBehaviour
    {
        public static ShootingUIManager Instance { get; private set; }

        [Header("편대원 HP 아이콘 (3개, Inspector 연결)")]
        public GameObject[] hpIcons;  // 버터컵/버블스/블로섬 아이콘 순서

        [Header("보스 HP바")]
        public GameObject bossHpPanel;
        public Slider bossHpSlider;

        [Header("웨이브 안내 텍스트")]
        public GameObject wavePanel;
        public Text waveText;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (bossHpPanel != null) bossHpPanel.SetActive(false);
            if (wavePanel    != null) wavePanel.SetActive(false);
        }

        // FormationManager에서 피격 시 호출
        public void UpdateHpIcons(int currentHp)
        {
            for (int i = 0; i < hpIcons.Length; i++)
            {
                if (hpIcons[i] != null)
                    hpIcons[i].SetActive(i < currentHp);
            }
        }

        // 보스 HP바 갱신
        public void UpdateBossHp(int current, int max)
        {
            if (bossHpPanel != null) bossHpPanel.SetActive(true);
            if (bossHpSlider != null)
                bossHpSlider.value = (float)current / max;
        }

        // 웨이브 전환 안내
        public void ShowWaveMessage(string message, float duration = 2f)
        {
            if (wavePanel == null || waveText == null) return;
            waveText.text = message;
            wavePanel.SetActive(true);
            Invoke(nameof(HideWaveMessage), duration);
        }

        void HideWaveMessage()
        {
            if (wavePanel != null) wavePanel.SetActive(false);
        }
    }
}
