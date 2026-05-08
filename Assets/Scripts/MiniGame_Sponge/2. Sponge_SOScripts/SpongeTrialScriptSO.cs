using UnityEngine;

/// <summary>
/// 대사 + 증언 통합 에셋
/// </summary>
[CreateAssetMenu(fileName = "SpongeTrialScriptSO", menuName = "Scriptable Objects/SpongeTrialScriptSO")]
public class SpongeTrialScriptSO : ScriptableObject
{
    // 게임 시작 시 재생되는 오프닝 대사 목록 / DialogueManager가 순서대로 읽어서 출력
    [Header("오프닝 대사")] public SpongeDialogueLine[] openingLines;

    // 심문 대상 증언 목록 / CrossExaminationManager
    [Header("증언 목록(심문 대상) - 순서 중요")] public SpongeTestimonyLine[] testimonyLines;

    // TestimonyLine.firstPressDialogueId로 첫 번째 대사를 찾고
    // 이후 nextLineId 체인으로 연결됨
    [Header("추궁 대사")] public SpongeDialogueLine[] pressDialogueLines;

    // 아래 고정 ID는 반드시 여기에 포함되어야 함:
    //   "evidence_fail_default" — 증거 제시 실패 시
    //   "press_fail_default"    — 추궁 불가 시
    //   "incomplete_warning"    — 필수 조건 미완료 시
    [Header("증거 제시 대사 + 고정 대사")] public SpongeDialogueLine[] evidenceDialogueLines;

    // 모든 조건 충족 시 재생되는 엔딩 대사
    // "ending_01"부터 시작해서 nextLineId 체인으로 연결됨
    [Header("엔딩 대사")] public SpongeDialogueLine[] endingLines;
}
