using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float yOffset = 2f;
    public float minX;
    public float maxX;

    public CinemachineBrain brain; // 시네머신 카메라가 활성화되면 CameraFollow는 비활성화

    [Header("시네머신 카메라 X 경계 설정")]
    [Tooltip("시네머신 카메라가 이 X 좌표보다 왼쪽으로 이동하지 않습니다.")]
    public float vcamMinX = -10f;
    [Tooltip("시네머신 카메라가 이 X 좌표보다 오른쪽으로 이동하지 않습니다.")]
    public float vcamMaxX = 30f;
    [Tooltip("시네머신 카메라 X 경계 사용 여부")]
    public bool useVcamBounds = true;

    // 경계 적용 대상 Cinemachine 카메라들
    [Tooltip("X 경계를 적용할 시네머신 카메라 목록 (비워두면 씬의 모든 CinemachineCamera에 적용)")]
    public CinemachineCamera[] targetVcams;

    private float fixedY;
    private bool isKnockbackMode = false;

    void Start()
    {
        // 시작 시 시네머신 브레인을 끄고 CameraFollow가 직접 제어
        if (brain != null) brain.enabled = false;
    }

    void LateUpdate()
    {
        // 시네머신이 활성화된 동안은 X 경계만 후처리로 클램프
        if (brain != null && brain.enabled)
        {
            if (useVcamBounds)
                ClampVcamPosition();
            return;
        }

        if (target == null) return;

        Vector3 pos = transform.position;
        float camHalfWidth = Camera.main.orthographicSize * Screen.width / (float)Screen.height;
        float targetX = target.position.x;
        pos.x = Mathf.Clamp(targetX, minX + camHalfWidth, maxX - camHalfWidth);

        if (!isKnockbackMode)
        {
            pos.y = target.position.y + yOffset;
        }
        else
        {
            pos.y = fixedY + yOffset;
        }

        pos.z = -10f;
        transform.position = pos;
    }

    // 시네머신이 계산한 카메라 위치의 X를 강제로 클램프
    private void ClampVcamPosition()
    {
        // 활성화된 Cinemachine 카메라들의 트랜스폼 X를 직접 클램프
        CinemachineCamera[] vcams = (targetVcams != null && targetVcams.Length > 0)
            ? targetVcams
            : FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var vcam in vcams)
        {
            if (vcam == null || !vcam.isActiveAndEnabled) continue;

            Vector3 vcamPos = vcam.transform.position;
            vcamPos.x = Mathf.Clamp(vcamPos.x, vcamMinX, vcamMaxX);
            vcam.transform.position = vcamPos;
        }

        // 메인 카메라(Brain) 위치도 클램프
        Vector3 camPos = transform.position;
        camPos.x = Mathf.Clamp(camPos.x, vcamMinX, vcamMaxX);
        transform.position = camPos;
    }

    public void SetKnockbackMode(bool enable)
    {
        isKnockbackMode = enable;
        if (enable && target != null)
        {
            fixedY = target.position.y;
        }
    }
}