using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    public class ClearCutsceneManager : MonoBehaviour
    {
        [Header("대화")]
        public EntryCutsceneManager dialogue;  // SpeechBubble 시스템 재사용

        [Header("아이템 획득 UI")]
        public GameObject itemPanel;
        public Image itemImage;
        public TextMeshProUGUI itemNameText;

        [Header("아이템 정보")]
        public Sprite itemSprite;
        public string itemName = "파워퍼프걸의 증표";

        // BossController에서 폭발 전에 호출 — 대화만 재생
        public void PlayDialogue(Action onComplete)
        {
            StartCoroutine(DialogueRoutine(onComplete));
        }

        IEnumerator DialogueRoutine(Action onComplete)
        {
            if (dialogue != null)
            {
                bool done = false;
                dialogue.Play("clear_", false, () => done = true);
                yield return new WaitUntil(() => done);
            }
            onComplete?.Invoke();
        }

        // ShootingGameController에서 폭발 후 호출 — 아이템 획득 연출만
        public void Play(Action onComplete)
        {
            StartCoroutine(CutsceneRoutine(onComplete));
        }

        IEnumerator CutsceneRoutine(Action onComplete)
        {
            yield return StartCoroutine(ShowItemGet());
            onComplete?.Invoke();
        }

        IEnumerator ShowItemGet()
        {
            if (itemPanel == null) yield break;

            if (itemImage    != null) itemImage.sprite  = itemSprite;
            if (itemNameText != null) itemNameText.text = itemName;

            PlayerPrefs.SetInt("1942_Cleared", 1);
            PlayerPrefs.Save();

            var cg = itemPanel.GetComponent<CanvasGroup>();
            itemPanel.SetActive(true);

            // 페이드인
            if (cg != null)
            {
                cg.alpha = 0f;
                for (float t = 0; t < 0.4f; t += Time.deltaTime)
                { cg.alpha = t / 0.4f; yield return null; }
                cg.alpha = 1f;
            }

            yield return new WaitForSeconds(2.5f);

            // 페이드아웃
            if (cg != null)
            {
                for (float t = 0; t < 0.4f; t += Time.deltaTime)
                { cg.alpha = 1f - t / 0.4f; yield return null; }
                cg.alpha = 0f;
            }

            itemPanel.SetActive(false);
        }
    }
}
