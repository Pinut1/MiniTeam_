using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[AddComponentMenu("")]
public class CinemachineMinMaxClamp : CinemachineExtension
{
    // 플레이어가 백그라운드 끝쪽에 있을때 시네머신이 작동하면 백그라운드의 뒷배경을 보여주는거 방지 하기위한 시네머신 한계 설정
    [Header("카메라 X축 이동 한계점")]
    [Tooltip("카메라가 이동할 수 있는 최소 X 좌표 (왼쪽 끝)")]
    public float minX = -10f;

    [Tooltip("카메라가 이동할 수 있는 최대 X 좌표 (오른쪽 끝)")]
    public float maxX = 10f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // Body(카메라 이동) 처리가 끝난 직후에 X축 위치만 강제 고정합니다.
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 clampedPos = state.RawPosition;

            // Y, Z값은 시네머신이 계산한 그대로 두고, X값만 제한을 겁니다.
            clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);

            state.RawPosition = clampedPos;
        }
    }
}