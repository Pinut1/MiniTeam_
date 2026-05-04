using UnityEngine;
/// <summary>
/// 상태 enum
/// </summary>
public class SpongeGameState
{
    public enum GameState
    {
        Dialogue,              // 일반 대사 진행 중
        CrossExzamination, // 심문 모드 (증언 넘기기 / Q / TAB 가능)
        Pressing,              // 추궁 대사 출력
        EvidenceSelect,     // 증거 선택 창 열림
        Resolution          // 사건 해결 - 모든 입력 차단
    }
}
