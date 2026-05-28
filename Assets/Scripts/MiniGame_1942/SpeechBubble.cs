using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    // 가시성은 CanvasGroup.alpha 로만 제어 (SetActive 사용 금지 —
    // EntryCutsceneManager 와 같은 오브젝트에 붙어 코루틴이 끊기는 것 방지)
    public class SpeechBubble : MonoBehaviour
    {
        public CanvasGroup     group;
        public Image           normalImage;        // 꼬리 아래 (PPG용)
        public Image           mojoImage;          // 꼬리 위 (모조조조용)
        public TextMeshProUGUI speakerLabel;
        public TextMeshProUGUI dialogueLabel;
        public TextMeshProUGUI mojoSpeakerLabel;
        public TextMeshProUGUI mojoDialogueLabel;

        void SetBubble(bool isMojo)
        {
            if (normalImage != null) normalImage.gameObject.SetActive(!isMojo);
            if (mojoImage   != null) mojoImage.gameObject.SetActive(isMojo);
        }

        public IEnumerator FadeIn(string speaker, string text, bool isMojo, float dur = 0.15f)
        {
            if (isMojo)
            {
                if (mojoSpeakerLabel  != null) mojoSpeakerLabel.text  = speaker;
                if (mojoDialogueLabel != null) mojoDialogueLabel.text = text;
            }
            else
            {
                if (speakerLabel  != null) speakerLabel.text  = speaker;
                if (dialogueLabel != null) dialogueLabel.text = text;
            }
            SetBubble(isMojo);

            group.blocksRaycasts = true;
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            { group.alpha = t / dur; yield return null; }
            group.alpha = 1f;
        }

        public IEnumerator FadeOut(float dur = 0.15f)
        {
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            { group.alpha = 1f - t / dur; yield return null; }
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }
}
