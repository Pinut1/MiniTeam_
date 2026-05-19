using System;
using UnityEngine;

/// <summary>
/// 증거 제시 판정을 담당
/// 1. 플레이어가 선택한 증거가 현재 증언 라인의 정답인지 확인
/// 2. 성공 시 -> 필수 조건이면 GameManager에 등록 -> 성공 대사 출력
/// 3. 실패 시 -> 실패 고정 대사 출력
/// 4. 증거 데이터 조회 (UIManager에서 목록 생성 시 사용)
/// </summary>
public class SpongeEvidenceManager : MonoBehaviour
{
    public static SpongeEvidenceManager Instance { get; private set; }

    private SpongeEvidenceData[] evidences;

    [Serializable]
    private class EvidenceDatabaseWrapper
    {
        public SpongeEvidenceData[] evidences;
    }

    // 증거 상태가 바뀔 때 구독자들에게 알려주는 이벤트
    public static event Action<string> OnEvidenceSelected; // 증거 선택됨
    public static event Action OnEvidenceListChanged; // 목록 변경됨

    public string SelectedEvidenceId { get; private set; }

    private void Awake()
    {
        Instance = this;
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        var json = Resources.Load<TextAsset>("SpongeData/SpongeEvidenceDatabase");
        evidences = JsonUtility.FromJson<EvidenceDatabaseWrapper>(json.text).evidences;
    }

    // 증거 슬롯 첫 클릭 시 호출 — 하이라이트 + 상세 이미지 표시
    public void SelectEvidence(string evidenceId)
    {
        SelectedEvidenceId = evidenceId;
        OnEvidenceSelected?.Invoke(evidenceId);
    }

    // 패널 닫힐 때 선택 초기화
    public void ClearSelection()
    {
        SelectedEvidenceId = null;
    }

    // ── 증거 제시 ────────────────────────────────────────────
    // UIManager의 증거 목록에서 증거를 선택하면 UIManager에서 메서드 호출
    // evidenceId : 선택한 증거의 id (예: "knife", "receipt")
    /// <summary>
    /// 증거 제시
    /// </summary>
    /// <param name="evidenceId"></param>
    public void PresentEvidence(string evidenceId)
    {
        // CrossExaminationManager에서 현재 증언 라인을 가져옴
        // 이 라인의 validEvidenceIds와 비교해서 정답 여부 판단
        var currentLine = SpongeCrossExaminationManager.Instance.CurrentLine;

        // 현재 증언 라인의 정답 증거 목록에서 선택한 증거가 있는지 확인
        // Array.Exists = 배열에서 조건에 맞는 요소가 하나라도 있으면 true 반환
        bool isCorrect = System.Array.Exists(currentLine.validEvidenceIds, // 정답 증거 ID 목록
                                                        id => id == evidenceId);    // 선택한 증거 ID와 같은지 비교
        
        // 증거 패널 닫기 - 성공/실패 상관 없이 먼저 닫음
        SpongeUIManager.Instance.CloseEvidencePanel();

        if (isCorrect)
        {
            // ── 정답 증거 제시 성공 ──────────────────────────
            // RegisterEvidence() 안에서 conditionJustMet 체크도 같이 됨
            if (currentLine.isRequiredEvidence)
                SpongeGameManager.Instance.RegisterEvidence(evidenceId);
            // 증거 제시 대사 상태로 전환
            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.EvidenceSelect);

            SpongeCrossExaminationManager.Instance.OnEvidenceResolved(true);

            // 성공 대사 출력
            // 대사가 끝나면 DialogueManager.OnSequenceEnd()가 자동으로 호출됨
            // → OnSequenceEnd()에서 conditionJustMet 체크 후 엔딩 or 심문 복귀
            SpongeDialogueManager.Instance.ShowLine(currentLine.evidenceSuccessDialogueId);
        }
        else
        {
            // ── 증거 제시 실패 ───────────────────────────────

            SpongeCrossExaminationManager.Instance.OnEvidenceResolved(false);

            // 실패해도 심문 상태는 유지 (CrossExamination 상태 그대로)
            // 실패 고정 대사 출력
            // "evidence_fail_default" 는 TrialScriptSO에 반드시 입력해야 함
            SpongeDialogueManager.Instance.ShowLine("evidence_fail_default_01");
        }
    }

    // ── 증거 데이터 조회 ─────────────────────────────────────
    // id로 증거 데이터를 찾아서 반환
    // UIManager에서 증거 목록 UI를 만들 때 사용
    /// <summary>
    /// 증거 데이터 조회
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SpongeEvidenceData GetById(string id)
    {
        return System.Array.Find(evidences, e => e.id == id);
    }

    // 전체 증거 목록 반환
    // UIManager에서 증거 버튼을 생성할 때 사용
    /// <summary>
    /// 전체 증거 목록 반환
    /// </summary>
    /// <returns></returns>
    public SpongeEvidenceData[] GetAllEvidences()
    {
        return evidences;
    }
}
