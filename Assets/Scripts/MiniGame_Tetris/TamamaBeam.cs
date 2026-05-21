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
        // ⭐️ 1. '월드 좌표(World Position)' 기준으로 절대 위치 계산
        // 부모(생성된 위치)의 월드 X값에서 계산하므로 트랜스폼 꼬임 완벽 차단
        float startWorldX = transform.position.x + startX;
        float targetWorldX = transform.position.x - distance - extraDistance;

        // ⭐️ 2. 대가리(Head): 로컬 무시하고 타격점 '월드 좌표'에 다이렉트로 꽂음
        beamHead.position = new Vector3(targetWorldX + 3f, transform.position.y, transform.position.z);
        beamHead.localScale = new Vector3(0.9f, 0.9f, 0.9f);

        // ⭐️ 3. 기둥(Body) 계산: 시작점과 타격점의 월드 좌표 중간값
        beamBody.position = new Vector3((startWorldX + targetWorldX) / 2f, transform.position.y, transform.position.z);

        // 4. 길이(스케일) 계산: 월드 좌표계 상의 진짜 절대 거리 계산
        float length = Mathf.Abs(startWorldX - targetWorldX);

        // (여기에 팀장님이 원하시던 + 2f 같은 길이 보정을 하시면 이제 정확히 먹힙니다!)
        // 예: float length = Mathf.Abs(startWorldX - targetWorldX) + 2f;

        // 스케일은 기둥 자신의 크기를 늘리는 것이므로 로컬 스케일 유지
        beamBody.localScale = new Vector3(length / baseSpriteLength, 1f, 1f);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(destroyTime, 0.2f);
        }

        Destroy(gameObject, destroyTime);
    }
}