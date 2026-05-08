using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Tetris
{
    // 담당: 김기욱
    public class TetrisGameController : MonoBehaviour, IMiniGame
    {
        void Start()
        {
            // TODO: 테트리스 게임 초기화
        }

        void Update()
        {
            // TODO: 블록 낙하, 입력 처리
        }

        public void OnGameClear()
        {
            Debug.Log("[Tetris] Game Clear!");
            MiniGameManager.Instance.OnMiniGameClear();
        }

        /// <summary>
        /// Signals that the Tetris mini-game has failed and notifies the central mini-game manager to handle failure.
        /// </summary>
        public void OnGameFail()
        {
            
            MiniGameManager.Instance.OnMiniGameFail();
        }
    }
}
