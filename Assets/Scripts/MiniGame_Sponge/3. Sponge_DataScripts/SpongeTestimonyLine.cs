using UnityEngine;

/// <summary>
/// 증언 한 줄 데이터
/// </summary>
[System.Serializable]
public class SpongeTestimonyLine
{
    [Header("증언 대사창에 표시될 텍스트")]
    [TextArea(2, 5)] public string txt;

    [Header("추궁 가능 여부")] public bool ispressable;
    [Header("추궁시 시작할 첫 대사")] public string firstPressDialogueId;
    [Header("필수 추궁 여부")] public bool isRequiredPress;

    [Header("정답 증거 ID 목록")] public string[] validEvidenceIds;
    [Header("올바른 증거 제시 후 대사 ID")] public string evidenceSuccessDialogueId;
    [Header("필수 증거 제시 여부")] public bool isRequiredEvidence;
}
