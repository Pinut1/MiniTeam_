using UnityEngine;

/// <summary>
/// 증거 아이템 데이터
/// </summary>
[System.Serializable]
public class SpongeEvidenceData
{
    public string id; // 코드에 참고할 고유 ID 예 : "knife"
    public string evidenceName; // 증거 목록에 표시될 이름
    [TextArea(2, 4)] public string description; // 증거 설명
    public Sprite icon;  // 증거 아이콘 이미지
}
