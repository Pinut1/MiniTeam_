using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float yOffset = 2f; // 위로 얼마나 띄울지

    // 맵 끝 제한
    public float minX;
    public float maxX;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position;  // 플레이어 위치

        // 카메라 화면 반 너비 계산
        float camHalfWidth =
            Camera.main.orthographicSize * Screen.width / (float)Screen.height;

        // 좌우 계산
        pos.x = Mathf.Clamp(
            pos.x,
            minX + camHalfWidth,
            maxX - camHalfWidth);

        // 위쪽 오프셋
        pos.y += yOffset;
        // 카메라 z 고정
        pos.z = -10f;

        transform.position = pos;
    }
}