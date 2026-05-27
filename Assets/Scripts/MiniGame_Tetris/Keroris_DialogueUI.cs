using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace MiniTeam.Tetris
{
    // 테트리스(케로리스) 미니게임 전용 대화 UI 제어 스크립트
    // Z, Space, Enter 키뿐만 아니라 마우스 왼쪽 클릭으로도 대사를 넘길 수 있도록 독자 지원
    public class Keroris_DialogueUI : MonoBehaviour
    {
        public static Keroris_DialogueUI Instance { get; private set; }

        [Header("UI 연결")]
        public GameObject panel;
        public TextMeshProUGUI dialogueText;
        public GameObject confirmIndicator; // ▼ 아이콘 (없어도 동작)

        [Header("캐릭터 이미지 설정")]
        public UnityEngine.UI.Image illustrationImage; // 일러스트를 표시할 UI Image 컴포넌트
        public Sprite[] dialogueSprites; // 여러 장의 캐릭터 이미지

        public bool IsShowing { get; private set; }
        private bool waitingForInput;

        void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            }
            Instance = this;
        }

        void Start()
        {
            if (!IsShowing && panel != null) panel.SetActive(false);
        }

        void Update()
        {
            if (waitingForInput &&
                (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
            {
                waitingForInput = false;
            }
        }

        public IEnumerator Show(string message)
        {
            yield return StartCoroutine(ShowInternal(message, null));
        }

        public IEnumerator Show(string message, int spriteIndex)
        {
            Sprite sprite = null;
            if (spriteIndex >= 0 && dialogueSprites != null && spriteIndex < dialogueSprites.Length)
            {
                sprite = dialogueSprites[spriteIndex];
            }
            yield return StartCoroutine(ShowInternal(message, sprite));
        }

        public IEnumerator Show(string message, Sprite customSprite)
        {
            yield return StartCoroutine(ShowInternal(message, customSprite));
        }

        private IEnumerator ShowInternal(string message, Sprite spriteToDisplay)
        {
            IsShowing = true;
            if (panel != null)
            {
                panel.transform.SetAsLastSibling(); // 블랙아웃 등 최상위 배치 대비
                panel.SetActive(true);
            }
            
            if (dialogueText != null) dialogueText.text = message;
            if (confirmIndicator != null) confirmIndicator.SetActive(false);

            // 이미지 변경 처리
            if (illustrationImage != null)
            {
                if (spriteToDisplay != null)
                {
                    illustrationImage.sprite = spriteToDisplay;
                    illustrationImage.SetNativeSize(); // 원래 크기(Native Size)에 맞춰 조절
                    illustrationImage.gameObject.SetActive(true);
                }
                else
                {
                    illustrationImage.gameObject.SetActive(false);
                }
            }

            yield return new WaitForSeconds(0.3f); // 입력 씹힘 방지

            if (confirmIndicator != null) confirmIndicator.SetActive(true);
            waitingForInput = true;
            yield return new WaitUntil(() => !waitingForInput);

            IsShowing = false;
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
            IsShowing = false;
        }
    }
}
