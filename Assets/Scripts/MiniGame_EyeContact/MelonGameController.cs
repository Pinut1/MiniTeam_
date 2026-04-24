using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Melon
{
    // 담당: 김예지
    public class MelonGameController : MonoBehaviour, IMiniGame
    {
        void Start()
        {
            // TODO: 눈빛 보내기 게임 초기화
        }

        void Update()
        {
            // TODO: 게임 로직
        }

        public void OnGameClear()
        {
            Debug.Log("[Melon] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            Debug.Log("[Melon] Game Fail!");
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
