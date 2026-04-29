using UnityEngine;
using MiniTeam.Core;

namespace MiniTeam.Shooting1942
{
    // 1942 게임 전체 흐름 관리
    // 같은 GameObject에 WaveManager 컴포넌트도 추가할 것
    public class ShootingGameController : MonoBehaviour, IMiniGame
    {
        private WaveManager waveManager;

        void Awake()
        {
            waveManager = GetComponent<WaveManager>();
        }

        void Start()
        {
            waveManager.StartWaves();
        }

        // WaveManager → Boss 격파 시 호출
        public void OnBossDefeated()
        {
            OnGameClear();
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
