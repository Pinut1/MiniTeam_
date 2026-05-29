using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JudangChiDialogueManager : MonoBehaviour
{
    public static JudangChiDialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeSentenceRoutine(data, onComplete));
    }

    private IEnumerator TypeSentenceRoutine(DialogueData data, Action onComplete)
    {
        foreach (DialogueData.SentenceData sentenceData in data.sentences)
        {
            if (sentenceData.expressionSprite != null)
            {
                HubUIManager.Instance.ChangeBigJudangchiExpression(sentenceData.expressionSprite);
            }

            // 이번 문장에 설정된 디지바이스 특수 연출 트리거가 있다면 즉시 실행
            if (!string.IsNullOrEmpty(sentenceData.animationTriggerName))
            {
                HubUIManager.Instance.PlaySpecialAnimation(sentenceData.animationTriggerName);
            }

            // 해당 문장 전용 보이스/효과음이 있다면 시작 시 재생
            if (sentenceData.voiceClip != null)
            {
                MiniTeam.Core.AudioManager.Instance?.PlayVoice(sentenceData.voiceClip);
            }

            dialogueText.text = "";
            bool skipTyping = false;

            // 한 글자씩 타이핑 효과 출력
            for (int i = 0; i < sentenceData.text.Length; i++)
            {
                // 글자 출력 도중 클릭 시 즉시 스킵 플래그 생성
                if (Input.GetMouseButtonDown(0))
                {
                    skipTyping = true;
                    break;
                }

                // Rich Text 태그 처리: < 로 시작하면 > 가 닫힐 때까지 한 번에 덧붙임
                if (sentenceData.text[i] == '<')
                {
                    int closeIdx = sentenceData.text.IndexOf('>', i);
                    if (closeIdx != -1)
                    {
                        dialogueText.text += sentenceData.text.Substring(i, closeIdx - i + 1);
                        i = closeIdx; // 인덱스를 닫는 괄호 위치로 건너뜀
                        continue;
                    }
                }

                dialogueText.text += sentenceData.text[i];

                // 타이핑 대기 시간 중에도 마우스 클릭 입력 감지지
                float elapsed = 0f;
                while (elapsed < typingSpeed)
                {
                    if (Input.GetMouseButtonDown(0))
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
            dialogueText.text = sentenceData.text;

            // 스킵 당시의 마우스 클릭이 다음 대사 넘어가기로 즉시 인식되지 않도록 한 프레임 대기
            yield return null;

            // 마우스 클릭 시 다음 대사로 진행
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield return null;
        }

        // 모든 대사 배열을 다 순회했다면 창을 끄고 콜백 실행
        dialoguePanel.SetActive(false);
        onComplete?.Invoke(); // 안정성을 위해 ? 연산자 추가
    }
}