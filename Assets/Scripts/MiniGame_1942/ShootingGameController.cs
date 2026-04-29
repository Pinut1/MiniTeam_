using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    // 담당: 김영욱
    // 전체 게임 흐름 총괄 (웨이브 → 보스 → 클리어/실패)
    public class ShootingGameController : MonoBehaviour, IMiniGame
    {
        [Header("연출")]
        public GameObject spaceshipRewardObj;  // 클리어 시 등장할 우주선 오브젝트

        private bool isGameOver = false;

        void Start()
        {
            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(false);
        }

        void Update() { }

        public void OnGameClear()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("[1942] Game Clear! 우주선 획득");

            // 우주선 오브제 등장 연출
            if (spaceshipRewardObj != null)
                spaceshipRewardObj.SetActive(true);

            // 2초 후 허브로 복귀
            Invoke(nameof(ExitToHub), 2f);
        }

        public void OnGameFail()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("[1942] Game Fail!");
            Invoke(nameof(ExitToHub), 1.5f);
        }

        void ExitToHub()
        {
            MiniGameManager.Instance.OnMiniGameClear();
        }
    }
}
