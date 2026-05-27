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

    [Header("Audio")]
    [Tooltip("타이핑 시 재생될 기본 효과음 (동물의 숲 텍스트 소리 등)")]
    [SerializeField] private AudioClip defaultTypingSfx;
    [Tooltip("몇 글자마다 타이핑 소리를 낼지 결정 (기본: 2)")]
    [SerializeField] private int typingSfxFrequency = 2;


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
                MiniTeam.Core.SoundManager.Instance?.PlaySFX(sentenceData.voiceClip);
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

                dialogueText.text += sentenceData.text[i];

                // 공백이 아닌 글자를 출력할 때 타건음 재생
                if (defaultTypingSfx != null && sentenceData.text[i] != ' ' && (i % typingSfxFrequency == 0))
                {
                    MiniTeam.Core.SoundManager.Instance?.PlaySFX(defaultTypingSfx, 0.4f); // 텍스트 소리는 약간 작게
                }

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