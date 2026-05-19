using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float yOffset = 2f; // 위로 얼마나 띄울지

    // 맵 끝 제한
    public float minX;
    public float maxX;

    // ★ [추가] 넉백 시 Y축 고정을 위한 변수들
    private float fixedY;
    private bool isKnockbackMode = false;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;  // 카메라의 현재 위치에서 출발

        // 카메라 화면 반 너비 계산
        float camHalfWidth =
            Camera.main.orthographicSize * Screen.width / (float)Screen.height;

        // 좌우 계산 (넉백 중이든 아니든 X축은 항상 플레이어를 따라갑니다)
        float targetX = target.position.x;
        pos.x = Mathf.Clamp(
            targetX,
            minX + camHalfWidth,
            maxX - camHalfWidth);

        // ★ [수정] 넉백 상태에 따른 Y축 처리
        if (!isKnockbackMode)
        {
            // 일반 상태: 평소처럼 플레이어의 Y축을 따라다님
            pos.y = target.position.y + yOffset;
        }
        else
        {
            // 넉백 상태: 플레이어가 위로 떠 올라도 카메라는 기존 땅 높이에 고정됨
            pos.y = fixedY + yOffset;
        }

        // 카메라 z 고정
        pos.z = -10f;

        transform.position = pos;
    }

    // ★ [추가] 외부(PlayerLaser)에서 넉백 시작과 종료 시 호출해줄 함수
    public void SetKnockbackMode(bool enable)
    {
        isKnockbackMode = enable;
        if (enable && target != null)
        {
            // 넉백이 시작되는 순간, 플레이어가 공중으로 뜨기 전의 원래 땅 높이(Y)를 기억합니다.
            fixedY = target.position.y;
        }
    }
}