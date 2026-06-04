using MiniTeam.Core;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageDoor : MonoBehaviour
{
    public enum OpenType { Automatic, Manual }

    [Header("Flow Control")]
    public OpenType openType = OpenType.Manual;
    public string inactiveDoorWarning = "현재 지정된 경로에 접근할 수 없습니다. 계속하려면 다른 문을 이용하십시오.";
    public string cutscenePendingWarning = "작업이 중단되었습니다. 계속 실행하려면 [주댕치]의 이야기를 입력 하십시오.";
    public string allClearWarning = "더 이상 실행할 작업이 없습니다.";
    public string alreadyClearedWarning = "해당 구역은 이미 탐색이 완료되었습니다.";

    [Header("Door Animation")]
    public Transform doorTransform;
    public float smooth = 1.0f;
    public float doorOpenAngle = -90.0f;
    public float doorCloseAngle = 0.0f;

    [Header("Audio")]
    public AudioClip openDoorSound;
    public AudioClip closeDoorSound;

    private float currentYAngle = 0f;
    private bool isOpen = false;
    private bool isPlayerInRange = false; // ���� �ü��� ��� �ִ��� ����

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

    private void HandleDoorRotation()
    {
        if (doorTransform == null) return;

        float targetAngle = isOpen ? doorOpenAngle : doorCloseAngle;
        float rotationSpeed = 150f * smooth;

        currentYAngle = Mathf.MoveTowards(currentYAngle, targetAngle, Time.deltaTime * rotationSpeed);
        doorTransform.localRotation = Quaternion.Euler(0, currentYAngle, 0);
    }

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
            ShowInactiveWarning();
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

    private bool managerIsActiveAndAutomatic()
    {
        var manager = MiniGameManager.Instance;
        return manager != null && manager.IsDoorActive(this) && openType == OpenType.Automatic;
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        HideWarning();

        AudioClip clipToPlay = isOpen ? openDoorSound : closeDoorSound;
        if (SoundManager.Instance != null && clipToPlay != null)
        {
            SoundManager.Instance.PlaySFX(clipToPlay);
        }
    }

    private void ShowWarning(string message)
    {
        HubUIManager.Instance?.ToggleWarningUI(true, message);
    }

    private void HideWarning()
    {
        HubUIManager.Instance?.ToggleWarningUI(false);
    }

    private void ShowInactiveWarning()
    {
        var manager = MiniGameManager.Instance;
        if (manager == null || manager.stageDoors == null)
        {
            ShowWarning(inactiveDoorWarning);
            return;
        }

        if (manager.currentStage >= 5)
        {
            ShowWarning(allClearWarning);
            return;
        }

        int myIndex = System.Array.IndexOf(manager.stageDoors, this);
        if (myIndex != -1 && myIndex < manager.currentStage)
        {
            ShowWarning(alreadyClearedWarning);
            return;
        }

        ShowWarning(inactiveDoorWarning);
    }
}
