using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpongeCrossExaminationManager : MonoBehaviour
{
    public static SpongeCrossExaminationManager Instance { get; private set; }

    [SerializeField] private SpongeTrialScriptSO trialScript;

    private SpongeTestimonyLine[] lines;
    private int currentIdx = 0;

    // 외부(EvidenceManager)에서 현재 증언 라인 참조용
    public SpongeTestimonyLine currentLine => lines[currentIdx];
    public int CurrentIdx => currentIdx;

    private void Awake()
    {
        Instance = this;
        lines = trialScript.testimonyLines;
    }

    // ── 심문 시작 ────────────────────────────────────────────
    public void StartCrossExamination()
    {
        currentIdx = 0;
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
        ShowCurrentTestimony();
    }

    /// <summary>
    /// 현재 증언 표시
    /// </summary>
    public void ShowCurrentTestimony()
    {
        // 증언 텍스트를 DialogueManager의 대사창에 표시
        // speaker는 증인 이름. txt는 증언 내용
        SpongeDialogueManager.Instance.ShowTestimonyLine(lines[currentIdx]);
    }

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

    public void PrevLine()
    {
        if (currentIdx > 0)
        {
            currentIdx--;
            ShowCurrentTestimony();
        }
    }

    public void PressWitness()
    {
        var line = lines[currentIdx];
        if (!line.ispressable)
        {
            SpongeDialogueManager.Instance.ShowLine("Press_fail_default");
            return;
        }

        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Pressing);
        
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
