using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MiniTeam.Pokemon; // 추가된 네임스페이스

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

    private void Start()
    {
        if (DialogueDB.Instance != null)
        {
            DialogueDB.Instance.Load("Hub");
        }
    }

    public void StartDialogue(string dialogueKeyPrefix, Action onComplete = null)
    {
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeSentenceRoutine(dialogueKeyPrefix, onComplete));
    }

    private IEnumerator TypeSentenceRoutine(string dialogueKeyPrefix, Action onComplete)
    {
        string countStr = DialogueDB.Instance.Get(dialogueKeyPrefix + "_count");
        if (!int.TryParse(countStr, out int sentenceCount)) sentenceCount = 0;

        for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
        {
            string prefix = $"{dialogueKeyPrefix}_";
            string text = DialogueDB.Instance.Get($"{prefix}text_{sentenceIndex}");
            string spriteName = DialogueDB.Instance.Get($"{prefix}sprite_{sentenceIndex}");
            string animTrigger = DialogueDB.Instance.Get($"{prefix}anim_{sentenceIndex}");
            string voiceName = DialogueDB.Instance.Get($"{prefix}voice_{sentenceIndex}");

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite expressionSprite = Resources.Load<Sprite>($"Sprites/Judangchi/{spriteName}");
                if (expressionSprite != null)
                {
                    DialoguePortraitController.Instance?.ChangeExpression(expressionSprite);
                }
                else
                {
                    Debug.LogWarning($"[대화 에셋 오류] 표정 이미지를 찾을 수 없습니다! 파일명: {spriteName} (위치: {dialogueKeyPrefix}의 {sentenceIndex}번째 대사)");
                }
            }

            // 이번 문장에 설정된 디지바이스 특수 연출 트리거가 있다면 즉시 실행
            if (!string.IsNullOrEmpty(animTrigger))
            {
                BottomUIController.Instance?.PlaySpecialAnimation(animTrigger);
                Debug.Log($"[대화 트리거 알림] {dialogueKeyPrefix}의 {sentenceIndex}번째 대사에서 '{animTrigger}' 애니메이션 트리거 호출 시도. (트리거가 없다면 유니티 자체 경고가 발생합니다)");
            }

            // 해당 문장 전용 보이스/효과음이 있다면 시작 시 재생
            if (!string.IsNullOrEmpty(voiceName))
            {
                AudioClip voiceClip = Resources.Load<AudioClip>($"Audio/Judangchi/{voiceName}");
                if (voiceClip != null)
                {
                    MiniTeam.Core.AudioManager.Instance?.PlayVoice(voiceClip);
                }
                else
                {
                    Debug.LogWarning($"[대화 에셋 오류] 음성 파일을 찾을 수 없습니다! 파일명: {voiceName} (위치: {dialogueKeyPrefix}의 {sentenceIndex}번째 대사)");
                }
            }

            dialogueText.text = "";
            bool skipTyping = false;

            // 한 글자씩 타이핑 효과 출력
            for (int i = 0; i < text.Length; i++)
            {
                // 글자 출력 도중 클릭 시 즉시 스킵 플래그 생성
                if (Input.GetMouseButtonDown(0))
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
            dialogueText.text = text;

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
