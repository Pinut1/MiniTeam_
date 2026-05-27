using UnityEngine;
using MiniTeam.Core;
using System.Collections;

namespace MiniTeam.Tetris
{
    
    public class TetrisGameController : MonoBehaviour, IMiniGame
    {
        public static TetrisGameController Instance { get; private set; }

        public bool isCutscenePlaying = true; // 오프닝 컷씬 재생을 위해 true로 시작

        [Header("클리어 조건 세팅")]
        public int targetImpactCount = 4;
        private int currentImpactCount = 0;

        [Header("사운드 세팅")]
        public AudioClip bgmClip;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);
            currentImpactCount = 0;

          

            // 오프닝 컷씬 자동 시작
            if (TetrisCutsceneManager.Instance != null)
            {
                isCutscenePlaying = true;
                StartCoroutine(TetrisCutsceneManager.Instance.PlayOpeningCutscene(() => 
                {
                    isCutscenePlaying = false;
                    Debug.Log("[Tetris] 오프닝 컷씬 완료. 첫 테트로미노 스폰!");
                    if (SpawnTetromino.Instance != null)
                        SpawnTetromino.Instance.NewTetromino();

                    // BGM 재생 연동
                    if (bgmClip != null && SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayBGM(bgmClip);
                    }
                }));
            }
            else
            {
                isCutscenePlaying = false;
                if (SpawnTetromino.Instance != null)
                    SpawnTetromino.Instance.NewTetromino();
            }
        }

        public void OnTamamaImpactTriggered(bool isHorizontal, Vector3 spawnPos, Quaternion rot, float distance, bool isDebug = false)
        {
            if (isCutscenePlaying) return; 
            isCutscenePlaying = true;

            // 1. 타격 지점 및 유효 범위 체크 (Y: 7.0 ~ 9.0)
            bool isValidRange = spawnPos.y >= 7.0f && spawnPos.y <= 9.0f;

            // 시각적 디졸브 좌표는 Y: 8.0으로 고정
            Vector3 impactPoint = new Vector3(-2.3f, 8.0f, 0f);

            if (isHorizontal && isValidRange) 
            {
                currentImpactCount++;
            }

            // 2. 연출 데이터 패키징 (구조체 활용)
            TamamaImpactData impactData = new TamamaImpactData
            {
                isHorizontal = isHorizontal && isValidRange, // 범위 밖이면 실패 연출을 하도록 설정 가능
                spawnPos = spawnPos,
                rot = rot,
                distance = distance,
                hitCount = currentImpactCount,
                impactPoint = impactPoint
            };

            // 4. 컷신 매니저에게 연출 위임
            if (TetrisCutsceneManager.Instance != null)
            {
                StartCoroutine(TetrisCutsceneManager.Instance.PlayTamamaImpact(impactData, () => 
                {
                    // 연출 종료 후 콜백
                    isCutscenePlaying = false;

                    if (isHorizontal)
                        Debug.Log($"[Tetris] 타마마 임팩트 완료! ({currentImpactCount}/{targetImpactCount})");

                    if (currentImpactCount >= targetImpactCount)
                    {
                        // 클리어 횟수 도달 시 클리어 애니메이션 재생 후 게임 클리어로 진행
                        isCutscenePlaying = true; // 연출 동안 다른 입력 차단
                        StartCoroutine(TetrisCutsceneManager.Instance.PlayEndingCutscene(() => 
                        {
                            isCutscenePlaying = false;
                            OnGameClear();
                        }));
                    }
                    else
                    {
                        // 디버그 모드가 아닐 때만 새로운 테트로미노를 스폰
                        if (!isDebug && SpawnTetromino.Instance != null)
                            SpawnTetromino.Instance.NewTetromino();
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
            CleanupRemainingBlocks();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopBGM();
            }
            MiniGameManager.Instance?.OnMiniGameClear();
        }

        public void OnGameFail()
        {
            CleanupRemainingBlocks();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopBGM();
            }
            MiniGameManager.Instance?.OnMiniGameFail();
        }

        private void CleanupRemainingBlocks()
        {
            // 1. Clear static grid and destroy block tiles in the grid
            TetrisBlock.ClearGrid();

            // 2. Clear hold/next dummies in SpawnTetromino
            if (SpawnTetromino.Instance != null)
            {
                SpawnTetromino.Instance.ClearAllDummies();
            }

            // 3. Destroy all other TetrisBlock objects (active/inactive)
            TetrisBlock[] blocks = FindObjectsByType<TetrisBlock>(FindObjectsSortMode.None);
            foreach (var block in blocks)
            {
                if (block != null) Destroy(block.gameObject);
            }
        }
    }
}
