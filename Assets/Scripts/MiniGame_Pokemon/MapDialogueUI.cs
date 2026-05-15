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
            IsShowing = true;
            if (panel != null) panel.SetActive(true);
            if (dialogueText != null) dialogueText.text = message;
            if (confirmIndicator != null) confirmIndicator.SetActive(false);

            yield return new WaitForSeconds(0.3f); // 입력 씹힘 방지

            if (confirmIndicator != null) confirmIndicator.SetActive(true);
            waitingForInput = true;
            yield return new WaitUntil(() => !waitingForInput);

            if (panel != null) panel.SetActive(false);
            IsShowing = false;
        }
    }
}
