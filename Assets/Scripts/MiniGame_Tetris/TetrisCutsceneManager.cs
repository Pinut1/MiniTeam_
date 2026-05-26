using UnityEngine;
using System.Collections;
using System;

namespace MiniTeam.Tetris
{
    public class TetrisCutsceneManager : MonoBehaviour
    {
        public static TetrisCutsceneManager Instance { get; private set; }

        [Header("공용 연출 요소")]
        public GameObject backgroundPnl;
        public Animator tamamaAnim;

        [Header("타마마 임팩트 관련")]
        public GameObject tamamaBeamPrefab;
        public GameObject leftWall;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// 타마마 임팩트 컷씬 시퀀스를 재생합니다.
        /// </summary>
        public IEnumerator PlayTamamaImpact(bool isHorizontal, Vector3 spawnPos, Quaternion rot, float distance, Action onComplete)
        {
            // 1. 컷씬 연출 시작 (배경 암전 및 애니메이션)
            if (backgroundPnl != null) backgroundPnl.SetActive(true);

            if (tamamaAnim != null)
            {
                tamamaAnim.gameObject.SetActive(true);
                yield return null;
                tamamaAnim.SetTrigger("Impact");
            }

            // 캐릭터 리액션 표시
            if (CharacterReactionUI.Instance != null)
            {
                ReactionType reaction = isHorizontal ? ReactionType.Impact_success : ReactionType.Impact_fail;
                CharacterReactionUI.Instance.ShowReaction(reaction);
            }

            // 2. 컷씬 연출 대기 (기 모으는 시간)
            yield return new WaitForSeconds(1.0f);

            // 3. 빔 발사
            GameObject beamObj = null;
            if (tamamaBeamPrefab != null)
            {
                beamObj = Instantiate(tamamaBeamPrefab, spawnPos, rot);
                if (beamObj.TryGetComponent(out TamamaBeam beamScript))
                {
                    beamScript.Setup(distance);
                }
            }

            // 4. 화면 진동 (빔이 발사되는 순간 진동 시작)
            if (leftWall != null && isHorizontal)
            {
                if (leftWall.TryGetComponent(out ObjectShaker shaker))
                {
                    shaker.TriggerShake(0.5f, 0.2f);
                }
            }

            // 5. 빔 유지 및 컷씬 종료 대기
            yield return new WaitForSeconds(0.5f);

            // 6. 이펙트 및 상태 복구
            if (beamObj != null) Destroy(beamObj);
            if (backgroundPnl != null) backgroundPnl.SetActive(false);
            if (tamamaAnim != null) tamamaAnim.gameObject.SetActive(false);

            // 7. 완료 콜백 호출
            onComplete?.Invoke();
        }

        // 🌟 향후 새로운 컷씬(예: 케로로 소환 등)이 필요하면 여기에 추가하면 됩니다.
    }
}
