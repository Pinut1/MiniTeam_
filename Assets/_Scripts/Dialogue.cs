using MiniTeam.Pokemon;
using System;
using System.Collections;
using UnityEngine;

public class DialogueService : MonoBehaviour
{
    public static DialogueService Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 대사를 시작하는 범용 함수
    /// </summary>
    /// <param name="fileName">Resources/Dialogues/ 에 있는 파일명</param>
    /// <param name="dialogueKey">출력할 대사의 키</param>
    /// <param name="onComplete">대사가 다 끝난 후 실행할 동작</param>
    public void StartDialogue(string fileName, string dialogueKey, Action onComplete = null)
    {
        StartCoroutine(RunDialogueRoutine(fileName, dialogueKey, onComplete));
    }

    private IEnumerator RunDialogueRoutine(string fileName, string dialogueKey, Action onComplete)
    {
        // 1. JSON 로드
        DialogueDB.Instance.Load(fileName);

        // 2. 대사 가져오기
        string text = DialogueDB.Instance.Get(dialogueKey);

        // 3. UI에 표시 (기존 MapDialogueUI 활용)
        yield return StartCoroutine(MapDialogueUI.Instance.Show(text));

        // 4. 대화 종료 후 콜백 실행
        onComplete?.Invoke();
    }
}