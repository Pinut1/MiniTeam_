using System.Collections;
using UnityEngine;
using TMPro;

namespace MiniTeam.Pokemon
{
    public class MapDialogueUI : MonoBehaviour
    {
        public static MapDialogueUI Instance { get; private set; }

        [Header("UI 연결")]
        public GameObject panel;
        public TextMeshProUGUI dialogueText;
        public GameObject confirmIndicator; // ▼ 아이콘 (없어도 동작)

        public bool IsShowing { get; private set; }
        private bool waitingForInput;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (panel != null) panel.SetActive(false);
        }

        void Update()
        {
            if (waitingForInput &&
                (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                waitingForInput = false;
            }
        }

        public IEnumerator Show(string message)
        {
            Debug.Log($"[MapDialogueUI] Show 시작: {message}");
            IsShowing = true;
            if (panel != null)
            {
                panel.transform.SetAsLastSibling(); // 블랙아웃 캔버스보다 위로
                panel.SetActive(true);
                Debug.Log("[MapDialogueUI] panel 활성화 됨");
            }
            else
            {
                Debug.LogWarning("[MapDialogueUI] panel이 NULL입니다! 화면에 UI가 보이지 않습니다.");
            }

            if (dialogueText != null) 
            {
                dialogueText.text = message;
            }
            else
            {
                Debug.LogWarning("[MapDialogueUI] dialogueText가 NULL입니다! 대사가 표시되지 않습니다.");
            }

            if (confirmIndicator != null) confirmIndicator.SetActive(false);

            Debug.Log("[MapDialogueUI] 0.3초 대기 시작");
            yield return new WaitForSeconds(0.3f); // 입력 씹힘 방지
            Debug.Log("[MapDialogueUI] 0.3초 대기 끝. 키 입력 대기 시작");

            if (confirmIndicator != null) confirmIndicator.SetActive(true);
            waitingForInput = true;
            
            yield return new WaitUntil(() => !waitingForInput);
            
            Debug.Log("[MapDialogueUI] 키 입력 완료! Show 코루틴 종료");

            if (panel != null) panel.SetActive(false);
            IsShowing = false;
        }
    }
}
