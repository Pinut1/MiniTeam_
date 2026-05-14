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
        // 1. 기존의 string 대신 SentenceData 구조체를 순회합니다.
        foreach (DialogueData.SentenceData sentenceData in data.sentences)
        {
            // 2. 대사 출력을 시작하기 전에, 설정된 표정 이미지가 있다면 HubUIManager에 전달하여 표정을 바꿉니다.
            if (sentenceData.expressionSprite != null)
            {
                HubUIManager.Instance.ChangeBigJudangchiExpression(sentenceData.expressionSprite);
            }

            dialogueText.text = "";

            // 3. 한 글자씩 출력하는 타자기 효과 (구조체 안의 text 필드 사용)
            foreach (char letter in sentenceData.text.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            // 4. 글자가 다 찍히면 유저의 클릭을 기다림
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            // 클릭이 중복 처리되지 않도록 한 프레임 대기
            yield return null;
        }

        // 모든 대사 배열을 다 순회했다면 창을 끄고 콜백 실행
        dialoguePanel.SetActive(false);
        onComplete?.Invoke(); // 안정성을 위해 ? 연산자 추가
    }
}