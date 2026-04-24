using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Sponge
{
    // 담당: 장한나
    public class SpongeGameController : MonoBehaviour, IMiniGame
    {
        void Start()
        {
            // TODO: 스폰지밥 게임 초기화
        }

        void Update()
        {
            // TODO: 게임 로직
        }

        public void OnGameClear()
        {
            Debug.Log("[Sponge] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            Debug.Log("[Sponge] Game Fail!");
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
