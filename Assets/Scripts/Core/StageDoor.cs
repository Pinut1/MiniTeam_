using MiniTeam.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageDoor : MonoBehaviour
{
    public enum OpenType { Automatic, Manual }

    [Header("Flow Control")]
    [Tooltip("문의 개폐 방식 설정 (자동 / 수동)")]
    public OpenType openType = OpenType.Manual;
    
    [Tooltip("아직 비활성화된 스테이지 문에 접근했을 때의 경고 메시지")]
    public string inactiveDoorWarning = "아직 들어갈 수 없는 곳이다. 다른 문에 가 보자.";
    
    [Tooltip("활성화된 문이지만 주댕치와의 대화 전일 때의 경고 메시지")]
    public string cutscenePendingWarning = "주댕치의 이야기를\n 들어봐야 할 것 같다";

    [Header("Door Animation")]
    [Tooltip("회전시킬 실제 문 트랜스폼")]
    public Transform doorTransform;
    [Tooltip("문이 열리는 속도 조절 배율")]
    public float smooth = 1.0f;
    [Tooltip("문이 열렸을 때의 목표 로컬 Y 각도")]
    public float doorOpenAngle = -90.0f;
    [Tooltip("문이 닫혔을 때의 목표 로컬 Y 각도")]
    public float doorCloseAngle = 0.0f;

    [Header("Audio")]
    public AudioClip openDoorSound;
    public AudioClip closeDoorSound;

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
        if (!isPlayerInRange || openType != OpenType.Manual) return;
        
        var manager = MiniGameManager.Instance;
        if (manager == null || !manager.IsDoorActive(this)) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (manager.isCutscenePlayed)
            {
                Debug.Log($"[StageDoor - {gameObject.name}] F키 입력으로 문 토글");
                ToggleDoor();
            }
            else
            {
                ShowWarning(cutscenePendingWarning);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;

        var manager = MiniGameManager.Instance;
        if (manager == null) return;

        if (manager.IsDoorActive(this))
        {
            if (manager.isCutscenePlayed)
            {
                if (openType == OpenType.Automatic && !isOpen)
                {
                    ToggleDoor(); // 자동문 열기
                }
            }
            else
            {
                ShowWarning(cutscenePendingWarning);
            }
        }
        else
        {
            ShowWarning(inactiveDoorWarning);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;
        HideWarning();

        // 자동문일 경우 플레이어가 영역을 벗어나면 다시 문을 닫습니다.
        if (managerIsActiveAndAutomatic())
        {
            if (isOpen)
            {
                ToggleDoor(); // 자동문 닫기
            }
        }
    }

    /// <summary>
    /// 자동문 개폐 조치 가능 여부를 검사합니다.
    /// </summary>
    private bool managerIsActiveAndAutomatic()
    {
        var manager = MiniGameManager.Instance;
        return manager != null && manager.IsDoorActive(this) && openType == OpenType.Automatic;
    }

    /// <summary>
    /// 문의 개폐 상태를 토글하고 그에 따른 효과음을 출력합니다.
    /// </summary>
    private void ToggleDoor()
    {
        isOpen = !isOpen;

        // 문이 개폐되면 기존에 켜져 있던 경고창은 시야 확보를 위해 꺼줍니다.
        HideWarning();

        AudioClip clipToPlay = isOpen ? openDoorSound : closeDoorSound;
        if (SoundManager.Instance != null && clipToPlay != null)
        {
            SoundManager.Instance.PlaySFX(clipToPlay);
        }
    }

    /// <summary>
    /// 경고 UI를 메시지와 함께 활성화합니다.
    /// </summary>
    private void ShowWarning(string message)
    {
        Debug.Log($"[StageDoor Warning] {message}");
        HubUIManager.Instance?.ToggleWarningUI(true, message);
    }

    /// <summary>
    /// 경고 UI를 비활성화합니다.
    /// </summary>
    private void HideWarning()
    {
        HubUIManager.Instance?.ToggleWarningUI(false);
    }
}