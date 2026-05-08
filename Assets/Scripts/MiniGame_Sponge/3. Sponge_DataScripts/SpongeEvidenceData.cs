using UnityEngine;

/// <summary>
/// 증거 아이템 데이터
/// </summary>
[System.Serializable]
public class SpongeEvidenceData
{
    [Header("코드에 참고할 고유 ID - 예) knaife")] public string id;
    [Header("증거 목록에 표시될 이름")] public string evidenceName;
    [Header("증거 상세 설명 - 증거 선택 시 표시")] [TextArea(2, 4)] public string description;
    [Header("증거 아이콘 이미지")] public Sprite icon;
}
