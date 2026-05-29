using MiniTeam.Core;
using System;
using System.Collections;
using UnityEngine;
using MiniTeam.Pokemon; // 추가된 네임스페이스

public class JudangChiController : MonoBehaviour
{
    public static JudangChiController Instance { get; private set; }

    // [Header("대사 데이터 키 (JSON 연동)")]
    // [Tooltip("인덱스 0: 게임 시작시, 1: 1스테이지 클리어 후...")]
    // public DialogueData[] stageDialogues; (SO 데이터 제거)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 씬 시작 시 Hub 전용 대사 데이터 로드
        DialogueDB.Instance.Load("Hub");
    }

    // 최초 허브에서 주댕치-> 주댕치 대화 씬 진입 제어
    #region FirstScene
    public void PlaySequenceForFirstStage()
    {
        int currentStage = MiniGameManager.Instance.currentStage;

        string countStr = DialogueDB.Instance.Get($"hub_stage{currentStage}_count");
        if (!int.TryParse(countStr, out int count) || count == 0) return;

        StartCoroutine(FirstSequenceRoutine($"hub_stage{currentStage}"));

    }

    private IEnumerator FirstSequenceRoutine(string dialogueKeyPrefix)
    {
        MiniGameManager.Instance.DisablePlayerInput();

        HubUIManager.Instance.FirstCinemaEnter();

        // 1. 시네마틱 입장 (하단UI 내려감 + 레터박스 나옴 + 큰 주댕치 올라옴)
        yield return new WaitUntil(() => HubUIManager.Instance.isFirstCinemaEnterDone);
        yield return new WaitForSeconds(0.2f);

        // 2. 대사 진행
        bool isDialogueDone = false;
        JudangChiDialogueManager.Instance.StartDialogue(dialogueKeyPrefix, () => isDialogueDone = true);
        yield return new WaitUntil(() => isDialogueDone);

        // 3. 시네마틱 퇴장 (큰 주댕치 내려감 + 레터박스 들어감 + 상황에 맞는 하단UI 올라옴)
        yield return StartCoroutine(HubUIManager.Instance.FirstCinemaExit(MiniGameManager.Instance.currentStage));

        // 컷신 감상 완료 표시 및 느낌표 제거
        MiniGameManager.Instance.SetCutscenePlayed(true);

        MiniGameManager.Instance.EnablePlayerInput();
    }

    #endregion

    // 디지바이스를 얻은 이후 디지바이스 -> 주댕치 대화 씬 진입 제어
    #region DigiviceScene
    public void PlaySequenceForCurrentStage()
    {
        int currentStage = MiniGameManager.Instance.currentStage;
        
        string countStr = DialogueDB.Instance.Get($"hub_stage{currentStage}_count");
        if (!int.TryParse(countStr, out int count) || count == 0) return;
       
        StartCoroutine(NomalSequenceRoutine($"hub_stage{currentStage}"));
    }

    private IEnumerator NomalSequenceRoutine(string dialogueKeyPrefix)
    {
        MiniGameManager.Instance.DisablePlayerInput();

        HubUIManager.Instance.PlayCinemaEnter();
        // 1. 시네마틱 입장 (하단UI 내려감 + 레터박스 나옴 + 큰 주댕치 올라옴)
        yield return new WaitUntil(() => HubUIManager.Instance.isNormalCinemaEnterDone);
        yield return new WaitForSeconds(0.2f);

        // 2. 대사 진행
        bool isDialogueDone = false;
        JudangChiDialogueManager.Instance.StartDialogue(dialogueKeyPrefix, () => isDialogueDone = true);
        yield return new WaitUntil(() => isDialogueDone);

        bool isFinalStage = MiniGameManager.Instance.currentStage == 5;
        Debug.Log($"{isFinalStage}");

        if (isFinalStage)
        {
            // 마지막 대사였다면 평소처럼 퇴장하지 않고, 엔딩 시퀀스로 진입합니다.
            yield return StartCoroutine(HubUIManager.Instance.PlayCinemaExit(MiniGameManager.Instance.currentStage));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(PlayEndingSequence());
        }
        else
        {
            // 평소라면 얌전히 퇴장하고 플레이어에게 조작권을 돌려줍니다.
            yield return StartCoroutine(HubUIManager.Instance.PlayCinemaExit(MiniGameManager.Instance.currentStage));
            MiniGameManager.Instance.EnablePlayerInput();
        }

        // 컷신 감상 완료 표시 및 느낌표 제거
        MiniGameManager.Instance.SetCutscenePlayed(true);

    }
    #endregion



    // 게임 클리어 후 주댕치 대화 씬 바로 진입
    #region AfterGameClear
    internal void PlaySequenceForGameClear()
    {
        int currentStage = MiniGameManager.Instance.currentStage;

        string countStr = DialogueDB.Instance.Get($"hub_stage{currentStage}_count");
        if (!int.TryParse(countStr, out int count) || count == 0)
        {
            // 데이터가 없어도 게임 진행을 위해 필수 UI 갱신 및 조작권 반환 처리
            HubUIManager.Instance?.InitializeBottomUI(currentStage);
            MiniGameManager.Instance.EnablePlayerInput();
            return;
        }

        StartCoroutine(GameClearSequenceRoutine($"hub_stage{currentStage}"));
    }

    private IEnumerator GameClearSequenceRoutine(string dialogueKeyPrefix)
    {
        yield return new WaitForSeconds(0.5f);
        MiniGameManager.Instance.DisablePlayerInput();

        HubUIManager.Instance.StageClear_ObjectGet();
        
        // StageClear_ObjectGet 연출이 대략 2.5초간 진행된다고 가정하고 대기
        yield return new WaitForSeconds(2.2f);
        
        MiniGameManager.Instance.EnablePlayerInput();
    }
    #endregion



    private IEnumerator PlayEndingSequence()
    {
        // 엔딩 씬 진입 시 허브 BGM 중단
        MiniTeam.Core.AudioManager.Instance?.StopBGM();

        // (선택) 여기서 화면을 천천히 까맣게 페이드아웃 시키는 UI 연출을 넣으면 맛있습니다.
        //  yield return StartCoroutine(HubUIManager.Instance.PlayFadeOut());

        Debug.Log("모든 스테이지 클리어! 엔딩 씬으로 진입합니다.");

        yield return new WaitForSeconds(1f);

        // 씬 전환이라는 무거운 작업은 Controller가 직접 하지 않고 Manager에게 '위임'합니다.
        MiniGameManager.Instance.LoadEndingScene();
    }

}
