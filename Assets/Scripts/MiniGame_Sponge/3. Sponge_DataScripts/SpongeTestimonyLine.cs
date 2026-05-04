using UnityEngine;

[System.Serializable]
public class SpongeTestimonyLine
{
    [TextArea(2, 5)] public string txt; // 증언 창에 표시될 텍스트

    [Header("추궁 설정")]
    public bool ispressable; // 추궁 가능 여부
    public string firstPressDialogueId; // 추궁 시 시작할 첫 대사 ID
    public bool isRequiredPress; // 필수 추궁 여부

    [Header("증거 제시 설정")]
    public string[] validEvidenceIds; // 정답 증거 ID 목록
    public string evidenceSuccessDialogueId; // 올바른 증거 제시 후 대사 ID
    public bool isRequiredEvidence; // 필수 증거 제시 여부
}
