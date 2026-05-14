using MiniTeam.Core;
using System;
using System.Collections;
using UnityEngine;

public class JudangChiController : MonoBehaviour
{
    public static JudangChiController Instance { get; private set; }

    [Header("대사 데이터 목록")]
    [Tooltip("인덱스 0: 게임 시작시, 1: 1스테이지 클리어 후...")]
    public DialogueData[] stageDialogues;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }


    public void PlaySequenceForCurrentStage()
    {
        int currentStage = MiniGameManager.Instance.currentStage;

        if (currentStage >= stageDialogues.Length) return;

        StartCoroutine(SequenceRoutine(stageDialogues[currentStage]));

    }

    private IEnumerator SequenceRoutine(DialogueData dialogueData)
    {
        MiniGameManager.Instance.DisablePlayerInput();

        // 1. 작은 주당치 퇴장
        yield return StartCoroutine(HubUIManager.Instance.HideBottomUI(MiniGameManager.Instance.currentStage));

        // 2. 레터박스
        bool isLetterBoxDone = false;
        LetterBoxManager.Instance.ShowBars(() => isLetterBoxDone = true);
        yield return new WaitUntil(() => isLetterBoxDone);
        yield return new WaitForSeconds(0.1f);

        // 3. 큰 주댕치 입장
        yield return StartCoroutine(HubUIManager.Instance.ShowBigJudangchi());

        // 4. 주댕치 대사 및 표정 변화
        bool isDialogueDone = false;

        JudangChiDialogueManager.Instance.StartDialogue(dialogueData, () => isDialogueDone = true);

        yield return new WaitUntil(() => isDialogueDone);

        // 5. 큰 주댕치 퇴장
        yield return StartCoroutine(HubUIManager.Instance.HideBigJudangchi());

        // 6. 레터박스 치우기
        bool isLetterBoxHidden = false;
        LetterBoxManager.Instance.HideBars(() => isLetterBoxHidden = true);
        yield return new WaitUntil(() => isLetterBoxHidden);

        // ★ 7. 현재 스테이지가 1 이상이면 디지바이스 등장! (아니면 작은 주당치)
        yield return StartCoroutine(HubUIManager.Instance.ShowBottomUI(MiniGameManager.Instance.currentStage));

        MiniGameManager.Instance.EnablePlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
