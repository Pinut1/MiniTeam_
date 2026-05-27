using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    public class EntryCutsceneManager : MonoBehaviour
    {
        [Serializable]
        public struct DialogueLine
        {
            public string speakerName;
            [Tooltip("Left=0, Right=1, None=-1")]
            public int speakerSide;   // 0: 왼쪽, 1: 오른쪽, -1: 화자 없음
            [TextArea(2, 5)]
            public string text;
        }

        [Header("스탠딩 이미지 (좌 / 우)")]
        public Image leftCharaImage;
        public Image rightCharaImage;
        public Sprite leftSprite;   // Blossom 등
        public Sprite rightSprite;  // Mojo Jojo 등

        [Tooltip("비화자 이미지 어둡게")]
        public float dimAlpha = 0.4f;

        [Header("대화창 UI")]
        public GameObject dialoguePanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI nextHintText;

        [Header("대사 목록")]
        public DialogueLine[] lines;

        [Header("연출")]
        public CanvasGroup rootGroup;
        public float fadeInDuration  = 0.3f;
        public float fadeOutDuration = 0.3f;

        public void Play(Action onComplete)
        {
            gameObject.SetActive(true);
            StartCoroutine(CutsceneRoutine(onComplete));
        }

        IEnumerator CutsceneRoutine(Action onComplete)
        {
            // 초기화
            if (leftCharaImage  != null) { leftCharaImage.sprite  = leftSprite;  leftCharaImage.gameObject.SetActive(leftSprite   != null); }
            if (rightCharaImage != null) { rightCharaImage.sprite = rightSprite; rightCharaImage.gameObject.SetActive(rightSprite != null); }
            SetCharaDim(-1);

            if (rootGroup != null) rootGroup.alpha = 0f;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            // 페이드 인
            yield return StartCoroutine(Fade(rootGroup, 0f, 1f, fadeInDuration));

            foreach (var line in lines)
            {
                ShowLine(line);
                yield return WaitForInput();
            }

            // 페이드 아웃
            yield return StartCoroutine(Fade(rootGroup, 1f, 0f, fadeOutDuration));

            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        void ShowLine(DialogueLine line)
        {
            if (speakerNameText != null) speakerNameText.text = line.speakerName;
            if (dialogueText    != null) dialogueText.text    = line.text;
            SetCharaDim(line.speakerSide);
        }

        void SetCharaDim(int activeSide)
        {
            if (leftCharaImage  != null) leftCharaImage.color  = activeSide == 0 ? Color.white : new Color(1,1,1, dimAlpha);
            if (rightCharaImage != null) rightCharaImage.color = activeSide == 1 ? Color.white : new Color(1,1,1, dimAlpha);
        }

        IEnumerator WaitForInput()
        {
            if (nextHintText != null) nextHintText.gameObject.SetActive(true);
            yield return null;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space));
            if (nextHintText != null) nextHintText.gameObject.SetActive(false);
        }

        IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
