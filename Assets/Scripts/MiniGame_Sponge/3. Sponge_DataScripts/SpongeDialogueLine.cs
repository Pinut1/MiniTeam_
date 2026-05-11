using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대사 한 줄 데이터
/// </summary>
[System.Serializable]
public class SpongeDialogueLine
{
    [Header("대사 찾을 때 쓰는 ID - 겹치면 X")] public string lineId;

    [Header("대사창에 표시될 화자 이름")] public string speaker; // "스폰지밥", "징징이", "플랑크톤", "플레이어" 등
    [Header("실제 대사 사용 - RichText 태그 사용 가능")] [TextArea(2, 5)] public string txt; // RichTxt 태그 사용 가능 : <color=red></color>

    [Header("Animator Trigger 이름 - 비우면 이전 유지")] public string animationTrig;
    [Header("씬전환")] public string sceneToLoad;
    [Header("배경 / 캐릭터 전환")]
    public Sprite backgroundSpr; // 바꿀 배경, 비우면 이전 유지
    // public Sprite charaterSpr; // 바꿀 캐릭터, 비우면 이전 유지
    [Space(5f)] public CharacterPosition characterPos;

    [Header("선택지 - null이면 선택지 X")]
    public SpongeChoice[] choices;
    public string nextLineId; // 선택지 없을 때 다음 대사 ID
    
    public enum CharacterPosition
    {
        None, SpongeBob, Ddungi, JingJingi, Plankton, JipgeSajang, Player
    }
}
