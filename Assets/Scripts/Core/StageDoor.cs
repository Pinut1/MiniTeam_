using MiniTeam.Core;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageDoor : MonoBehaviour
{
    public enum OpenType { Automatic, Manual }

    [Header("Flow Control")]
    public OpenType openType = OpenType.Manual;
    public string warningMessage = "아직 들어갈 수 없는 곳이다. 다른 문에 가 보자.";

    [Header("Door Animation")]
    public Transform doorTransform;
    public float smooth = 1.0f;
    public float doorOpenAngle = -90.0f;
    public float doorCloseAngle = 0.0f;

    private float currentYAngle = 0f;

    [Header("Audio")]
    public AudioClip openDoorSound;
    public AudioClip closeDoorSound;

    private bool isOpen = false;
    private bool isPlayerInRange = false; // 플레이어가 문 앞 범위에 있는지 추적하는 변수 추가

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

    void Update()
    {
        if (doorTransform == null) return;

        // 목표 각도 설정
        float targetAngle = isOpen ? doorOpenAngle : doorCloseAngle;
        float rotationSpeed = 150f * smooth;

        // 2. 쿼터니언이 아니라, 그냥 순수 숫자(float)를 목표치까지 일정하게 더하거나 뺍니다. 
        // 무조건 0에서 -90까지 정확하게 도달합니다.
        currentYAngle = Mathf.MoveTowards(currentYAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // 3. 계산된 깔끔한 숫자를 마지막에 딱 한 번만 회전값으로 덮어씌웁니다.
        doorTransform.localRotation = Quaternion.Euler(0, currentYAngle, 0);
        if (isPlayerInRange && openType == OpenType.Manual && MiniGameManager.Instance.IsDoorActive(this))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log($"[StageDoor - {gameObject.name}] 수동문 F키 입력 감지! (ToggleDoor 호출)");
                ToggleDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true; // 플레이어 진입 체크

        if (MiniGameManager.Instance.IsDoorActive(this))
        {
            if (openType == OpenType.Automatic && !isOpen)
            {
                ToggleDoor(); // 자동문 열기
            }
        }
        else
        {
            ShowWarning();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false; // 플레이어 이탈 체크

        // (선택 사항) 자동문일 경우 플레이어가 멀어지면 다시 닫히게 만들고 싶다면 주석 해제
        /*
        if (MiniGameManager.Instance.IsDoorActive(this) && openType == OpenType.Automatic && isOpen)
        {
            ToggleDoor(); // 자동문 닫기
        }
        */
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        AudioClip clipToPlay = isOpen ? openDoorSound : closeDoorSound;

        if (SoundManager.Instance != null && clipToPlay != null)
        {
            SoundManager.Instance.PlaySFX(clipToPlay);
        }
    }

    private void ShowWarning()
    {
        Debug.Log($"Warning : {warningMessage}");
        // TODO: 나중에 UI 텍스트 띄우는 로직 여기에 연결
    }
}