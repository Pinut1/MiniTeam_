using UnityEngine;
using MiniTeam.Core;
using System.Collections;

namespace MiniTeam.Tetris
{
    // 담당: 김기욱
    public class TetrisGameController : MonoBehaviour, IMiniGame
    {
        public static TetrisGameController Instance { get; private set; }

        [Header("왼쪽 노란 벽")]
        public GameObject leftWall;

        public bool isCutscenePlaying = false;

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

        public void OnTamamaImpactTriggered(bool isHorizontal)
        {
            isCutscenePlaying = true;

            if(isHorizontal) 
            {
                currentImpactCount++;
            }
           
            Debug.Log($"[Tetris] 타마마 임팩트 발동! ({currentImpactCount}/{targetImpactCount}");

            if (leftWall != null && isHorizontal)
            {
                Debug.Log($"[1단계 통과] {leftWall.name} 오브젝트 연결 확인됨.");

                // ⭐️ GetComponent 대신 TryGetComponent를 쓰면 에러 없이 부드럽게 검사합니다.
                if (leftWall.TryGetComponent(out UIShaker shaker))
                {
                    Debug.Log("[2단계 통과] UIShaker 컴포넌트 찾음! 진동 발동!");
                    shaker.TriggerShake(0.5f, 0.2f);
                }
            }
       

            // 목표 횟수에 도달하면 게임 클리어!
            if (currentImpactCount >= targetImpactCount)
            {
                OnGameClear();
            }
        }

        private IEnumerator EndCutsceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isCutscenePlaying = false;

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
