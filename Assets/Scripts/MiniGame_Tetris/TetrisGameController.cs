using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Tetris
{
    // 담당: 김기욱
    public class TetrisGameController : MonoBehaviour, IMiniGame
    {
        public static TetrisGameController Instance { get; private set; }

        [Header("클리어 조건 세팅")]
        public int targetImpactCount = 4;
        private int currentImpactCount = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // TODO
            currentImpactCount = 0;
        }

        void Update()
        {
            // TODO: 블록 낙하, 입력 처리
        }

        public void OnTamamaImpactTriggered()
        {
            currentImpactCount++;
            Debug.Log($"[Tetris] 타마마 임팩트 발동! ({currentImpactCount}/{targetImpactCount}");

            // 목표 횟수에 도달하면 게임 클리어!
            if (currentImpactCount >= targetImpactCount)
            {
                OnGameClear();
            }
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
