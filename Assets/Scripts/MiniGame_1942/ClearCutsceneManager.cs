using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    // 1942 클리어 컷씬: 플레이어 ↔ 파워퍼프걸 대화 → 오브젝트 획득 → 허브 복귀
    public class ClearCutsceneManager : MonoBehaviour
    {
        [Serializable]
        public struct DialogueLine
        {
            public string speakerName;
            public Sprite speakerSprite;
            [TextArea(2, 5)]
            public string text;
        }

        [Header("대화 UI")]
        public GameObject dialoguePanel;
        public Image speakerImage;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI nextHintText; // "▶ 계속하려면 Enter"

        [Header("아이템 획득 UI")]
        public GameObject itemPanel;
        public Image itemImage;
        public TextMeshProUGUI itemNameText;

        [Header("대사 목록 (Inspector에서 편집)")]
        public DialogueLine[] lines;

        [Header("아이템 정보")]
        public Sprite itemSprite;
        public string itemName = "파워퍼프걸의 증표";

        private Action onComplete;

        public void Play(Action onComplete)
        {
            this.onComplete = onComplete;
            StartCoroutine(CutsceneRoutine());
        }

        IEnumerator CutsceneRoutine()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (itemPanel     != null) itemPanel.SetActive(false);

            foreach (var line in lines)
            {
                ShowLine(line);
                yield return WaitForInput();
            }

            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            // 아이템 획득 연출
            yield return ShowItemGet();

            onComplete?.Invoke();
        }

        void ShowLine(DialogueLine line)
        {
            if (speakerImage    != null) speakerImage.sprite = line.speakerSprite;
            if (speakerImage    != null) speakerImage.gameObject.SetActive(line.speakerSprite != null);
            if (speakerNameText != null) speakerNameText.text = line.speakerName;
            if (dialogueText    != null) dialogueText.text    = line.text;
        }

        IEnumerator WaitForInput()
        {
            if (nextHintText != null) nextHintText.gameObject.SetActive(true);
            yield return null; // 같은 프레임 입력 무시
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space));
            if (nextHintText != null) nextHintText.gameObject.SetActive(false);
        }

        IEnumerator ShowItemGet()
        {
            if (itemPanel == null) yield break;

            if (itemImage    != null) itemImage.sprite = itemSprite;
            if (itemNameText != null) itemNameText.text = itemName;
            itemPanel.SetActive(true);

            yield return new WaitForSeconds(2.5f);
            itemPanel.SetActive(false);
        }
    }
}
