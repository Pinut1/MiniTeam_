using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;

    // 커서가 보이는 상태인지 확인하는 변수
    private bool isCursorVisible = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 게임 시작 시 마우스 커서 숨기기 및 고정
        LockCursor();
    }

    void Update()
    {
        HandleCursorState();

        // 커서가 숨겨져 있을 때(Alt 키를 안 누를 때)만 시점 회전
        if (!isCursorVisible)
        {
            LookAround();
        }

        // 이동은 커서 상태와 무관하게 항상 가능하도록 유지
        Move();
    }

  
    // 마우스 커서 상태 변경 처리
    void HandleCursorState()
    {
        // 왼쪽 Alt 키를 누르는 순간
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }
        // 왼쪽 Alt 키를 떼는 순간
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            LockCursor();
        }
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    // 커서를 숨기고 중앙에 고정하는 함수
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorVisible = false;
    }

    // 커서를 보이게 하고 자유롭게 움직이도록 푸는 함수
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorVisible = true;
    }
}