using UnityEngine;

public class TamamaBeam : MonoBehaviour
{
    public Transform beamBody;
    public Transform beamHead;

    public float destroyTime = 0.5f;

    [Header("직관적 좌표 세팅")]
    public float startX = -0.5f;       // 빔 시작점 (블록 왼쪽 끝)
    public float extraDistance = 1.5f; // 벽 타격 보정용 추가 거리

    [Header("스프라이트 보정")]
    public float baseSpriteLength = 7f; // 원본 기둥 이미지의 실제 길이 (7칸)

    public void Setup(float distance)
    {

        // ⭐️ 1. 시작점 보정 (Local 기준 이동)
        // 월드 좌표를 무시하고 '자신이 현재 바라보는 방향' 기준으로 startX만큼 이동시킵니다.
        // 즉, 왼쪽을 보면 왼쪽으로, 위를 보면 위로 알아서 시작점을 당겨줍니다.
        transform.Translate(new Vector3(startX, 0, 0), Space.Self);

        // ⭐️ 2. 타격 목표점 (부모 기준 로컬 좌표)
        // 빔은 항상 부모의 왼쪽(-X)으로 뻗어나가도록 만들어졌으므로 마이너스 값 적용
        float targetLocalX = -distance - extraDistance;

        // ⭐️ 3. 대가리(Head) 세팅
        if (beamHead != null)
        {
            // 월드 좌표 싹 다 버리고, 부모(프리팹 본체)를 기준으로 한 'localPosition'에 박습니다.
            // 팀장님이 원하신 + 3f (땡겨오기) 완벽 적용!
            beamHead.localPosition = new Vector3(targetLocalX + 3f, 0, 0);

            // 2D 이미지의 경우 Z스케일은 1로 두는 것이 렌더링에 안전합니다.
            beamHead.localScale = new Vector3(0.9f, 0.9f, 1f);
        }

        // ⭐️ 4. 기둥(Body) 세팅
        if (beamBody != null)
        {
            // 기둥 중심은 시작점(0)과 타격점의 정확히 절반 위치
            beamBody.localPosition = new Vector3(targetLocalX / 2f, 0, 0);

            // 길이(스케일) 계산: 타격점까지의 절대값 거리
            float length = Mathf.Abs(targetLocalX);
            beamBody.localScale = new Vector3(length / baseSpriteLength, 1f, 1f);
        }

        // 5. 팀장님이 추가하신 진동 이펙트
        if (TryGetComponent(out ObjectShaker shaker))
        {
            shaker.TriggerShake(destroyTime, 0.15f); // 0.3초 -> destroyTime으로 일치시키신 센스 굿입니다!
        }

        Destroy(gameObject, destroyTime);
    }
}