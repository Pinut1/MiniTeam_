using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대사 한 줄 데이터
/// </summary>
[System.Serializable]
public class SpongeDialogueLine
{
    public string lineId;
    public string speaker; // "스폰지밥", "징징이", "플랑크톤", "플레이어" 등
    [TextArea(2, 5)] public string txt; // RichTxt 태그 사용 가능
    public string animationTrig; // Animator Trigger 이름, 비우면 이전 유지

    [Header("씬 / 화면 전환")]
    public Image backgroundImg; // 바꿀 배경, 비우면 이전 유지
    public Image charaterImg; // 바꿀 캐릭터, 비우면 이전 유지
    public CharacterPosition characterPos;
    public string sceneToLoad;

    public SpongeChoice[] choices; // null이면 선택지 X
    public string nextLineId; // 선택지 없을 때 다음 대사 ID

    public enum CharacterPosition
    {
        None, SpongeBob, Ddungi, JingJingi, Plankton, JipgeSajang, Player
    }
}
