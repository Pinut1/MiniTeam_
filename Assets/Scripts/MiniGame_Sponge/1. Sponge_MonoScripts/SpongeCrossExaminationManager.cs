using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 심문 진행 담당
/// 1. 증언 배열을 인덱스로 순회 (앞/뒤 이동, 마지막에서 처음으로 루프)
/// 2. Q키 추궁 처리 — 추궁 가능 여부 확인 후 대사 시작
/// 3. 현재 증언 라인을 DialogueManager를 통해 대사창에 표시
/// </summary>
public class SpongeCrossExaminationManager : MonoBehaviour
{
    public static SpongeCrossExaminationManager Instance { get; private set; }

    // 재판 대본 에셋
    [SerializeField] private SpongeTrialScriptSO trialScript;

    // 증언 배열 - TrialScriptSO.testimonyLines를 복사해서 사용
    private SpongeTestimonyLine[] lines;
    private int currentIdx = 0; // 증언 인덱스

    // 외부(EvidenceManager)에서 현재 증언 라인 참조용
    public SpongeTestimonyLine currentLine => lines[currentIdx];
    public int CurrentIdx { get => currentIdx; set => currentIdx = value; }
    private void Awake()
    {
        Instance = this;
        // TrialScriptSO에서 증언 배열 가져오기
        lines = trialScript.testimonyLines;
    }

    // ── 심문 시작 ────────────────────────────────────────────
    /// <summary>
    /// 오프닝 대사가 끝나면 DialogueManager.OnSequenceEnd()에서 호출
    /// 인덱스를 0으로 초기화하고 첫 번째 증언 표시
    /// </summary>
    public void StartCrossExamination()
    {
        // 인덱스 초기화 - 항상 첫번째 증언부터 시작
        currentIdx = 0;
        // 심문 상태로 전환 - Q/TAB 키 활성화
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
        // 첫번쩨 증언 표시
        ShowCurrentTestimony();
    }

    // ── 현재 증언 표시 ───────────────────────────────────────────
    /// <summary>
    /// 현재 인덱스의 증언을 대사창에 표시
    /// DialogueManager.ShowTestimonyLine()을 통해 즉시 표시 (타이핑 X)
    /// 호출 하는 곳 : StartCrossExamination() : 심문 시작 시, NextLine() / PrevLine() : 증언 이동 시 / DialogueManager.OnSequenceEnd() : 추궁/증거 대사 끝나고 복귀 시
    /// </summary>
    public void ShowCurrentTestimony()
    {
        // 증언 텍스트를 DialogueManager의 대사창에 표시
        // speaker는 증인 이름. txt는 증언 내용
        SpongeDialogueManager.Instance.ShowTestimonyLine(lines[currentIdx]);
    }

    // ── 다음 증언으로 ────────────────────────────────────────────
    /// <summary>
    /// 클릭 시 다음 증언으로 이동
    /// 마지막 증언에서 클릭하면 처음으로
    /// UIManager에서 클릭 이벤트 수신 시 호출
    /// </summary>
    public void NextLine()
    {
        // 마지막 증언이 아니므로 다음으로
        if (currentIdx < lines.Length - 1)
        {
            currentIdx++;
            ShowCurrentTestimony();
        }
        else
        {
            // 마지막에서 처음으로 루프
            currentIdx = 0;
            ShowCurrentTestimony();
        }
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Dialogue);
        SpongeDialogueManager.Instance.ShowLine("ending_01");
    }
    // ── 이전 증언으로 ────────────────────────────────────────────
    /// <summary>
    /// 이전 증언으로 이동
    /// 첫번째 증언에서는 더이상 이동X
    /// </summary>
    public void PrevLine()
    {
        // 첫번째 증언이 아닐때만 이동
        if (currentIdx > 0)
            currentIdx--;
        ShowCurrentTestimony();
    }

    // ── 추궁하기 (Q키) ───────────────────────────────────────────
    /// <summary>
    /// Q키 입력시 UIManager에서 호출
    /// 현재 증언이 추궁 가능한지 확인 후 추궁 대사 시작
    /// </summary>
    public void PressWitness()
    {
        var line = lines[currentIdx];

        // 추궁 불가능 증언 - 고정 대사 출력 후 종료
        if (!line.ispressable)
        {
            SpongeDialogueManager.Instance.ShowLine("Press_fail_default");
            return;
        }

        // 추궁 상태로 전환
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Pressing);
        
        // 필수 추궁인 경우 완료 등록
        if (line.isRequiredEvidence)
        {
            SpongeGameManager.Instance.RegisterPress(currentIdx);

            if (SpongeGameManager.Instance.IsAllConditionsMet())
            {
                // 추궁 대사 끝나면 OnsquenceEnd()가 엔딩으로 보냄
                // -> OnSequenceEnd()의 Pressing 케이스에서 처리    
            }
        }
        // 추궁 대사 시작 - 대사 끝난 뒤 OnSequenceEnd()에서 조건 체크
        SpongeDialogueManager.Instance.ShowLine(line.firstPressDialogueId);
    }

    public void OnEivdenceResolved(bool sucess)
    {
        if (sucess)
            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Pressing);
        else
            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
    }
}
