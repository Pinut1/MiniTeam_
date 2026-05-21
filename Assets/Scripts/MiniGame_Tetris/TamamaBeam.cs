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
        // 목표 타격 지점 (왼쪽이므로 마이너스)
        float targetX = -distance - extraDistance;

        // 1. 대가리(Head): 타격 지점에 그대로 박음
        beamHead.localPosition = new Vector3(targetX, 0, 0);

        // 2. 기둥(Body) 계산
        float length = Mathf.Abs(startX - targetX);
        beamBody.localPosition = new Vector3((startX + targetX) / 2f, 0, 0);

        // ⭐️ 스케일 X에 1/7 (length / 7) 적용!
        beamBody.localScale = new Vector3(length / baseSpriteLength, 1f, 1f);

        Destroy(gameObject, destroyTime);
    }
}