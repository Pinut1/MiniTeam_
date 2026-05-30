using MiniTeam.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageDoor : MonoBehaviour
{
    [Header("Door Animation")]
    [Tooltip("회전시킬 실제 문 트랜스폼")]
    public Transform doorTransform;
    [Tooltip("문이 열리는 속도 조절 배율")]
    public float smooth = 1.0f;
    [Tooltip("문이 열렸을 때의 목표 로컬 Y 각도")]
    public float doorOpenAngle = -90.0f;
    [Tooltip("문이 닫혔을 때의 목표 로컬 Y 각도")]
    public float doorCloseAngle = 0.0f;

    public bool IsOpen => isOpen;

    private float currentYAngle = 0f;
    private bool isOpen = false;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        if (doorTransform == null && transform.childCount > 0)
        {
            doorTransform = transform.GetChild(0);
        }
    }

    private void OnEnable()
    {
        isOpen = false;
        isPlayerInRange = false;
    }

    private void Update()
    {
        HandleDoorRotation();
        HandleManualInput();
    }

    /// <summary>
    /// 목표 각도로 부드럽게 문 회전을 업데이트합니다.
    /// </summary>
    private void HandleDoorRotation()
    {
        if (doorTransform == null) return;

        float targetAngle = isOpen ? doorOpenAngle : doorCloseAngle;
        float rotationSpeed = 150f * smooth;

        currentYAngle = Mathf.MoveTowards(currentYAngle, targetAngle, Time.deltaTime * rotationSpeed);
        doorTransform.localRotation = Quaternion.Euler(0, currentYAngle, 0);
    }

    /// <summary>
    /// 수동 모드일 때 플레이어의 F키 개폐 입력을 처리합니다.
    /// </summary>
    private void HandleManualInput()
    {
        if (!isPlayerInRange) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            DoorManager.Instance?.TryInteractWithDoor(this, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;
        
        DoorManager.Instance?.TryInteractWithDoor(this, false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;
        DoorManager.Instance?.HideWarning();
    }

    /// <summary>
    /// 문의 개폐 상태를 토글하고 그에 따른 효과음을 출력합니다.
    /// </summary>
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // 문이 개폐되면 기존에 켜져 있던 경고창은 시야 확보를 위해 꺼줍니다.
        DoorManager.Instance?.HideWarning();

        if (AudioManager.Instance != null)
        {
            if (isOpen)
                AudioManager.Instance.PlayDoorOpen();
            else
                AudioManager.Instance.PlayDoorClose();
        }
    }
}