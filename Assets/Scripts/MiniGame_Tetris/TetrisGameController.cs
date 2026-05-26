using UnityEngine;
using MiniTeam.Core;
using System.Collections;

namespace MiniTeam.Tetris
{
    // 담당: 김기욱
    public class TetrisGameController : MonoBehaviour, IMiniGame
    {
        public static TetrisGameController Instance { get; private set; }

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
            currentImpactCount = 0;
        }

        public void OnTamamaImpactTriggered(bool isHorizontal, Vector3 spawnPos, Quaternion rot, float distance)
        {
            if (isCutscenePlaying) return; 
            isCutscenePlaying = true;

            // 컷씬 매니저에게 연출 위임
            if (TetrisCutsceneManager.Instance != null)
            {
                StartCoroutine(TetrisCutsceneManager.Instance.PlayTamamaImpact(isHorizontal, spawnPos, rot, distance, () => 
                {
                    // 컷씬 종료 후 실행될 로직 (Callback)
                    isCutscenePlaying = false;

                    if (isHorizontal)
                    {
                        currentImpactCount++;
                        Debug.Log($"[Tetris] 타마마 임팩트 성공! ({currentImpactCount}/{targetImpactCount})");
                    }

                    // 컷신 동안 미뤄졌던 새로운 블록 생성 호출
                    if (SpawnTetromino.Instance != null)
                    {
                        SpawnTetromino.Instance.NewTetromino();
                    }

                    // 목표 횟수에 도달하면 게임 클리어!
                    if (currentImpactCount >= targetImpactCount)
                    {
                        OnGameClear();
                    }
                }));
            }
            else
            {
                Debug.LogError("TetrisCutsceneManager를 찾을 수 없습니다!");
                isCutscenePlaying = false;
            }
        }

        public void OnGameClear()
        {
            Debug.Log("[Tetris] Game Clear!");
            MiniGameManager.Instance?.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            MiniGameManager.Instance?.OnMiniGameFail();
        }
    }
}
