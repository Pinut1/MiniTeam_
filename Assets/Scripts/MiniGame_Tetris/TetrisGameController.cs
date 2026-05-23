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

        [Header("컷씬 연출용 애니메이터 및 배경 패널")]
        public Animator tamamaAnim;
        public GameObject backgroundPnl;


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
            if (isCutscenePlaying) return; // 이미 재생 중이면 무시

            StartCoroutine(PerformCutscene(isHorizontal));
        }

        private IEnumerator PerformCutscene(bool isHorizontal)
        {
            isCutscenePlaying = true;

            // 0. 배경 패널 활성화 활성화
            if (backgroundPnl != null) backgroundPnl.SetActive(true);

            if (isHorizontal)
            {
                currentImpactCount++;
            }

            Debug.Log($"[Tetris] 타마마 임팩트 발동! ({currentImpactCount}/{targetImpactCount})");

            // 1. 애니메이션 트리거 실행
            if (tamamaAnim != null)
            {
                tamamaAnim.gameObject.SetActive(true);
                yield return null;
                tamamaAnim.SetTrigger("Impact");

            }

            // 2. 캐릭터 리액션 표시
            if (CharacterReactionUI.Instance != null)
            {
                if (isHorizontal)
                {
                    CharacterReactionUI.Instance.ShowReaction(ReactionType.Impact_success);
                }
                else
                {
                    CharacterReactionUI.Instance.ShowReaction(ReactionType.Impact_fail);
                }
            }

            // 3. 화면 진동 (노란 벽이 있을 경우)
            if (leftWall != null && isHorizontal)
            {
                if (leftWall.TryGetComponent(out UIShaker shaker))
                {
                    shaker.TriggerShake();
                }
            }

            // 4. 컷신 연출을 위한 대기 (예: 1초)
            yield return new WaitForSeconds(1.5f);

            // 5. 상태 복구
            if (backgroundPnl != null) backgroundPnl.SetActive(false);
            if (tamamaAnim.gameObject != null) tamamaAnim.gameObject.SetActive(false);
            
            isCutscenePlaying = false;

            // 6. 컷신 동안 미뤄졌던 새로운 블록 생성 호출
            if (SpawnTetromino.Instance != null)
            {
                SpawnTetromino.Instance.NewTetromino();
            }

            // 목표 횟수에 도달하면 게임 클리어!
            if (currentImpactCount >= targetImpactCount)
            {
                OnGameClear();
            }
        }

        public void OnGameClear()
        {
            Debug.Log("[Tetris] Game Clear!");
            MiniGameManager.Instance?.OnMiniGameClear();
        }

        /// <summary>
        /// Signals that the Tetris mini-game has failed and notifies the central mini-game manager to handle failure.
        /// </summary>
        public void OnGameFail()
        {
            
            MiniGameManager.Instance?.OnMiniGameFail();
        }
    }
}
