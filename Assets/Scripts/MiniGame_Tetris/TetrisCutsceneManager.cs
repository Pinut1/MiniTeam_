using UnityEngine;
using System.Collections;
using System;

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
        public Animator tamamaAnim;

        [Header("타마마 임팩트 관련")]
        public GameObject tamamaBeamPrefab;
        public GameObject leftWall;

        [Header("벽 디졸브 제어")]
        public WallDissolveController wallTop;
        public WallDissolveController wallBottom;

        [Header("클리어 애니메이션 설정")]
        public string clearAnimationStateName = "Clear";
        public float clearAnimationDuration = 1.5f;

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
        /// wallTop(케로리스_UI_LeftUp_Top)의 Animator를 이용해 클리어 애니메이션을 재생합니다.
        /// </summary>
        public IEnumerator PlayClearAnimation(Action onComplete)
        {
            if (wallTop != null)
            {
                Animator anim = wallTop.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play(clearAnimationStateName);
                    Debug.Log($"[Cutscene] 케로리스_UI_LeftUp_Top 클리어 애니메이션 재생 시작 (State: {clearAnimationStateName})");
                }
                else
                {
                    Debug.LogWarning("[Cutscene] wallTop(케로리스_UI_LeftUp_Top)에 Animator가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[Cutscene] wallTop(케로리스_UI_LeftUp_Top) 오브젝트 레퍼런스가 할당되지 않았습니다.");
            }

            yield return new WaitForSeconds(clearAnimationDuration);
            onComplete?.Invoke();
        }
    }
}
