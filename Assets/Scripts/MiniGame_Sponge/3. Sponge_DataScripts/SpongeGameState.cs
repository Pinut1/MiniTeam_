using UnityEngine;
/// <summary>
/// 상태 enum
/// GameManager가 이 값을 들고 모든 스크립트가 참조
/// </summary>
public class SpongeGameState
{
    public enum GameState
    {
        Dialogue,         // 일반 대사 진행 중
        Testifying,       // 증언 낭독 중 - 타이핑으로 표시, 좌우 화살표 없음
        CrossExamination, // 심문 모드 (증언 넘기기 / Q / TAB 가능)
        Pressing,         // 추궁 대사 출력
        EvidenceSelect,   // 증거 선택 창 열림
        Resolution        // 사건 해결 - 모든 입력 차단
    }
}
