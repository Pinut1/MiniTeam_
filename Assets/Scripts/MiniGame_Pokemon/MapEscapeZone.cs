using UnityEngine;
using TMPro;

namespace MiniTeam.Pokemon
{
    // 탈출구 트리거: 아이템 보유 시 게임 클리어, 미보유 시 안내 메시지
    public class MapEscapeZone : MonoBehaviour
    {
        [Header("안내 UI")]
        public GameObject noticePanel;
        public TextMeshProUGUI noticeText;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var gc = PokemonGameController.Instance;
            if (gc == null) return;

            if (gc.IsPokemonEventDone)
            {
                gc.OnGameClear();
            }
            else
            {
                ShowNotice("오브제 없이는 탈출할 수 없다!");
            }
        }

        void ShowNotice(string message)
        {
            if (noticePanel != null) noticePanel.SetActive(true);
            if (noticeText  != null) noticeText.text = message;
            Invoke(nameof(HideNotice), 2f);
        }

        void HideNotice()
        {
            if (noticePanel != null) noticePanel.SetActive(false);
        }
    }
}
