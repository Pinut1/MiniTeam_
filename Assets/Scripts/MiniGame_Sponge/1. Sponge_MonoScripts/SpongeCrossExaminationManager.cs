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
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExzamination);
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        // 증언 텍스트를 DialogueManager의 대사창에 표시
        // speaker는 증인 이름. txt는 증언 내용
    }
}
