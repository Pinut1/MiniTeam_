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

        foreach (string sentence in data.sentences)
        {
            dialogueText.text = "";

            // 1. 한 글자씩 출력하는 타자기 효과
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            // 2. 글자가 다 찍히녕 유저의 클릭을 기다림
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            // 클릭이 중복 처리되지 않도록 한 프레임 대기
            yield return null;
        }

        //모든 대사 배열을 다 순회했다면 창을 끄고 콜백 실행
        dialoguePanel.SetActive(false);
        onComplete.Invoke();
    }

   
}
