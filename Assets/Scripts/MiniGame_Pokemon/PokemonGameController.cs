using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Pokemon
{
    // 담당: 차정민 + 황해인
    public class PokemonGameController : MonoBehaviour, IMiniGame
    {
        void Start()
        {
            // TODO: 디지몬 × 포켓몬 게임 초기화
        }

        void Update()
        {
            // TODO: 게임 로직
        }

        public void OnGameClear()
        {
            Debug.Log("[Pokemon] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            Debug.Log("[Pokemon] Game Fail!");
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
