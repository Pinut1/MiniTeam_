using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    // 담당: 김영욱
    public class ShootingGameController : MonoBehaviour, IMiniGame
    {
        void Start()
        {
            // TODO: 1942 슈팅 게임 초기화
        }

        void Update()
        {
            // TODO: 플레이어 이동, 총알 발사, 적 스폰
        }

        public void OnGameClear()
        {
            Debug.Log("[1942] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            Debug.Log("[1942] Game Fail!");
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
