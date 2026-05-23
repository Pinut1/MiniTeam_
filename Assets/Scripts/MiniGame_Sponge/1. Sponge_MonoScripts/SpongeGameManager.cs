using System;
using System.Collections.Generic;
using UnityEngine;
using static SpongeGameState;

/// <summary>
/// 전체 게임 상태 관리하는 핵심
/// 1. 현재 GameState를 들고 있으면서 상태 전환 관리
/// 2. 필수 추궁/증거 제시 조건 완료 여부 추적
/// </summary>
public class SpongeGameManager : MonoBehaviour
{
    public static SpongeGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeRequiredConditions();
    }

    // ── 상태 관리 ────────────────────────────────────────────
    //[Header("초기 상태")]
    //[SerializeField] private SpongeGameState gmState;

    // 현재 게임 상태
    private GameState currentState;
    // currentState를 읽을 수 있게 공개 
    public GameState CurrentState => currentState;

    /// <summary>
    /// 게임 상태 전환 - 모든 상태 전환은 반드시 이 메서드를 통해서 해야함
    /// </summary>
    /// <param name="newState"></param>
    public static event System.Action<GameState> OnStateChanged;

    public void ChangeState(GameState newState)
    {
        // 현재 상태와 같으면 전환 X
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"[GameManager] 상태 전환 -> {newState}");
        OnStateChanged?.Invoke(newState);
    }

    // 현재 상태에서 입력을 완전히 차단해야 하는지
    // Resolution(게임 클리어) 상태에서만 true
    public bool IsInputBlocked() =>
        currentState == GameState.Resolution ||
        (SpongeFadeManager.Instance != null && SpongeFadeManager.Instance.IsFading) ||
        (SpongeUIManager.Instance != null && SpongeUIManager.Instance.IsPlayingHoldit) ||
        (SpongeUIManager.Instance != null && SpongeUIManager.Instance.IsPlayingObjection) ||
        (SpongeUIManager.Instance != null && SpongeUIManager.Instance.IsPlayingTakeThat) ||
        (SpongeUIManager.Instance != null && SpongeUIManager.Instance.IsPlayingInnocence);

    // Q키(추궁하기)를 누를 수 있는 상태인지
    // CrossExamination(심문) 상태에서만 true
    public bool CanPress() => currentState == GameState.CrossExamination;

    public bool HasPressedTestimony(int idx) => completedPresses.Contains(idx);

    // ─프로퍼티를 이용해 캡슐화 (실시간으로 상태를 확인하여 결과를 반환)─
    // TAB키(증거 목록)를 열 수 있는 상태인지
    // 일반 대사 중이거나 심문 중일때만 가능
    public bool CanOpenEvidence => currentState == GameState.Dialogue || currentState == GameState.CrossExamination;

    // ── 진행 조건 (ProgeressTraccker) ────────────────────────────────────────────
    [Header("1. 필수 추궁")]
    [SerializeField] private int[] requiredPressIndices; // 예 : {0, 2}, n번째 증언은 반드시 추궁 해야함
    [Header("1. 필수 제시 증거")]
    [SerializeField] private string[] requiredEvidenceIds; // 예 : {"knife", "receipt"}

    [Header("2. 필수 추궁")]
    [SerializeField] private int[] requiredRetestimonyPressIndices;
    [Header("2. 필수 제시 증거")]
    [SerializeField] private string[] requiredRetestimonyEvidenceIds;

   // 완료 목록 - 라운드별 분리
    private HashSet<int> completedPresses = new();
    private HashSet<string> completedEvidences = new();
    private HashSet<int> completedRetestimonyPresses = new();
    private HashSet<string> completedRetestimonyEvidences = new();

    // 추궁 또는 증거 제시로 조건이 방금 충족 됐는지 저장
    // RegisterPress / RegisterEvidence 호출 시 자동으로 갱신됨
    // ConsumeConditionMet()으로 한 번만 읽을 수 있음
    private bool conditionJustMet = false;

    [Header("몇번째 심문인지")]
    public int CurrentRound { get; private set; } = 1; // CrossExaminationManager에서 라운드 전환 시 변경

    public void SetRound(int round)
    {
        CurrentRound = round;
        Debug.Log($"[GameManager] 라운드 전환 -> {round}라운드");
    }

    /// <summary>
    /// 게임 시작 시 완료 목록 초기화
    /// </summary>
    void InitializeRequiredConditions()
    {
        completedPresses.Clear();
        completedEvidences.Clear();
        completedRetestimonyPresses.Clear();
        completedRetestimonyEvidences.Clear();
        conditionJustMet = false;
    }

    /// <summary>
    /// 추궁 완료 등록
    /// </summary>
    /// <param name="lineIdx"></param>
    public void RegisterPress(int lineIdx)
    {
        if (CurrentRound == 1)
            completedPresses.Add(lineIdx);
        else
            completedRetestimonyPresses.Add(lineIdx);
        Debug.Log($"[GameManager] 추궁 완료 : {lineIdx}번 증언");
        conditionJustMet = IsAllConditionsMet();
    }

    /// <summary>
    /// 증거 제시 완료 등록
    /// </summary>
    /// <param name="evidenceId"></param>
    public void RegisterEvidence(string evidenceId)
    {
        if (CurrentRound == 1)
            completedEvidences.Add(evidenceId);
        else
            completedRetestimonyEvidences.Add(evidenceId);
        Debug.Log($"[GameManager] 증거 제시 완료 :  {evidenceId}");
        conditionJustMet = IsAllConditionsMet();
    }

    /// <summary>
    /// 모든 필수 조건이 충족 됐는지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsAllConditionsMet()
    {
        bool allMet = true;

        if (CurrentRound == 1)
        {
            foreach (int i in requiredPressIndices)
                if (!completedPresses.Contains(i))
                {
                    Debug.Log($"남은 필수 추궁 : {i}");
                    allMet = false;
                }
            foreach (string id in requiredEvidenceIds)
                if (!completedEvidences.Contains(id))
                {
                    Debug.Log($"남은 필수 증거 제시 : {id}");
                    allMet = false;
                }
        }
        else
        {
            foreach (int i in requiredRetestimonyPressIndices)
                if (!completedRetestimonyPresses.Contains(i))
                {
                    Debug.Log($"남은 필수 추궁 : {i}");
                    allMet = false;
                }
            foreach (string id in requiredRetestimonyEvidenceIds)
                if (!completedRetestimonyEvidences.Contains(id))
                {
                    Debug.Log($"남은 필수 증거 제시 : {id}");
                    allMet = false;
                }
        }

        return allMet;
    }

    /// <summary>
    /// ConsumeConditionMet을 한 번만 읽고 즉시 false로 초기화
    /// DialogueManager.OnSequenceEnd()에서 엔딩 여부 판단 시 사용
    /// "방금 조건이 충족됐어?" 라고 한 번 물어보면 자동으로 리셋
    /// </summary>
    /// <returns></returns>
    public bool ConsumeConditionMet()
    {
        bool result = conditionJustMet; // 현재 값을 임시 저장
        conditionJustMet = false; // 한 번 읽으면 초기화
        return result; // 저장해뒀던 값 반환
    }

    public void SkipAllRequiredConditions()
    {
        if (CurrentRound == 1)
        {
            foreach (int i in requiredPressIndices)   completedPresses.Add(i);
            foreach (string id in requiredEvidenceIds) completedEvidences.Add(id);
        }
        else
        {
            foreach (int i in requiredRetestimonyPressIndices)    completedRetestimonyPresses.Add(i);
            foreach (string id in requiredRetestimonyEvidenceIds) completedRetestimonyEvidences.Add(id);
        }
    }

    // ── 게임 시작 / 재시작 ───────────────────────────────────
    private void Start()
    {
        // 상태 Dialogue로 변경
        currentState = GameState.Dialogue;
        // 오프닝 첫 대사 호출
        SpongeDialogueManager.Instance.ShowLine("opening_01");
    }
}
