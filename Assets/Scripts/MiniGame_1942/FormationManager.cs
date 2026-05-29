using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    // 역할: 편대원 상태 / HP 관리
    // 피격 이벤트는 PlayerHit.cs에서 호출
    public class FormationManager : MonoBehaviour
    {
        [Header("편대원 오브젝트 (Inspector에서 연결)")]
        public GameObject buttercup;  // 첫 번째로 이탈
        public GameObject bubbles;    // 두 번째로 이탈
        public GameObject blossom;    // 마지막 (게임오버)

        public int CurrentHP { get; private set; } = 3;

        void Start()
        {
            SetActive(buttercup, true);
            SetActive(bubbles,   true);
            SetActive(blossom,   true);
            CurrentHP = 3;
            ShootingUIManager.Instance?.UpdateHpIcons(CurrentHP);
        }

        public void TakeHit()
        {
            CurrentHP--;

            switch (CurrentHP)
            {
                case 2: SetActive(buttercup, false); break;
                case 1: SetActive(bubbles,   false); break;
                case 0: SetActive(blossom,   false); GameOver(); break;
            }

            ShootingUIManager.Instance?.UpdateHpIcons(CurrentHP);
            Debug.Log($"[Formation] 피격 - 남은 HP: {CurrentHP}");
        }

        public void Recover()
        {
            if (CurrentHP >= 3) return;
            CurrentHP++;

            switch (CurrentHP)
            {
                case 2: SetActive(bubbles,   true); break;
                case 3: SetActive(buttercup, true); break;
            }

            ShootingUIManager.Instance?.UpdateHpIcons(CurrentHP);
            Debug.Log($"[Formation] 복귀 - 현재 HP: {CurrentHP}");
        }

        public void FullRestore()
        {
            CurrentHP = 3;
            SetActive(blossom,   true);
            SetActive(bubbles,   true);
            SetActive(buttercup, true);
            ShootingUIManager.Instance?.UpdateHpIcons(CurrentHP);
        }

        void GameOver()
        {
            Debug.Log("[Formation] 게임오버");
            var gc = FindAnyObjectByType<ShootingGameController>();
            if (gc != null)
                gc.OnGameFail();
            else if (MiniGameManager.Instance != null)
                MiniGameManager.Instance.OnMiniGameFail();
        }

        void SetActive(GameObject obj, bool active)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}
