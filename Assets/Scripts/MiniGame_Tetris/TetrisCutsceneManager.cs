using UnityEngine;
using System.Collections;
using System;
using MiniTeam.Pokemon;

namespace MiniTeam.Tetris
{
    /// <summary>
    /// 타마마 임팩트 연출에 필요한 모든 데이터를 담는 구조체
    /// </summary>
    public struct TamamaImpactData
    {
        public bool isHorizontal;
        public Vector3 spawnPos;
        public Quaternion rot;
        public float distance;
        public int hitCount;
        public Vector3 impactPoint; // 벽 타격 지점 월드 좌표
    }

    public class TetrisCutsceneManager : MonoBehaviour
    {
        public static TetrisCutsceneManager Instance { get; private set; }

        [Header("공용 연출 요소")]
        public GameObject backgroundPnl;
        public GameObject DialogueCutScenePnl;
        public Animator tamamaAnim;

        [Header("타마마 임팩트 관련")]
        public GameObject tamamaBeamPrefab;
        public GameObject leftWall;

        [Header("벽 디졸브 제어")]
        public WallDissolveController wallTop;
        public WallDissolveController wallBottom;

        [Header("벽 애니메이션 설정")]
        public Animator WallAnim;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// 구조체를 사용하여 타마마 임팩트 컷씬 시퀀스를 재생합니다.
        /// </summary>
        public IEnumerator PlayTamamaImpact(TamamaImpactData data, Action onComplete)
        {
            // 1. 컷씬 연출 시작 (배경 암전 및 애니메이션)
            if (backgroundPnl != null) backgroundPnl.SetActive(true);

            if (tamamaAnim != null)
            {
                tamamaAnim.gameObject.SetActive(true);
                yield return null;
                tamamaAnim.SetTrigger("Impact");
            }

            if (CharacterReactionUI.Instance != null)
            {
                ReactionType reaction = data.isHorizontal ? ReactionType.Impact_success : ReactionType.Impact_fail;
                CharacterReactionUI.Instance.ShowReaction(reaction);
            }

            // 2. 기 모으는 시간 (1.0초 대기)
            yield return new WaitForSeconds(1.0f);

            // 3. 빔 발사
            GameObject beamObj = null;
            if (tamamaBeamPrefab != null)
            {
                beamObj = Instantiate(tamamaBeamPrefab, data.spawnPos, data.rot);
                if (beamObj.TryGetComponent(out TamamaBeam beamScript))
                {
                    beamScript.Setup(data.distance);
                }
            }

            // 🌟 4. 바로 이 타이밍에 벽 디졸브 진행 (빔 발사 직후)
            if (data.isHorizontal)
            {
                WallAnim.SetTrigger("NextDissolve");
                if (wallTop != null)
                {
                    wallTop.SetImpactPosition(data.impactPoint);
                    wallTop.SetDissolveStage(data.hitCount);
                }
                if (wallBottom != null)
                {
                    wallBottom.SetImpactPosition(data.impactPoint);
                    wallBottom.SetDissolveStage(data.hitCount);
                }
            }

            // 5. 화면 진동
            if (leftWall != null && data.isHorizontal)
            {
                if (leftWall.TryGetComponent(out ObjectShaker shaker))
                {
                    shaker.TriggerShake(0.5f, 0.2f);
                }
            }

            // 6. 빔 유지 및 컷씬 종료 대기
            yield return new WaitForSeconds(0.5f);

            // 7. 이펙트 및 상태 복구
            if (beamObj != null) Destroy(beamObj);
            if (backgroundPnl != null) backgroundPnl.SetActive(false);
            if (tamamaAnim != null) tamamaAnim.gameObject.SetActive(false);

            // 8. 완료 콜백 호출
            onComplete?.Invoke();
        }

        /// <summary>
        /// 게임 클리어 시 벽 애니메이션을 재생하고 엔딩 대화를 출력합니다.
        /// </summary>
        public IEnumerator PlayEndingCutscene(Action onComplete)
        {
            // 1. WallAnim의 Clear 트리거 호출
            if (WallAnim != null)
            {
                WallAnim.SetTrigger("Clear");
                yield return null; // 트리거 반영을 위해 1프레임 대기
                
                // 애니메이션 완료 대기
                var stateInfo = WallAnim.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForSeconds(stateInfo.length);
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }

            // 2. DialogueDB 로드 검사
            if (DialogueDB.Instance == null)
            {
                GameObject dbObj = new GameObject("DialogueDB");
                dbObj.AddComponent<DialogueDB>();
            }
            DialogueDB.Instance.Load("Tetris");

            // 3. 캐릭터 컷신 활성화
            if (DialogueCutScenePnl != null)
            {
                DialogueCutScenePnl.SetActive(true);
            }

            // 4. 엔딩 대사 출력 (스프라이트 1번: 케로로)
            if (Keroris_DialogueUI.Instance != null)
            {
                string text = DialogueDB.Instance.Get("keroro_ed_1");
                yield return StartCoroutine(Keroris_DialogueUI.Instance.Show(text, 1));
                Keroris_DialogueUI.Instance.Close();
            }

            // 5. 캐릭터 컷신 비활성화
            if (DialogueCutScenePnl != null)
            {
                DialogueCutScenePnl.SetActive(false);
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// 게임 시작 시 타마마 캐릭터 컷신과 대화창을 띄우고 사용자의 키/마우스 입력을 대기합니다.
        /// </summary>
        public IEnumerator PlayOpeningCutscene(Action onComplete)
        {
            // 1. 에디터 개별 씬 테스트를 대비해 DialogueDB 인스턴스가 없을 시 동적 생성
            if (DialogueDB.Instance == null)
            {
                GameObject dbObj = new GameObject("DialogueDB");
                dbObj.AddComponent<DialogueDB>();
            }

            // 2. 테트리스용 대사 데이터 로드
            DialogueDB.Instance.Load("Tetris");

            // 3. 캐릭터 컷신 활성화
            if (DialogueCutScenePnl != null)
            {
                DialogueCutScenePnl.SetActive(true);
              
            } 

            // 4. 대사 순차 출력 (Z, Space, Enter 또는 마우스 클릭으로 진행, 이미지 인덱스 매핑)
            string[] dialogueKeys = { "tamama_op_1", "tamama_op_2", "tamama_op_3", "tamama_op_4", "tamama_op_5" };
           

            if (Keroris_DialogueUI.Instance != null)
            {
                for (int i = 0; i < dialogueKeys.Length; i++)
                {
                    string text = DialogueDB.Instance.Get(dialogueKeys[i]);
                    yield return StartCoroutine(Keroris_DialogueUI.Instance.Show(text, 0));
                }
                Keroris_DialogueUI.Instance.Close();
            }
            else
            {
                Debug.LogWarning("[Cutscene] Keroris_DialogueUI.Instance를 찾을 수 없습니다. 대사창 출력 없이 배경 컷신만 1.5초 노출 후 진행합니다.");
                yield return new WaitForSeconds(1.5f);
            }

            // 5. 컷신 연출 종료 및 복구
            if (DialogueCutScenePnl != null) DialogueCutScenePnl.SetActive(false);
           

            onComplete?.Invoke();
        }
    }
}
