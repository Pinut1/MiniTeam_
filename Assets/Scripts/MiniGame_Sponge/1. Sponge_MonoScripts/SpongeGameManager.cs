using System;
using System.Collections.Generic;
using UnityEngine;
using static SpongeGameState;

public class SpongeGameManager : MonoBehaviour
{
    public static SpongeGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeRequiredConditions();
    }

    // ── 상태 관리 ────────────────────────────────────────────
    [Header("초기 상태")]
    [SerializeField] private SpongeGameState gmState;
    private GameState startState = GameState.Dialogue;
    private GameState currentState;

    public GameState CurrentState => currentState;

    /// <summary>
    /// 상태 전환
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"[GameManager] 상태 전환 -> {newState}");
    }

    public bool IsInputBlocked() => currentState == GameState.Resolution;
    public bool CanPress() => currentState == GameState.CrossExzamination;
    public bool CanOpenEvidence => currentState == GameState.Dialogue || currentState == GameState.CrossExzamination;

    // ── 진행 조건 (ProgeressTraccker) ────────────────────────────────────────────
    [Header("필수 완료 조건")]
    [SerializeField] private int[] requiredPressIndices; // 예 : {0, 2}
    [SerializeField] private string[] requiredEvidenceIds; // 예 : {"knife", "receipt"}

    private HashSet<int> completedPresses = new(); 
    private HashSet<string> completedEvidences = new();

    void InitializeRequiredConditions()
    {
        completedPresses.Clear();
        completedEvidences.Clear();
    }

    public void RegisterPress(int lineIdx)
    {
        completedPresses.Add(lineIdx);
        Debug.Log($"[GameManager] 추궁 완료 : {lineIdx}번 증언");
    }

    public void RegisterEvidence(string evidenceId)
    {
        completedEvidences.Add(evidenceId);
        Debug.Log($"[GameManager] 증거 제시 완료 :  {evidenceId}");
    }

    public bool IsAllConditionsMet()
    {
        foreach (int i in requiredPressIndices)
            if (completedPresses.Contains(i)) return false;
        foreach (string id in requiredEvidenceIds)
            if (completedEvidences.Contains(id)) return false;
        return true;
    }

    // 조건 충족 즉시 체크 - 필요 시 자동 Resolution 전환
    void CheckAllConditions()
    {

    }

    // ── 게임 시작 / 재시작 ───────────────────────────────────
    private void Start()
    {
        currentState = GameState.Dialogue;
        
    }
}
