using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float yOffset = 2f;
    public float minX;
    public float maxX;

    public CinemachineBrain brain; // 메인 카메라의 시네마머신 브레인

    private float fixedY;
    private bool isKnockbackMode = false;

    void Start()
    {
        // 시작 시 시네마머신을 꺼서 수동 제어를 우선시함
        if (brain != null) brain.enabled = false;
    }

    void LateUpdate()
    {
        // 컷신 진행 중(브레인이 켜진 상태)이면 수동 제어 중지
        if (brain != null && brain.enabled) return;

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

    public void SetKnockbackMode(bool enable)
    {
        isKnockbackMode = enable;
        if (enable && target != null)
        {
            fixedY = target.position.y;
        }
    }
}