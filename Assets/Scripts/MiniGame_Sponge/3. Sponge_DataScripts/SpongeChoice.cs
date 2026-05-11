using UnityEngine;

/// <summary>
/// 선택지 데이터
/// </summary>
[System.Serializable]
public class SpongeChoice
{
    [Header("선택지 버튼 텍스트")] public string choiceTxt;
    [Header("선택지 선택시 이동할 대사")] public string nextLineId;
}
