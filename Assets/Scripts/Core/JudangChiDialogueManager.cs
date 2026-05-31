using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JudangChiDialogueManager : MonoBehaviour
{
    public static JudangChiDialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(string dialogueKeyPrefix, Action onComplete = null)
    {
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeSentenceRoutine(dialogueKeyPrefix, onComplete));
    }

    private IEnumerator TypeSentenceRoutine(string prefix, Action onComplete)
    {
        string countStr = MiniTeam.Pokemon.DialogueDB.Instance.Get($"{prefix}_count");
        if (!int.TryParse(countStr, out int count)) count = 0;

        for (int sentenceIndex = 0; sentenceIndex < count; sentenceIndex++)
        {
            string text = MiniTeam.Pokemon.DialogueDB.Instance.Get($"{prefix}_text_{sentenceIndex}");
            string animText = MiniTeam.Pokemon.DialogueDB.Instance.Get($"{prefix}_anim_{sentenceIndex}");
            string spriteName = MiniTeam.Pokemon.DialogueDB.Instance.Get($"{prefix}_sprite_{sentenceIndex}");
            string voiceName = MiniTeam.Pokemon.DialogueDB.Instance.Get($"{prefix}_voice_{sentenceIndex}");

            if (text.StartsWith("[")) text = "";
            if (animText.StartsWith("[")) animText = "";
            if (spriteName.StartsWith("[")) spriteName = "";
            if (voiceName.StartsWith("[")) voiceName = "";

            // 새 대사로 넘어갈 때 이전 대사에서 틀어둔 애니메이션을 즉시 취소합니다.
            HubUIManager.Instance.StopSpecialAnimation();

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Judangchi/{spriteName}");
                if (sprite != null)
                {
                    HubUIManager.Instance.ChangeBigJudangchiExpression(sprite);
                }
            }

            if (!string.IsNullOrEmpty(animText))
            {
                // 불 값을 파싱하여 제어하는 기능 추가
                if (animText.EndsWith("_true"))
                {
                    string boolName = animText.Replace("_true", "");
                    HubUIManager.Instance.cinemaAnimator.SetBool(boolName, true);
                }
                else if (animText.EndsWith("_false"))
                {
                    string boolName = animText.Replace("_false", "");
                    HubUIManager.Instance.cinemaAnimator.SetBool(boolName, false);
                }
                else
                {
                    HubUIManager.Instance.PlaySpecialAnimation(animText);
                }
            }

            // 해당 문장 전용 보이스/효과음이 있다면 시작 시 재생
            if (!string.IsNullOrEmpty(voiceName))
            {
                AudioClip clip = Resources.Load<AudioClip>($"Audio/Judangchi/{voiceName}");
                if (clip != null)
                {
                    MiniTeam.Core.AudioManager.Instance?.PlayVoice(clip);
                }
            }

            dialogueText.text = "";
            bool skipTyping = false;

            // 한 글자씩 타이핑 효과 출력
            for (int i = 0; i < text.Length; i++)
            {
                // 글자 출력 도중 클릭 시 즉시 스킵 플래그 생성
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    skipTyping = true;
                    break;
                }

                // Rich Text 태그 처리: < 로 시작하면 > 가 닫힐 때까지 한 번에 덧붙임
                if (text[i] == '<')
                {
                    int closeIdx = text.IndexOf('>', i);
                    if (closeIdx != -1)
                    {
                        dialogueText.text += text.Substring(i, closeIdx - i + 1);
                        i = closeIdx; // 인덱스를 닫는 괄호 위치로 건너뜀
                        continue;
                    }
                }

                dialogueText.text += text[i];

                // 타이핑 대기 시간 중에도 마우스 클릭 입력 감지
                float elapsed = 0f;
                while (elapsed < typingSpeed)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                    {
                        skipTyping = true;
                        break;
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (skipTyping) break;
            }

            // 텍스트를 끝까지 출력
            dialogueText.text = text;

            // 스킵 당시의 마우스 클릭이 다음 대사 넘어가기로 즉시 인식되지 않도록 한 프레임 대기
            yield return null;

            // 마우스 클릭 시 다음 대사로 진행
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
            yield return null;
        }

        // 모든 대사 배열을 다 순회했다면 창을 끄고 콜백 실행
        dialoguePanel.SetActive(false);
        onComplete?.Invoke(); // 안정성을 위해 ? 연산자 추가
    }
}
